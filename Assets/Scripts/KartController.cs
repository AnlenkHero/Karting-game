using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Kart
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
        AxleInfo[] axleInfos;

        [Header("Motor Attributes")] [SerializeField]
        float maxMotorTorque = 3000f;

        [SerializeField] float maxSpeed;
        [SerializeField] private float speedRatio = 5f;
        [SerializeField] private float engineBrakingForce = 50f;

        [Header("Steering Attributes")] [SerializeField]
        private float turnPersistenceTorque = 0.005f;

        [SerializeField] float driftAngleThreshold = 90f;
        [SerializeField] float maxDriftAngle = 150f;
        [SerializeField] private float lowSpeedTurnThreshold = 22f;
        [SerializeField] float maxSteeringAngle = 30f;
        [SerializeField] private float reverseSteeringAngle = 15f;
        [SerializeField] AnimationCurve turnCurve;
        [SerializeField] float turnStrength = 1500f;

        [Header("Braking and Drifting")] [SerializeField]
        float driftSteerMultiplier = 1.5f;

        [SerializeField] float brakeTorque = 10000f;

        [Header("Physics")] [SerializeField] Transform centerOfMass;
        [SerializeField] float downForce = 100f;
        [SerializeField] float gravity = Physics.gravity.y;
        [SerializeField] float lateralGScale = 10f;
        [SerializeField] float gravityMultiplierForAirborne = 5f;
        [SerializeField] float airControlMultiplier = 0.5f;

        [Header("Banking")] [SerializeField] float maxBankAngle = 5f;
        [SerializeField] float bankSpeed = 2f;

        [Header("Input")] [SerializeField] InputReader playerInput;

        [Header("References")] [SerializeField]
        Circuit circuit;

        [SerializeField] AIDriverData driverData;
        [SerializeField] CinemachineCamera playerCamera;
        [SerializeField] AudioListener playerAudioListener;

        IDrive input;
        Rigidbody rb;

        Vector3 kartVelocity;
        float brakeVelocity;
        float driftVelocity;

        Vector3 originalCenterOfMass;

        public bool IsGrounded = false;
        public Vector3 Velocity => kartVelocity;
        public float MaxSpeed => maxSpeed;

        [SerializeField] GameObject serverCube;
        [SerializeField] GameObject clientCube;

        [Header("Player Debug Info")] [SerializeField]
        TextMeshPro playerText;


        void Awake()
        {
            if (playerInput is IDrive driveInput)
            {
                input = driveInput;
            }

            rb = GetComponent<Rigidbody>();
            input.Enable();

            rb.centerOfMass = centerOfMass.localPosition;
            originalCenterOfMass = centerOfMass.localPosition;

            foreach (AxleInfo axleInfo in axleInfos)
            {
                axleInfo.originalForwardFriction = axleInfo.leftWheel.forwardFriction;
                axleInfo.originalSidewaysFriction = axleInfo.leftWheel.sidewaysFriction;
            }
        }

        public void SetInput(IDrive input)
        {
            this.input = input;
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

        void Update()
        {
            UpdateIsGrounded();


            playerText.SetText(
                $"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {kartVelocity.magnitude:F1}");
        }

        void FixedUpdate()
        {
            Move(input.Move);
        }


        void Move(Vector2 inputVector)
        {
            float verticalInput = AdjustInput(inputVector.y);
            float horizontalInput = AdjustInput(inputVector.x);


            float randomTorqueFactor = UnityEngine.Random.Range(0.95f, 1.05f);
            float motor = maxMotorTorque * verticalInput * speedRatio * randomTorqueFactor;
            float steering = maxSteeringAngle * horizontalInput;

            UpdateAxles(motor, steering);
            UpdateBanking(horizontalInput);

            kartVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            if (IsGrounded)
            {
                Debug.Log("negr");
                HandleGroundedMovement(verticalInput, motor);
            }
            else
            {
                HandleAirborneMovement(verticalInput, horizontalInput);
            }
        }


        void HandleGroundedMovement(float verticalInput, float motor)
        {
            if (Mathf.Abs(verticalInput) < 0.1f && rb.linearVelocity.magnitude > 0.1f && !input.IsBraking)
            {
                ApplyEngineBraking();
            }

            float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float targetSpeed = verticalInput * maxSpeed * (verticalInput < 0 ? 1 / speedRatio : 1);
            if (verticalInput < 0 && currentSpeed > 0)
            {
                targetSpeed = verticalInput * maxSpeed * speedRatio / 2;
            }
            else if (verticalInput < 0)
            {
                targetSpeed = verticalInput * maxSpeed * 1 / speedRatio;
            }
            else
            {
                targetSpeed = verticalInput * maxSpeed;
            }

            if (!input.IsBraking)
            {
                if (rb.linearVelocity.magnitude < Mathf.Abs(targetSpeed))
                {
                    rb.AddForce(transform.forward * motor, ForceMode.Acceleration);
                }
            }

            float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            float lateralG = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
            float downForceFactor = Mathf.Max(speedFactor, lateralG / lateralGScale);
            rb.AddForce(-transform.up * (downForce * rb.mass * downForceFactor));

            float speed = rb.linearVelocity.magnitude;
            Vector3 centerOfMassAdjustment = (speed > 10f)
                ? new Vector3(0f, 0f, Mathf.Abs(verticalInput) > 0.1f ? Mathf.Sign(verticalInput) * -0.5f : 0f)
                : Vector3.zero;
            rb.centerOfMass = originalCenterOfMass + centerOfMassAdjustment;
        }

        void HandleAirborneMovement(float verticalInput, float horizontalInput)
        {
            Vector3 downwardForce = Vector3.down * -gravity * gravityMultiplierForAirborne;

            rb.AddForce(downwardForce, ForceMode.Acceleration);

            Vector3 airControlForce = new Vector3(horizontalInput, 0, verticalInput) * airControlMultiplier;

            rb.AddForce(transform.TransformDirection(airControlForce), ForceMode.Acceleration);
        }


        void ApplyEngineBraking()
        {
            float speedFactor = rb.linearVelocity.magnitude / maxSpeed;
            Vector3 deceleration = -rb.linearVelocity.normalized * engineBrakingForce * speedFactor;
            rb.AddForce(deceleration, ForceMode.Acceleration);
        }

        void UpdateBanking(float horizontalInput)
        {
            float targetBankAngle = horizontalInput * -maxBankAngle;
            Vector3 currentEuler = transform.localEulerAngles;
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, targetBankAngle, Time.deltaTime * bankSpeed);
            transform.localEulerAngles = currentEuler;
        }

        void UpdateAxles(float motor, float steering)
        {
            foreach (AxleInfo axleInfo in axleInfos)
            {
                HandleSteering(axleInfo, steering);
                HandleMotor(axleInfo, motor);
                HandleBrakesAndDrift(axleInfo);
                UpdateWheelVisuals(axleInfo.leftWheel);
                UpdateWheelVisuals(axleInfo.rightWheel);
            }
        }

        void UpdateWheelVisuals(WheelCollider collider)
        {
            if (collider.transform.childCount == 0) return;

            Transform visualWheel = collider.transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

        void HandleSteering(AxleInfo axleInfo, float steeringInput)
        {
            float speed = axleInfo.rightWheel.rpm * axleInfo.rightWheel.radius * 2f * Mathf.PI / 10f;
            float speedFactor = Mathf.Clamp01(speed / maxSpeed);


            float adjustedTurnFactor = turnCurve.Evaluate(speedFactor);

            bool isMovingForward = Vector3.Dot(transform.forward, rb.linearVelocity) > 0;
            float effectiveSteeringAngle = isMovingForward ? maxSteeringAngle : reverseSteeringAngle;

            float targetSteeringAngle = steeringInput * effectiveSteeringAngle * adjustedTurnFactor;

            targetSteeringAngle +=
                Vector3.SignedAngle(transform.forward, rb.linearVelocity + transform.forward, Vector3.up);
            targetSteeringAngle = Mathf.Clamp(targetSteeringAngle, -effectiveSteeringAngle, effectiveSteeringAngle);

            if (axleInfo.steering)
            {
                float steeringMultiplier = input.IsBraking ? driftSteerMultiplier : 1f;
                axleInfo.leftWheel.steerAngle = targetSteeringAngle * steeringMultiplier;
                axleInfo.rightWheel.steerAngle = targetSteeringAngle * steeringMultiplier;
            }


            if (Mathf.Abs(steeringInput) > 0.1f && kartVelocity.magnitude > lowSpeedTurnThreshold && IsGrounded)
            {
                float angleBetween = Vector3.Angle(transform.forward, rb.linearVelocity);
                float baseDirectionMultiplier = isMovingForward ? 1f : -1f;
                float driftBlendFactor = Mathf.InverseLerp(driftAngleThreshold, maxDriftAngle, angleBetween);
                float directionMultiplier =
                    Mathf.Lerp(baseDirectionMultiplier, -baseDirectionMultiplier, driftBlendFactor);
                Vector3 desiredTurnDirection = Vector3.up *
                                               (steeringInput * turnPersistenceTorque * adjustedTurnFactor *
                                                directionMultiplier);
                rb.AddTorque(desiredTurnDirection, ForceMode.Acceleration);
            }
        }


        void HandleMotor(AxleInfo axleInfo, float motor)
        {
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }

        void HandleBrakesAndDrift(AxleInfo axleInfo)
        {
            if (axleInfo.motor)
            {
                if (input.IsBraking)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotationX;
                    Vector3 forwardDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                    Vector3 currentVelocity = rb.linearVelocity;
                    Vector3 forwardVelocity = Vector3.Project(currentVelocity, forwardDirection);
                    Vector3 sidewaysVelocity = currentVelocity - forwardVelocity;
                    forwardVelocity = Vector3.Lerp(forwardVelocity, Vector3.zero, Time.fixedDeltaTime);
                    sidewaysVelocity = Vector3.Lerp(sidewaysVelocity, sidewaysVelocity * 0.5f, Time.fixedDeltaTime);
                    rb.linearVelocity = forwardVelocity + sidewaysVelocity;

                    axleInfo.leftWheel.brakeTorque = brakeTorque;
                    axleInfo.rightWheel.brakeTorque = brakeTorque;
                    ApplyDriftFriction(axleInfo.leftWheel);
                    ApplyDriftFriction(axleInfo.rightWheel);
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;

                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.brakeTorque = 0;
                    ResetDriftFriction(axleInfo.leftWheel);
                    ResetDriftFriction(axleInfo.rightWheel);
                }
            }
        }

        void ResetDriftFriction(WheelCollider wheel)
        {
            AxleInfo axleInfo = axleInfos.FirstOrDefault(axle => axle.leftWheel == wheel || axle.rightWheel == wheel);
            if (axleInfo == null) return;

            wheel.forwardFriction = axleInfo.originalForwardFriction;
            wheel.sidewaysFriction = axleInfo.originalSidewaysFriction;
        }

        void ApplyDriftFriction(WheelCollider wheel)
        {
            if (wheel.GetGroundHit(out var hit))
            {
                wheel.forwardFriction = UpdateFriction(wheel.forwardFriction);
                wheel.sidewaysFriction = UpdateFriction(wheel.sidewaysFriction);
            }
        }

        WheelFrictionCurve UpdateFriction(WheelFrictionCurve friction)
        {
            friction.stiffness = input.IsBraking
                ? Mathf.SmoothDamp(friction.stiffness, .5f, ref driftVelocity, Time.deltaTime * 2f)
                : 1f;
            return friction;
        }

        float AdjustInput(float inputValue)
        {
            return inputValue switch
            {
                >= .7f => 1f,
                <= -.7f => -1f,
                _ => inputValue
            };
        }

        void UpdateIsGrounded()
        {
            foreach (AxleInfo axleInfo in axleInfos.Where(x => x.motor))
            {
                if (IsWheelGrounded(axleInfo.leftWheel) || IsWheelGrounded(axleInfo.rightWheel))
                {
                    IsGrounded = true;
                }
                else
                {
                    IsGrounded = false;
                }
            }
        }

        bool IsWheelGrounded(WheelCollider wheel)
        {
            return wheel.GetGroundHit(out _);
        }
    }
}