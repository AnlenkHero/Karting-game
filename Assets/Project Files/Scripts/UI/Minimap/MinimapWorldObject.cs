using Fusion;
using UnityEngine;


namespace Kart.Project_Files.Scripts.UI.Minimap
{
    public class MinimapWorldObject : NetworkBehaviour
    {
        [SerializeField] private bool isDynamicInstantiatedObject;
        [SerializeField] private bool followObject;
        [SerializeField] private Sprite minimapIcon;
        [SerializeField] private string nameText;

        public Sprite MinimapIcon => minimapIcon;
        public string NameText   => nameText;

        public override void Spawned()   => TryRegisterOffline();
        public override void Despawned(NetworkRunner runner, bool hasState) => TryRemove();

        private void TryRegisterOffline()
        {
            if(isDynamicInstantiatedObject)
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