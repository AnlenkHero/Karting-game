using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class SteeringWheelMenu : MonoBehaviour
    {
        [Header("1) The steerable wheel graphic (rotates around Z).")]
        [Tooltip("Drag in the RectTransform of your actual steering‐wheel GameObject.")]
        [SerializeField] private RectTransform wheelRect = null;

        [Header("2) The 3 UI slots (Left, Center, Right).")]
        [Tooltip("The Button that sits on the LEFT side of the wheel.")]
        [SerializeField] private Button leftSlotButton = null;

        [Tooltip("The Button that sits in the CENTER (selected) of the wheel).")]
        [SerializeField] private Button centerSlotButton = null;

        [Tooltip("The Button that sits on the RIGHT side of the wheel.")]
        [SerializeField] private Button rightSlotButton = null;

        [Header("3) All of your SteeringButtonData entries in the order you want them to cycle.")]
        [Tooltip("Populate this array with every possible wheel‐button (e.g. Start, Quit, Options, etc.).\n" +
                 "Each entry has its own targetAngle (0..360), icon, name, onClick event.")]
        [SerializeField] private SteeringButtonData[] allButtonData = null;


        // Internal: convenience array of exactly the three visible Buttons:
        private Button[] _visibleButtons = new Button[3];

        // Cache the number of data entries:
        private int _buttonCount;


        private void Awake()
        {
            if (wheelRect == null ||
                leftSlotButton == null ||
                centerSlotButton == null ||
                rightSlotButton == null ||
                allButtonData == null ||
                allButtonData.Length < 1)
            {
                Debug.LogError("SteeringWheelMenu: You must assign wheelRect, 3 slot Buttons, and at least one SteeringButtonData.");
                enabled = false;
                return;
            }

            // Fill our visible‐button array in the same order:
            _visibleButtons[0] = leftSlotButton;
            _visibleButtons[1] = centerSlotButton;
            _visibleButtons[2] = rightSlotButton;

            _buttonCount = allButtonData.Length;
        }

        private void Start()
        {
            // On startup, do one pass so the UI is correct at frame zero
            UpdateSlotsBasedOnWheelRotation();
        }

        private void Update()
        {
            UpdateSlotsBasedOnWheelRotation();
        }

        /// <summary>
        /// Reads wheelRect.eulerAngles.z, finds which SteeringButtonData.targetAngle is closest,
        /// then populates left/center/right slots with the correct Sprites & onClick listeners.
        /// </summary>
        private void UpdateSlotsBasedOnWheelRotation()
        {
            // 1) Get wheel Z rotation in [0..360)
            float rawZ = wheelRect.eulerAngles.z;
            float normalizedZ = ((rawZ % 360f) + 360f) % 360f;

            // 2) Find the index in allButtonData whose targetAngle is closest to normalizedZ
            int selectedIndex = 0;
            float smallestAngleDelta = 360f;

            for (int i = 0; i < _buttonCount; i++)
            {
                float dataAngle = allButtonData[i].targetAngle;
                // Use DeltaAngle so that e.g. comparing 359° vs 1° returns a delta of 2°, not 358°.
                float delta = Mathf.Abs(Mathf.DeltaAngle(normalizedZ, dataAngle));
                if (delta < smallestAngleDelta)
                {
                    smallestAngleDelta = delta;
                    selectedIndex = i;
                }
            }

            // 3) Now compute the “left neighbor” and “right neighbor” indices in a circular fashion:
            int leftIndex = (selectedIndex - 1 + _buttonCount) % _buttonCount;
            int rightIndex = (selectedIndex + 1) % _buttonCount;

            // 4) Build a tiny array [leftData, centerData, rightData]:
            SteeringButtonData[] dataToShow = new SteeringButtonData[3]
            {
                allButtonData[leftIndex],
                allButtonData[selectedIndex],
                allButtonData[rightIndex]
            };

            // 5) For each of the 3 UI slot Buttons: assign sprite + onClick
            //    First clear out any old listeners so we don’t stack them:
            for (int slot = 0; slot < 3; slot++)
            {
                Button uiBtn = _visibleButtons[slot];
                SteeringButtonData data = dataToShow[slot];

                // a) Set the icon (if you have an Image component on the Button)
                Image img = uiBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = data.icon;
                    img.color = Color.white; // or whatever default tint you want
                }

                // b) Update the onClick listener:
                uiBtn.onClick.RemoveAllListeners();
                if (data.onClick != null)
                {
                    // If you want each SteeringButtonData to fire its UnityEvent when clicked:
                    uiBtn.onClick.AddListener(() => { data.onClick.Invoke(); });
                }
                
                uiBtn.name = "Slot[" + slot + "]_" + data.buttonName;
            }
        }
    }
}
