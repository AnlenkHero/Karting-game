using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Minimap
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private bool followObject = false;
        [SerializeField] private Sprite minimapIcon;
        [SerializeField] private string nameText;

        public Sprite MinimapIcon => minimapIcon;
        public string NameText   => nameText;

        private void OnEnable()   => TryRegister();
        private void Start()      => TryRegister();      
        private void OnDisable()  => TryRemove();
        private void OnDestroy()  => TryRemove();

        private void TryRegister()
        {
            var ctrl = MinimapController.Instance;
            if (ctrl == null || !ctrl.isActiveAndEnabled)
                return;

            ctrl.RegisterMinimapWorldObject(this, followObject);
        }

        private void TryRemove()
        {
            var ctrl = MinimapController.Instance;
            if (ctrl == null || !ctrl.isActiveAndEnabled)
                return;

            ctrl.RemoveMinimapWorldObject(this);
        }
    }
}