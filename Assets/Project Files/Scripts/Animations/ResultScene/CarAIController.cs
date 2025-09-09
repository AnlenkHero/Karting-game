using System.Collections.Generic;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarAIController : MonoBehaviour
    {
        [Header("Physics setup")] public WheelCollider frontLeft, frontRight, rearLeft, rearRight;
        public Transform meshFL, meshFR, meshRL, meshRR;

        [Header("Waypoints")] public List<Transform> waypoints;
        public float reachThreshold = 5f;

        [Header("Driving parameters")] public float maxSteerAngle = 30f;
        public float maxMotorTorque = 400f;
        public float targetSpeed = 20f;

        [Header("Braking")] public float extraBrakeTorque = 1500f;

        private Rigidbody _rb;
        public bool finishedAnimation;
        public Transform kartParent;

        public JumpOnPodium jumpOnPodium;

        private void Awake()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass += Vector3.down * 0.5f;
        }

        void FixedUpdate()
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                if (_rb.linearVelocity.magnitude < 1f)
                    finishedAnimation = true;
                ApplyBrake(extraBrakeTorque);
                return;
            }

            Vector3 toWp = waypoints[0].position - transform.position;
            float dist = toWp.magnitude;

            if (dist < reachThreshold)
            {
                waypoints.RemoveAt(0);
                return;
            }

            Steer(toWp);
            Drive();
            Physics.Simulate(Time.fixedDeltaTime);
            UpdateWheels();
        }

        void Steer(Vector3 toWp)
        {
            Vector3 localDir = transform.InverseTransformDirection(toWp.normalized);
            float steer = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            steer = Mathf.Clamp(steer, -maxSteerAngle, maxSteerAngle);

            frontLeft.steerAngle = steer;
            frontRight.steerAngle = steer;
        }

        void Drive()
        {
            SetAllBrakes(0f);

            float speed = _rb.linearVelocity.magnitude;
            float accel = Mathf.Clamp01((targetSpeed - speed) / targetSpeed);
            float torque = accel * maxMotorTorque;

            rearLeft.motorTorque = torque;
            rearRight.motorTorque = torque;
            frontLeft.motorTorque = 0f;
            frontRight.motorTorque = 0f;
        }

        void ApplyBrake(float brakeForce)
        {
            frontLeft.motorTorque =
                frontRight.motorTorque =
                    rearLeft.motorTorque =
                        rearRight.motorTorque = 0f;

            SetAllBrakes(brakeForce);
            UpdateWheels();
        }

        void SetAllBrakes(float brake)
        {
            frontLeft.brakeTorque =
                frontRight.brakeTorque =
                    rearLeft.brakeTorque =
                        rearRight.brakeTorque = brake;
        }

        void UpdateWheels()
        {
            ApplyPose(frontLeft, meshFL);
            ApplyPose(frontRight, meshFR);
            ApplyPose(rearLeft, meshRL);
            ApplyPose(rearRight, meshRR);
        }

        void ApplyPose(WheelCollider wc, Transform mesh)
        {
            Vector3 pos;
            Quaternion rot;
            wc.GetWorldPose(out pos, out rot);
            mesh.position = pos;
            mesh.rotation = rot;
        }
    }
}