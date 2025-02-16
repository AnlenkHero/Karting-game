using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class UIIntersectionDetector : MonoBehaviour
    {
        [Header("References")]
        public RectTransform rotatingRect;

        public MainMenuButton[] menuButtons;

        public RectTransform replicationTarget;

        public Button replicationButton;

        public Image replicationImage;

        private MainMenuButton currentReplicatedButton = null;
        private Dictionary<MainMenuButton, Rect> staticButtonAABBs = new Dictionary<MainMenuButton, Rect>();
        private void Awake()
        {
            foreach (MainMenuButton btn in menuButtons)
            {
                RectTransform btnRect = btn.GetComponent<RectTransform>();
                Vector3[] btnCorners = new Vector3[4];
                btnRect.GetWorldCorners(btnCorners);
                Rect btnAABB = GetAABB(btnCorners);
                staticButtonAABBs[btn] = btnAABB;
            }
        }

        void Update()
        {
            Vector3[] rotatingCorners = new Vector3[4];
            rotatingRect.GetWorldCorners(rotatingCorners);
            Rect rotatingAABB = GetAABB(rotatingCorners);

            bool intersectionFound = false;

            foreach (MainMenuButton btn in menuButtons)
            {
                RectTransform btnRect = btn.GetComponent<RectTransform>();
                Vector3[] btnCorners = new Vector3[4];
                btnRect.GetWorldCorners(btnCorners);
                Rect btnAABB = GetAABB(btnCorners);

                if (!rotatingAABB.Overlaps(btnAABB)) continue;
                if (currentReplicatedButton != btn)
                {
                    currentReplicatedButton = btn;
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
                currentReplicatedButton = null;
                replicationButton.gameObject.SetActive(false);
            }
        }

        void UpdateReplicationButton(MainMenuButton btn)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null && replicationImage != null)
            {
                replicationImage.sprite = btnImage.sprite;
                replicationImage.color = btnImage.color;
            }

            replicationButton.onClick.RemoveAllListeners();

            Button originalButton = btn.GetComponent<Button>();
            if (originalButton != null)
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