using Kart.Project_Files.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Audio
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAudio : MonoBehaviour
    {
        [SerializeField] private Button button;
        
        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            button.onClick.AddListener(() => AudioManager.Instance.PlayUI("clickUI"));
        }
    }
}