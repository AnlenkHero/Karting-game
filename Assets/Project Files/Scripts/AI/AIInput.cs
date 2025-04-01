using System;
using Kart.Controls;
using UnityEngine;
using UnityUtils;
using Utilities;

namespace Kart
{
    public class AIInput : MonoBehaviour, IDrive
    {
        public Circuit circuit;
        public AIDriverData driverData;
        public Vector2 Move { get; private set; }
        public bool IsBraking { get; private set; }

        public void Enable()
        {
            //
        }

        private int currentWaypointIndex;
        private int currentCornerIndex;

        CountdownTimer driftTimer;

        float previousYaw; 

        public void AddDriverData(AIDriverData data) => driverData = data;
        public void AddCircuit(Circuit circuit) => this.circuit = circuit;

        void Start()
        {
            if (circuit == null || driverData == null)
            {
                throw new ArgumentNullException($"AIInput requires a circuit and driver data to be set.");
            }

            previousYaw = transform.eulerAngles.y;
            driftTimer = new CountdownTimer(driverData.timeToDrift);
            driftTimer.OnTimerStart += () => IsBraking = true;
            driftTimer.OnTimerStop += () => IsBraking = false;
        }

        void Update()
        {
            driftTimer.Tick(Time.deltaTime);
            if (circuit.waypoints.Length == 0)
            {
                return;
            }
            
            float currentYaw = transform.eulerAngles.y;
            float deltaYaw = Mathf.DeltaAngle(previousYaw, currentYaw);
            float angularVelocity = deltaYaw / Time.deltaTime;
            previousYaw = currentYaw;

            Vector3 toNextPoint = circuit.waypoints[currentWaypointIndex].position - transform.position;
            Vector3 toNextCorner = circuit.waypoints[currentCornerIndex].position - transform.position;
            var distanceToNextPoint = toNextPoint.magnitude;
            var distanceToNextCorner = toNextCorner.magnitude;
            
            if (distanceToNextPoint < driverData.proximityThreshold)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Length;
            }
            
            if (distanceToNextCorner < driverData.updateCornerRange)
            {
                currentCornerIndex = currentWaypointIndex;
            }
            
            if (distanceToNextCorner < driverData.brakeRange && !driftTimer.IsRunning)
            {
                driftTimer.Start();
            }
            
            Debug.Log($"{currentWaypointIndex} {currentCornerIndex}");
            Move = Move.With(y: driftTimer.IsRunning ? driverData.speedWhileDrifting : 1f);
            
            Vector3 desiredForward = toNextPoint.normalized;
            Vector3 currentForward = transform.forward;
            float turnAngle = Vector3.SignedAngle(currentForward, desiredForward, Vector3.up);
            
            Move = turnAngle switch
            {
                > 5f => Move.With(x: 1f),
                < -5f => Move.With(x: -1f),
                _ => Move.With(x: 0f)
            };
            
            if (Mathf.Abs(angularVelocity) > driverData.spinThreshold)
            {
                Move = Move.With(x: -Mathf.Sign(angularVelocity));
                IsBraking = true;
            }
            else
            {
                IsBraking = false;
            }
        }
    }
}