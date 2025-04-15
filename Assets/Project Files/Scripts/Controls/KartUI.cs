using Fusion;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.OtherNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartUI : NetworkBehaviour
    {
        [Header("Player UI")] 
        [SerializeField] private KartController kartController;
        [SerializeField] private TextMeshPro playerText;
        [SerializeField] private RawImage countryFlagImage;
        [SerializeField] private GameObject playerUIGameObject;

        #region LifeCycle

        public override void Spawned()
        {
            base.Spawned();
            if (!HasInputAuthority) return;
            playerUIGameObject.gameObject.SetActive(false);

            RPC_SetKartFlag(RoomPlayer.Local.CountryCode.Value, RoomPlayer.Local.CountryPrivacy);
        }

        private void Update()
        {
            playerText.SetText(
                $"{kartController.KartName} SPEED: {kartController.NetworkedVelocity.magnitude:F1}");
        }

        #endregion

        #region RPC

        [Rpc]
        private void RPC_SetKartFlag(string countryCode, bool showCountry, RpcInfo info = default)
        {
            if (!countryCode.IsNullOrWhiteSpace() && !showCountry)
            {
                countryFlagImage.gameObject.SetActive(false);
                return;
            }

            CountryFlagLoader.LoadFlag(this, countryCode, texture2D => countryFlagImage.texture = texture2D);
        }

        #endregion
    }
}