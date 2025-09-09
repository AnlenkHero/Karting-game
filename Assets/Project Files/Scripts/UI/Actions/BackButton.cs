using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Actions
{
    public class BackButton : MonoBehaviour
    {
        [SerializeField] private Button backButton;

        private void Awake()
        {
            backButton.onClick.AddListener(InterfaceManager.Instance.CloseActiveScreen);
        }
    }
}