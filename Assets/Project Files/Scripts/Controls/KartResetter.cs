using Fusion;
using Fusion.Addons.Physics;
using Kart.Project_Files.Scripts.Managers.Game;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class CarResetter : NetworkBehaviour
    {
        [Header("Hold‑to‑Reset")]
        [SerializeField] KeyCode             resetKey            = KeyCode.R;
        [SerializeField] float               holdDuration        = 3f;
        [SerializeField] NetworkRigidbody3D  networkRigidbody3D;
        [SerializeField] private KartController kartController;

        float _holdTimer;

        void Update()
        {
            if (!Object.HasInputAuthority) return;

            if (Input.GetKey(resetKey))
            {
                _holdTimer += Time.deltaTime;
                if (!(_holdTimer >= holdDuration)) return;
                
                int nearestIndex = GetNearestCheckpointIndex();
                _holdTimer = 0f;
                RPC_RequestReset(nearestIndex);
            }
            else _holdTimer = 0f;
        }

        int GetNearestCheckpointIndex()
        {
            var cps = GameManager.Instance.currentTrack.resetCheckpoints;
            int bestI = -1;
            float bestSqr = float.MaxValue;
            Vector3 me = transform.position;
            for (int i = 0; i < cps.Length; i++)
            {
                float d = (cps[i].position - me).sqrMagnitude;
                if (!(d < bestSqr)) continue;
                bestSqr = d;
                bestI = i;
            }
            return bestI;
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        void RPC_RequestReset(int checkpointIndex)
        {
            if (checkpointIndex < 0) return;
            var cp = GameManager.Instance.currentTrack.resetCheckpoints[checkpointIndex];
            kartController.ResetSpeed();
            networkRigidbody3D.Teleport(cp.position, cp.rotation);
        }
    }
}