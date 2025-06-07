using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI
{
    public class ScreenshotToRawImage : MonoBehaviour
    {
        [Header("Drag your RawImage here")]
        [SerializeField] private RawImage targetRawImage;
        
        public void CaptureFullScreen()
        {
            StartCoroutine(CaptureCoroutine());
        }

        public IEnumerator CaptureCoroutine()
        {
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
        }
    }
}