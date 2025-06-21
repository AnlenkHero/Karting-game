using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI
{
    public class ScreenshotToRawImage : MonoBehaviour
    {
        [Header("Drag your RawImage here")] [SerializeField]
        private RawImage targetRawImage;

        [SerializeField] private Image targetMonitorImage;

        public void CaptureFullScreen()
        {
            //StartCoroutine(CaptureCoroutine());
        }

        public IEnumerator CaptureCoroutine(Sprite sprite)
        {
            targetMonitorImage.sprite = sprite;
            targetMonitorImage.enabled = true;
            yield return new WaitForEndOfFrame();
            
            int width = Screen.width;
            int height = Screen.height;
            Texture2D screenTex = new Texture2D(width, height, TextureFormat.RGB24, false);


            screenTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenTex.Apply();

            if (targetRawImage != null)
            {
                targetRawImage.texture = screenTex;
            }
            else
            {
                Debug.LogWarning("ScreenshotToRawImage: No RawImage assigned!");
            }

            yield return new WaitForEndOfFrame();
            targetMonitorImage.enabled = false;
        }
    }
}