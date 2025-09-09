using System;
using Fusion;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class SkidMarkHandler : NetworkBehaviour
    {
        [Networked] public bool ShouldSkid { get; set; }
        public event Action SkidStarted;
        public event Action SkidEnded;
        [SerializeField] private KartController kart;
        [SerializeField] private WheelCollider[] wheelColliders;
        [SerializeField] private TrailRenderer[] skidMarks;
        private const float MinimalDriftingSpeed = 3f;

        public void Update()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                var tr = skidMarks[i];
                switch (ShouldSkid)
                {
                    case true when !tr.emitting:
                        StartSkid(i);
                        break;
                    case false when tr.emitting:
                        EndSkid(i);
                        break;
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority)
                return;
            for (int i = 0; i < wheelColliders.Length; i++)
                CheckAndReportSkid(i);
        }

        void CheckAndReportSkid(int i)
        {
            bool shouldSkid = kart.IsGrounded()
                              && kart.NetworkedVelocity.magnitude > MinimalDriftingSpeed
                              && kart.IsWheelDrifting(wheelColliders[i]);
            Rpc_SetBool(shouldSkid);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void Rpc_SetBool(bool value)
        {
            ShouldSkid = value;
        }

        void SetSkidEmitting(int i, bool on)
        {
            var tr = skidMarks[i];
            if (tr == null) return;
            tr.emitting = on;
        }

        void StartSkid(int wheelIndex)
        {
            SkidStarted?.Invoke();
            SetSkidEmitting(wheelIndex, true);
        }


        void EndSkid(int wheelIndex)
        {
            SkidEnded?.Invoke();
            SetSkidEmitting(wheelIndex, false);
        }
    }
}