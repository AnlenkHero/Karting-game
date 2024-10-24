using Unity.Netcode.Components;
using UnityEngine;

namespace Kart {
    public enum AuthorityMode {
        Server,
        Client
    }
    
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        public AuthorityMode authorityMode = Kart.AuthorityMode.Client;

        protected override bool OnIsServerAuthoritative() => authorityMode == Kart.AuthorityMode.Server;
    }
}