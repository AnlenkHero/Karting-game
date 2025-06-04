using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class SteeringWheelMenu : MonoBehaviour
    {
        [Header("1) The steerable wheel graphic (rotates around Z).")]
        [Tooltip("Drag in the RectTransform of your actual steering‐wheel GameObject (the UI Image you rotate).")]
        [SerializeField] private RectTransform wheelRect;

        [Header("2) The 3 UI slots (Left, Center, Right).")]
        [Tooltip("The Button that sits on the LEFT side of the wheel.")]
        [SerializeField] private Button leftSlotButton;

        [Tooltip("The Button that sits in the CENTER (selected) of the wheel).")]
        [SerializeField] private Button centerSlotButton;

        [Tooltip("The Button that sits on the RIGHT side of the wheel).")]
        [SerializeField] private Button rightSlotButton;

        [Header("3) All of your SteeringButtonData entries (with isVisible flags).")]
        [Tooltip(
            "Populate this array with every possible wheel‐button (e.g. Start, Quit, Options, etc.).\n" +
            "Each entry has a targetAngle (0..360) and a reference to a preconfigured Button prefab/scene‐Button."
        )]
        [SerializeField] private SteeringButtonData[] allButtonData;

        [Header("(Optional) Show the name of the currently selected button here.")]
        [SerializeField] private TextMeshProUGUI multimediaText;
        [SerializeField] private RadialDragRotate radialDragRotate;
        
        private Button[] _slotButtons = new Button[3];
        private int _buttonCount;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (wheelRect == null ||
                leftSlotButton == null ||
                centerSlotButton == null ||
                rightSlotButton == null ||
                allButtonData == null ||
                allButtonData.Length < 1)
            {
                Debug.LogError("SteeringWheelMenu: You must assign wheelRect, the 3 slot Buttons, and at least one SteeringButtonData.");
                enabled = false;
                return;
            }

            _slotButtons[0] = leftSlotButton;
            _slotButtons[1] = centerSlotButton;
            _slotButtons[2] = rightSlotButton;

            _buttonCount = allButtonData.Length;
        }

        private void Start()
        {
            UpdateSlotsBasedOnWheelRotation();
        }

        private void Update()
        {
            UpdateSlotsBasedOnWheelRotation();
        }
        
        private void UpdateSlotsBasedOnWheelRotation()
        {
            List<int> visibleIndices = new List<int>(_buttonCount);
            for (int i = 0; i < _buttonCount; i++)
            {
                if (allButtonData[i].isVisible)
                    visibleIndices.Add(i);
            }

            int visibleCount = visibleIndices.Count;
            
            var selectedListPos = SelectDataIndex(visibleCount, visibleIndices, out var dataIndexSelected);

            if (multimediaText != null)
            {
                if (radialDragRotate.isAutoAnimating)
                {
                    multimediaText.text = "USE STEERING WHEEL TO SELECT OPTIONS";
                }
                else
                {
                    multimediaText.text = allButtonData[dataIndexSelected].buttonName;   
                }
            }
            
            if (HandleInvisibleButtons(visibleCount, dataIndexSelected, selectedListPos, visibleIndices)) return;

            AssignButtons(selectedListPos, visibleCount, visibleIndices);
        }

        private void AssignButtons(int selectedListPos, int vCount, List<int> visibleIndices)
        {
            int leftListIndex  = (selectedListPos - 1 + vCount) % vCount;
            int rightListIndex = (selectedListPos + 1) % vCount;

            int dataIndexLeft   = visibleIndices[leftListIndex];
            int dataIndexCenter = visibleIndices[selectedListPos];
            int dataIndexRight  = visibleIndices[rightListIndex];

            leftSlotButton.gameObject.SetActive(true);
            centerSlotButton.gameObject.SetActive(true);
            rightSlotButton.gameObject.SetActive(true);

            CopyTemplateButtonIntoSlot(allButtonData[dataIndexLeft].button,   leftSlotButton);
            CopyTemplateButtonIntoSlot(allButtonData[dataIndexCenter].button, centerSlotButton);
            CopyTemplateButtonIntoSlot(allButtonData[dataIndexRight].button,  rightSlotButton);
        }

        private bool HandleInvisibleButtons(int vCount, int dataIndexSelected, int selectedListPos, List<int> visibleIndices)
        {
            if (vCount == 0)
            {
                leftSlotButton.gameObject.SetActive(false);
                centerSlotButton.gameObject.SetActive(false);
                rightSlotButton.gameObject.SetActive(false);

                if (multimediaText != null)
                    multimediaText.text = string.Empty;

                return true;
            }
            
            if (vCount == 1)
            {
                leftSlotButton.gameObject.SetActive(false);
                rightSlotButton.gameObject.SetActive(false);

                centerSlotButton.gameObject.SetActive(true);
                CopyTemplateButtonIntoSlot(allButtonData[dataIndexSelected].button, centerSlotButton);
                return true;
            }
            
            if (vCount == 2)
            {
                int otherListPos = (selectedListPos == 0) ? 1 : 0;
                int dataIndexOther = visibleIndices[otherListPos];

                leftSlotButton.gameObject.SetActive(true);
                centerSlotButton.gameObject.SetActive(true);
                rightSlotButton.gameObject.SetActive(false);

                CopyTemplateButtonIntoSlot(allButtonData[dataIndexOther].button, leftSlotButton);
                CopyTemplateButtonIntoSlot(allButtonData[dataIndexSelected].button, centerSlotButton);
                return true;
            }

            return false;
        }

        private int SelectDataIndex(int vCount, List<int> visibleIndices, out int dataIndexSelected)
        {
            float rawZ = wheelRect.eulerAngles.z;
            float normalizedZ = ((rawZ % 360f) + 360f) % 360f;
            
            int selectedListPos = 0;
            float smallestDelta = 360f;
            for (int listPos = 0; listPos < vCount; listPos++)
            {
                int dataIdx = visibleIndices[listPos];
                float dataAngle = allButtonData[dataIdx].targetAngle;
                float delta = Mathf.Abs(Mathf.DeltaAngle(normalizedZ, dataAngle));
                if (delta < smallestDelta)
                {
                    smallestDelta = delta;
                    selectedListPos = listPos;
                }
            }
            dataIndexSelected = visibleIndices[selectedListPos];
            return selectedListPos;
        }

        private void CopyTemplateButtonIntoSlot(Button templateButton, Button slotButton)
        {
            if (templateButton == null || slotButton == null)
            {
                if (slotButton != null)
                    slotButton.gameObject.SetActive(false);
                return;
            }

            Image templateImg = templateButton.GetComponent<Image>();
            Image slotImg  = slotButton.GetComponent<Image>();
            if (templateImg != null && slotImg != null)
            {
                slotImg.sprite = templateImg.sprite;
                slotImg.color  = templateImg.color;
            }
            
            slotButton.transition    = templateButton.transition;
            slotButton.colors        = templateButton.colors;
            slotButton.navigation    = templateButton.navigation;
            slotButton.targetGraphic = slotImg;
            
            slotButton.onClick.RemoveAllListeners();
            
            slotButton.onClick.AddListener(() =>
            {
                templateButton.onClick.Invoke();
            });
            
            slotButton.name = $"Slot[{templateButton.name}]";
        }
    }
}
