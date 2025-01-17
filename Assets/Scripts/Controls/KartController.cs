using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Kart.Controls
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
        public WheelFrictionCurve originalForwardFriction;
        public WheelFrictionCurve originalSidewaysFriction;
    }

    public class KartController : NetworkBehaviour
    {
        [Header("Axle Information")] [SerializeField]
        private AxleInfo[] axleInfos;

        [Header("Motor Attributes")] [SerializeField]
        private float maxMotorTorque = 10f;

        [SerializeField] private float maxSpeed = 100;
        [SerializeField] private float speedRatio = 5f;
        [SerializeField] private float engineBrakingForce = 50f;

        [Header("Steering Attributes")] [SerializeField]
        private float turnPersistenceTorque = 0.09f;

        [SerializeField] private float driftAngleThreshold = 90f;
        [SerializeField] private float maxDriftAngle = 150f;
        [SerializeField] private float lowSpeedTurnThreshold = 22f;
        [SerializeField] private float maxSteeringAngle = 30f;
        [SerializeField] private float reverseSteeringAngle = 15f;
        [SerializeField] private AnimationCurve turnCurve;
        [SerializeField] private float turnStrength = 1500f;

        [Header("Braking and Drifting")] [SerializeField]
        private float driftSteerMultiplier = 1.5f;

        [SerializeField] private float driftFriction = 0.5f;
        [SerializeField] private float slipThreshold = 0.9f;
        [SerializeField] private float brakeTorque = 10000f;
        public float brakeMultiplier = 1.0f;

        [Header("Physics")] [SerializeField] private Transform centerOfMass;
        [SerializeField] private float downForce = 100f;
        [SerializeField] private float gravity = Physics.gravity.y;
        [SerializeField] private float lateralGScale = 10f;
        [SerializeField] private float gravityMultiplierForAirborne = 5f;
        [SerializeField] private float airControlMultiplier = 0.5f;

        [Header("Banking")] [SerializeField] private float maxBankAngle = 5f;
        [SerializeField] private float bankSpeed = 2f;

        [Header("Surface Modifiers")] public float frictionMultiplier = 1.0f;
        public float slowdownMultiplier = 1.0f;
        public float steeringSensitivityMultiplier = 1.0f;

        [Header("Input")] [SerializeField] private InputReader playerInput;

        [Header("References")] [SerializeField]
        private Circuit circuit;

        [SerializeField] private AIDriverData driverData;
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private AudioListener playerAudioListener;
        [SerializeField] private Rigidbody rb;

        [Header("Player Debug Info")] [SerializeField]
        private TextMeshPro playerText;

        private IDrive input;
        private Vector3 originalCenterOfMass;
        private Vector3 kartVelocity;
        private float brakeVelocity;
        private float driftVelocity;
        private float currentSteeringAngle;
        private float steeringVelocity;
        public float VerticalInput => input.Move.y;
        public Vector3 Velocity => kartVelocity;
        public float MaxSpeed => maxSpeed;
        public float MaxReverseSpeed => maxSpeed / speedRatio;
        public float Direction => Mathf.Sign(Vector3.Dot(transform.forward, rb.linearVelocity));
        public float SignedVelocityMagnitude => Velocity.magnitude * Direction;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            playerText.SetText(
                $"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {kartVelocity.magnitude:F1}");
        }

        private void FixedUpdate()
        {
            Move(input.Move);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                playerAudioListener.enabled = false;
                playerCamera.Priority = 0;
                return;
            }

            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;
        }

        #endregion
        
        #region CoreMethods

        private void Move(Vector2 inputVector)
        {
            ApplyLowSpeedStop();

            float verticalInput = AdjustInput(inputVector.y);
            float horizontalInput = AdjustInput(inputVector.x);

            float randomTorqueFactor = Random.Range(0.3f, 1.05f);
            float motor = maxMotorTorque * verticalInput * speedRatio * randomTorqueFactor;

            UpdateAxles(motor, horizontalInput);
            UpdateBanking(horizontalInput);

            kartVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            if (IsGrounded())
            {
                HandleGroundedMovement(verticalInput, motor);
            }
            else
            {
                HandleAirborneMovement(verticalInput, horizontalInput);
            }
        }

        private void UpdateAxles(float motor, float steeringInput)
        {
            foreach (AxleInfo axleInfo in axleInfos)
            {
                HandleSteering(axleInfo, steeringInput);
                HandleMotor(axleInfo, motor);
                HandleBrakesAndDrift(axleInfo);
                UpdateWheelVisuals(axleInfo.leftWheel);
                UpdateWheelVisuals(axleInfo.rightWheel);
            }
        }

        #endregion

        #region Acceleration

        private void HandleGroundedMovement(float verticalInput, float motor)
        {
            if (Mathf.Abs(verticalInput) < 0.1f && rb.linearVelocity.magnitude > 0.1f && !input.IsBraking)
            {
                ApplyEngineBraking();
            }

            var targetSpeed = DefineTargetSpeedForGroundMovement(verticalInput);
            AccelerateGroundMovement(motor, targetSpeed);
            ApplyDownforce();
            AdjustCenterOfMass(verticalInput);
        }

        private void AccelerateGroundMovement(float motor, float targetSpeed)
        {
            if (input.IsBraking) return;

            if (rb.linearVelocity.magnitude < Mathf.Abs(targetSpeed))
            {
                rb.AddForce(transform.forward * motor, ForceMode.Acceleration);
            }
        }

        private float DefineTargetSpeedForGroundMovement(float verticalInput)
        {
            float targetSpeed = verticalInput switch
            {
                < 0 when SignedVelocityMagnitude > 0 => verticalInput * maxSpeed * brakeMultiplier * speedRatio / 2,
                < 0 => verticalInput * maxSpeed * 1 / speedRatio,
                _ => verticalInput * maxSpeed * slowdownMultiplier
            };
            return targetSpeed;
        }

        private void HandleAirborneMovement(float verticalInput, float horizontalInput)
        {
            Vector3 downwardForce = Vector3.down * (-gravity * gravityMultiplierForAirborne);
            rb.AddForce(downwardForce, ForceMode.Acceleration);
            Vector3 airControlForce = new Vector3(horizontalInput, 0, verticalInput) * airControlMultiplier;
            rb.AddForce(transform.TransformDirection(airControlForce), ForceMode.Acceleration);
        }

        private void HandleMotor(AxleInfo axleInfo, float motor)
        {
            if (!axleInfo.motor) return;

            axleInfo.leftWheel.motorTorque = motor;
            axleInfo.rightWheel.motorTorque = motor;
        }

        #endregion

        #region Steering

        private void HandleSteering(AxleInfo axleInfo, float steeringInput)
        {
            float speedFactor = Mathf.Clamp01(kartVelocity.magnitude / maxSpeed);
            float adjustedTurnFactor = turnCurve.Evaluate(speedFactor);
            float effectiveSteeringAngle = (Direction > 0 ? maxSteeringAngle : reverseSteeringAngle) *
                                           steeringSensitivityMultiplier;
            float targetSteeringAngle = steeringInput * effectiveSteeringAngle * adjustedTurnFactor;

            targetSteeringAngle = ApplyCounterSteering(targetSteeringAngle);
            targetSteeringAngle = Mathf.Clamp(targetSteeringAngle, -effectiveSteeringAngle, effectiveSteeringAngle);

            ApplySteeringToWheels(axleInfo, targetSteeringAngle);
            ApplySteeringHelp(steeringInput, adjustedTurnFactor);
        }

        private void ApplySteeringHelp(float steeringInput, float adjustedTurnFactor)
        {
            if (!(Mathf.Abs(steeringInput) > 0.1f) || !(kartVelocity.magnitude > lowSpeedTurnThreshold) ||
                !IsGrounded()) return;

            float angleBetween = Vector3.Angle(transform.forward, rb.linearVelocity);
            float baseDirectionMultiplier = Direction > 0 ? 1f : -1f;
            float driftBlendFactor = Mathf.InverseLerp(driftAngleThreshold, maxDriftAngle, angleBetween);
            float directionMultiplier = Mathf.Lerp(baseDirectionMultiplier, -baseDirectionMultiplier, driftBlendFactor);
            Vector3 desiredTurnDirection = Vector3.up * (steeringInput * turnPersistenceTorque * adjustedTurnFactor *
                                                         directionMultiplier * steeringSensitivityMultiplier);
            rb.AddTorque(desiredTurnDirection, ForceMode.Acceleration);
        }

        private void ApplySteeringToWheels(AxleInfo axleInfo, float targetSteeringAngle)
        {
            if (!axleInfo.steering) return;

            float steeringMultiplier = input.IsBraking ? driftSteerMultiplier : 1f;

            currentSteeringAngle = Mathf.SmoothDamp(currentSteeringAngle, targetSteeringAngle * steeringMultiplier,
                ref steeringVelocity,
                Time.deltaTime * 2f);

            axleInfo.leftWheel.steerAngle = currentSteeringAngle;
            axleInfo.rightWheel.steerAngle = currentSteeringAngle;
        }

        private float ApplyCounterSteering(float targetSteeringAngle)
        {
            Vector3 referenceForward = Direction > 0 ? transform.forward : -transform.forward;
            float angleBetween = Vector3.SignedAngle(referenceForward, rb.linearVelocity, Vector3.up);

            if (Mathf.Abs(angleBetween) > 1)
            {
                targetSteeringAngle += angleBetween;
            }

            return targetSteeringAngle;
        }

        #endregion

        #region Brakes

        private void ApplyEngineBraking()
        {
            float speedFactor = rb.linearVelocity.magnitude / maxSpeed;
            Vector3 deceleration = -rb.linearVelocity.normalized * (engineBrakingForce * speedFactor) / speedRatio;
            rb.AddForce(deceleration, ForceMode.Acceleration);
        }

        private void HandleBrakesAndDrift(AxleInfo axleInfo)
        {
            if (!axleInfo.motor) return;

            if (input.IsBraking)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX;
                HandleHandbrake();
                ApplyHandbrakeToWheels(axleInfo);
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
                DisableWheelsHandbrake(axleInfo);
            }
        }

        private void DisableWheelsHandbrake(AxleInfo axleInfo)
        {
            axleInfo.leftWheel.brakeTorque = 0;
            axleInfo.rightWheel.brakeTorque = 0;
            ResetDriftFriction(axleInfo.leftWheel);
            ResetDriftFriction(axleInfo.rightWheel);
        }

        private void ApplyHandbrakeToWheels(AxleInfo axleInfo)
        {
            axleInfo.leftWheel.brakeTorque = brakeTorque;
            axleInfo.rightWheel.brakeTorque = brakeTorque;
            ApplyDriftFriction(axleInfo.leftWheel);
            ApplyDriftFriction(axleInfo.rightWheel);
        }

        private void HandleHandbrake()
        {
            Vector3 forwardDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 forwardVelocity = Vector3.Project(currentVelocity, forwardDirection);
            Vector3 sidewaysVelocity = currentVelocity - forwardVelocity;

            float stiffness = axleInfos.FirstOrDefault()?.originalSidewaysFriction.stiffness ?? 1f;

            if (Mathf.Approximately(stiffness, 0f))
            {
                stiffness = 0.0001f;
            }

            float lerpFactor = Time.fixedDeltaTime * stiffness * 0.1f * brakeMultiplier;

            forwardVelocity = Vector3.Lerp(forwardVelocity, Vector3.zero, lerpFactor);
            sidewaysVelocity = Vector3.Lerp(sidewaysVelocity, sidewaysVelocity * 0.5f, lerpFactor);

            rb.linearVelocity = forwardVelocity + sidewaysVelocity;
        }

        private void ApplyLowSpeedStop()
        {
            if (rb.linearVelocity.magnitude < 0.2f)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        #endregion

        #region Physics

        private void AdjustCenterOfMass(float verticalInput)
        {
            Vector3 centerOfMassAdjustment = Velocity.magnitude > 10f
                ? new Vector3(0f, 0f, Mathf.Abs(verticalInput) > 0.1f ? Mathf.Sign(verticalInput) * -0.5f : 0f)
                : Vector3.zero;
            rb.centerOfMass = originalCenterOfMass + centerOfMassAdjustment;
        }

        private void ApplyDownforce()
        {
            float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            float lateralG = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
            float downForceFactor = Mathf.Max(speedFactor, lateralG / lateralGScale);
            rb.AddForce(-transform.up * (downForce * rb.mass * downForceFactor));
        }

        #endregion

        #region WheelFriction

        private void ResetDriftFriction(WheelCollider wheel)
        {
            AxleInfo axleInfo = axleInfos.FirstOrDefault(axle => axle.leftWheel == wheel || axle.rightWheel == wheel);
            if (axleInfo == null) return;

            wheel.forwardFriction = axleInfo.originalForwardFriction;
            wheel.sidewaysFriction = axleInfo.originalSidewaysFriction;
        }

        public void SetSurfaceFriction(float forwardFriction, float sidewaysFriction)
        {
            foreach (var axleInfo in axleInfos)
            {
                axleInfo.originalForwardFriction = SetFriction(axleInfo.originalForwardFriction, forwardFriction);
                axleInfo.originalSidewaysFriction = SetFriction(axleInfo.originalSidewaysFriction, sidewaysFriction);

                axleInfo.leftWheel.forwardFriction = SetFriction(axleInfo.leftWheel.forwardFriction, forwardFriction);
                axleInfo.leftWheel.sidewaysFriction =
                    SetFriction(axleInfo.leftWheel.sidewaysFriction, sidewaysFriction);
                axleInfo.rightWheel.forwardFriction = SetFriction(axleInfo.rightWheel.forwardFriction, forwardFriction);
                axleInfo.rightWheel.sidewaysFriction =
                    SetFriction(axleInfo.rightWheel.sidewaysFriction, sidewaysFriction);
            }
        }

        private void ApplyDriftFriction(WheelCollider wheel)
        {
            if (!wheel.GetGroundHit(out _)) return;

            wheel.forwardFriction = UpdateFriction(wheel.forwardFriction);
            wheel.sidewaysFriction = UpdateFriction(wheel.sidewaysFriction);
        }

        private WheelFrictionCurve SetFriction(WheelFrictionCurve wheelFrictionCurve, float friction)
        {
            wheelFrictionCurve.stiffness = friction;
            return wheelFrictionCurve;
        }

        private WheelFrictionCurve UpdateFriction(WheelFrictionCurve friction)
        {
            friction.stiffness = input.IsBraking
                ? Mathf.SmoothDamp(friction.stiffness, driftFriction * frictionMultiplier, ref driftVelocity,
                    Time.deltaTime * 2f)
                : 1f;
            return friction;
        }

        #endregion

        #region State

        public bool IsGrounded()
        {
            return axleInfos.Where(x => x.motor).Any(axleInfo =>
                IsWheelGrounded(axleInfo.leftWheel) || IsWheelGrounded(axleInfo.rightWheel));
        }

        private bool IsWheelGrounded(WheelCollider wheel)
        {
            return wheel.GetGroundHit(out _);
        }

        public bool IsDrifting()
        {
            return axleInfos.Any(
                axleInfo => IsWheelDrifting(axleInfo.leftWheel) || IsWheelDrifting(axleInfo.rightWheel));
        }

        public bool IsWheelDrifting(WheelCollider wheelCollider)
        {
            if (!wheelCollider.GetGroundHit(out var hit)) return false;
            return Mathf.Abs(hit.sidewaysSlip) > slipThreshold || Mathf.Abs(hit.forwardSlip) > slipThreshold;
        }

        #endregion

        #region Helpers

        public void SetInput(IDrive driveInput)
        {
            input = driveInput;
        }

        private float AdjustInput(float inputValue)
        {
            return inputValue switch
            {
                >= .7f => 1f,
                <= -.7f => -1f,
                _ => inputValue
            };
        }

        #endregion

        #region Visuals

        private void UpdateBanking(float horizontalInput)
        {
            float targetBankAngle = horizontalInput * -maxBankAngle;
            Vector3 currentEuler = transform.localEulerAngles;
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, targetBankAngle, Time.deltaTime * bankSpeed);
            transform.localEulerAngles = currentEuler;
        }

        private void UpdateWheelVisuals(WheelCollider wheelCollider)
        {
            if (wheelCollider.transform.childCount == 0) return;

            Transform visualWheel = wheelCollider.transform.GetChild(0);
            wheelCollider.GetWorldPose(out var position, out var rotation);
            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            if (playerInput is IDrive driveInput)
            {
                input = driveInput;
            }

            input.Enable();

            rb.centerOfMass = centerOfMass.localPosition;
            originalCenterOfMass = centerOfMass.localPosition;

            foreach (AxleInfo axleInfo in axleInfos)
            {
                axleInfo.originalForwardFriction = axleInfo.leftWheel.forwardFriction;
                axleInfo.originalSidewaysFriction = axleInfo.leftWheel.sidewaysFriction;
            }
        }

        #endregion
    }
}