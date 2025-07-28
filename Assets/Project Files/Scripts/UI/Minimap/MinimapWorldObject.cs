using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Minimap
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private bool isNetworkObject;
        [SerializeField] private bool followObject;
        [SerializeField] private Sprite minimapIcon;
        [SerializeField] private string nameText;

        public Sprite MinimapIcon => minimapIcon;
        public string NameText   => nameText;

        private void OnEnable()   => TryRegisterOffline();
        private void Start()      => TryRegisterOffline();      
        private void OnDisable()  => TryRemove();
        private void OnDestroy()  => TryRemove();

        private void TryRegisterOffline()
        {
            if(isNetworkObject)
                return;
            var ctrl = MinimapController.Instance;
            if (ctrl == null || !ctrl.isActiveAndEnabled)
                return;

            ctrl.RegisterMinimapWorldObject(this, followObject);
        }

        public void SetData(string objectName)
        {
            nameText = objectName;
        }
        public void TryRegisterOnline(bool follow)
        {
            var ctrl = MinimapController.Instance;
            if (ctrl == null || !ctrl.isActiveAndEnabled)
                return;

            ctrl.RegisterMinimapWorldObject(this, follow);
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