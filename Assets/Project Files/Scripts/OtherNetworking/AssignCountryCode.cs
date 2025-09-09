using System;
using Kart.Project_Files.Scripts.Fusion;
using UnityEngine;

namespace Kart.Project_Files.Scripts.OtherNetworking
{
    public class AssignCountryCode : MonoBehaviour
    {
        private void Start()
        {
            SetCountryCode();
        }

        private void SetCountryCode()
        {
            CountryFlagLoader.LoadCountryCode(this, s => ClientInfo.CountryCode = s);
        }
    }
}