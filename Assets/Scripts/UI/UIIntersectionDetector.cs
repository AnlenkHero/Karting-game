using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class UIIntersectionDetector : MonoBehaviour
    {
        [Header("References")] 
        public RectTransform rotatingRect;
        public Button[] menuButtons;
        public RectTransform replicationTarget;
        public Button replicationButton;
        public Image replicationImage;
        private Button _currentReplicatedButton;
        
        void Update()
        {
            Vector3[] rotatingCorners = new Vector3[4];
            rotatingRect.GetWorldCorners(rotatingCorners);
            Rect rotatingAABB = GetAABB(rotatingCorners);

            bool intersectionFound = false;

            foreach (Button btn in menuButtons.Where(x => x.gameObject.activeSelf))
            {
                RectTransform btnRect = btn.transform as RectTransform;
                Vector3[] btnCorners = new Vector3[4];
                btnRect.GetWorldCorners(btnCorners);
                Rect btnAABB = GetAABB(btnCorners);

                if (!rotatingAABB.Overlaps(btnAABB)) continue;
                if (_currentReplicatedButton != btn)
                {
                    _currentReplicatedButton = btn;
                    UpdateReplicationButton(btn);
                }

                intersectionFound = true;
                break;
            }

            if (intersectionFound)
            {
                replicationButton.transform.position = replicationTarget.position;
                replicationButton.gameObject.SetActive(true);
            }
            else
            {
                _currentReplicatedButton = null;
                replicationButton.gameObject.SetActive(false);
            }
        }

        void UpdateReplicationButton(Button btn)
        {
            Image btnImage = (Image)btn.targetGraphic;
            if (btnImage && replicationImage)
            {
                replicationImage.sprite = btnImage.sprite;
                replicationImage.color = btnImage.color;
            }

            replicationButton.onClick.RemoveAllListeners();

            Button originalButton = btn;
            if (originalButton)
            {
                replicationButton.onClick.AddListener(() => { originalButton.onClick.Invoke(); });
            }
        }

        Rect GetAABB(Vector3[] corners)
        {
            float minX = corners[0].x;
            float maxX = corners[0].x;
            float minY = corners[0].y;
            float maxY = corners[0].y;
            for (int i = 1; i < corners.Length; i++)
            {
                minX = Mathf.Min(minX, corners[i].x);
                maxX = Mathf.Max(maxX, corners[i].x);
                minY = Mathf.Min(minY, corners[i].y);
                maxY = Mathf.Max(maxY, corners[i].y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}