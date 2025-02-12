using Fusion;
using UnityEngine;

namespace Kart.Fusion
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Rigidbody controller;
        public override void FixedUpdateNetwork()
        {
            if(GetInput(out KartInput.NetworkInputData input))
            {
                    if (input.Nigga)
                    {
                        controller.AddForce(1000 * Vector3.forward * Runner.DeltaTime);   
                    }
                    if (input.IsReverse)
                    {
                        controller.AddForce(1000 * Vector3.back * Runner.DeltaTime);
                    }
                    controller.AddForce(1000 * Vector3.right * Runner.DeltaTime * input.Steer);
            }
        }
    }
}