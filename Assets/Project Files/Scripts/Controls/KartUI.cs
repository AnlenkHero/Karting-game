using Fusion;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.OtherNetworking;
using Kart.Project_Files.Scripts.UI.Minimap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartUI : NetworkBehaviour
    {
        [Header("Player UI")] [SerializeField] private KartController kartController;
        [SerializeField] private TextMeshPro playerText;
        [SerializeField] private RawImage countryFlagImage;
        [SerializeField] private GameObject playerUIGameObject;
        [SerializeField] private MinimapWorldObject minimapWorldObject;
        private bool _isUiActive;
        
        #region LifeCycle

        public override void Spawned()
        {
            base.Spawned();
            ShowPlayerUI(!HasInputAuthority);
            if (!HasInputAuthority) return;
            RPC_SetMinimapWorldObject();
            RPC_SetKartFlag(RoomPlayer.Local.CountryCode.Value, RoomPlayer.Local.CountryPrivacy);
        }

        private void Update()
        {
            playerText.SetText($"{kartController.KartName}");
        }

        #endregion

        #region RPC
        
        [Rpc]
        private void RPC_SetMinimapWorldObject()
        {
            minimapWorldObject.SetData(kartController.KartName);
            minimapWorldObject.TryRegisterOnline(HasInputAuthority);
        }
        
        [Rpc]
        private void RPC_SetKartFlag(string countryCode, bool showCountry)
        {
            if (!showCountry || !_isUiActive)
            {
                countryFlagImage.gameObject.SetActive(false);
                return;
            }
            
            CountryFlagLoader.LoadFlag(this, countryCode, texture2D => countryFlagImage.texture = texture2D);
        }

        #endregion

        public void ShowPlayerUI(bool show)
        {
            if (playerUIGameObject == null)
            {
                _isUiActive = false;
                return;
            }
            _isUiActive = show;
            playerUIGameObject.gameObject.SetActive(show);
        }
    }
}