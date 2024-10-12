using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
using Utilities;

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

    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 inputVector;
        public Vector3 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref position);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref angularVelocity);
        }
    }

    public class KartController : NetworkBehaviour
    {
        [Header("Axle Information")] [SerializeField]
        private AxleInfo[] axleInfos;

        [Header("Motor Attributes")] [SerializeField]
        private float maxMotorTorque = 3000f;

        [SerializeField] private float maxSpeed;

        [Header("Steering Attributes")] [SerializeField]
        private float maxSteeringAngle = 30f;

        [SerializeField] private AnimationCurve turnCurve;
        [SerializeField] private float turnStrength = 1500f;

        [Header("Braking and Drifting")] [SerializeField]
        private float brakeTorque = 10000f;

        private float currentBrakeTorque = 0f;
        [SerializeField] private float brakeTorqueIncreaseRate = 5000f;
        [SerializeField] private float driftSteerMultiplier = 1.5f;

        [Header("Physics")] [SerializeField] private Transform centerOfMass;
        [SerializeField] private float downForce = 100f;
        [SerializeField] private float gravity = Physics.gravity.y;
        [SerializeField] float lateralGScale = 10f;

        [Header("Banking")] [SerializeField] private float maxBankAngle = 5f;
        [SerializeField] private float bankSpeed = 2f;

        [Header("Refs")] [SerializeField] private InputReader playerInput;
        [SerializeField] private Circuit circuit;
        [SerializeField] private AIDriverData driverData;
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private AudioListener playerAudioListener;

        private IDrive input;
        private Rigidbody rb;

        private Vector3 kartVelocity;
        private float brakeVelocity;
        private float driftVelocity;

        private RaycastHit hit;
        private const float thresholdSpeed = 10f;
        private const float centerOfMassOffset = -0.5f;
        private Vector3 originalCenterOfMass;

        public bool IsGrounded = true;
        public Vector3 Velocity => kartVelocity;
        public float MaxSpeed => maxSpeed;

        private NetworkTimer timer;
        private const float k_ServerTickRate = 60f;
        private const int k_bufferSize = 1024;

        private CircularBuffer<StatePayload> clientStateBuffer;
        private CircularBuffer<InputPayload> clientInputBuffer;
        private StatePayload lastServerState;
        private StatePayload lastProcessedState;

        private CircularBuffer<StatePayload> serverStateBuffer;
        private Queue<InputPayload> serverInputQueue;

        [Header("Netcode")] [SerializeField] private float reconciliationCooldownTime = 1f;
        [SerializeField] private float reconciliationThreshold = 10f;
        [SerializeField] private GameObject serverCube;
        [SerializeField] private GameObject clientCube;
        
        private CountdownTimer reconciliationCooldown;

        [Header("Netcode Debug")]
        [SerializeField] TextMeshPro networkText;
        [SerializeField] TextMeshPro playerText;
        [SerializeField] TextMeshPro serverRpcText;
        [SerializeField] TextMeshPro clientRpcText;
        
        private void Awake()
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

            timer = new NetworkTimer(k_ServerTickRate);
            clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

            serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            serverInputQueue = new Queue<InputPayload>();

            reconciliationCooldown = new CountdownTimer(reconciliationCooldownTime);

            /*       else
                   {
                       Debug.LogError("Using AI INPUT System");
                       var aiInput = gameObject.GetOrAdd<AIInput>();
                       aiInput.AddDriverData(driverData);
                       aiInput.AddCircuit(circuit);
                       input = aiInput;
                   }*/
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                playerCamera.Priority = 0;
                playerAudioListener.enabled = false;
                return;
            }
            
            networkText.SetText($"Player {NetworkManager.LocalClientId} Host: {NetworkManager.IsHost} Server: {IsServer} Client: {IsClient}");
            if (!IsServer) serverRpcText.SetText("Not Server");
            if (!IsClient) clientRpcText.SetText("Not Client");

            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;
        }

        private void Update()
        {
            timer.Update(Time.deltaTime);
            reconciliationCooldown.Tick(Time.deltaTime);
            
            playerText.SetText($"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {kartVelocity.magnitude:F1}");
            if (Input.GetKeyDown(KeyCode.Q))
            {
                transform.position = transform.forward * 20f;
            }
        }


        private void FixedUpdate()
        {
            while (timer.ShouldTick())
            {
                HandleClientTick();
                HandleServerTick();
            }
        }

        void HandleServerTick()
        {
            if (!IsServer) return;
             
            var bufferIndex = -1;
            InputPayload inputPayload = default;
            while (serverInputQueue.Count > 0) {
                inputPayload = serverInputQueue.Dequeue();
                
                bufferIndex = inputPayload.tick % k_bufferSize;
                
                StatePayload statePayload = ProcessMovement(inputPayload);
                serverCube.transform.position = statePayload.position.With(y: 4);
                serverStateBuffer.Add(statePayload, bufferIndex);
            }
            
            if (bufferIndex == -1) return;
            SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        }
        

        [ClientRpc]
        void SendToClientRpc(StatePayload statePayload) {
            clientRpcText.SetText($"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position}"); 
            serverCube.transform.position = statePayload.position.With(y: 4);
            if (!IsOwner) return;
            lastServerState = statePayload;
        }

        void HandleClientTick()
        {
            if (!IsClient || !IsOwner) return;

            var currentTick = timer.CurrentTick;
            var bufferIndex = currentTick % k_bufferSize;
            
            InputPayload inputPayload = new InputPayload() {
                tick = currentTick,
                inputVector = input.Move,
                position = transform.position
            };
            
            clientInputBuffer.Add(inputPayload, bufferIndex);
            SendToServerRpc(inputPayload);
            
            StatePayload statePayload = ProcessMovement(inputPayload);
            clientCube.transform.position = statePayload.position.With(y: 4);
            clientStateBuffer.Add(statePayload, bufferIndex);
            
            HandleServerReconciliation();
        }

        void HandleServerReconciliation()
        {
            if (!ShouldReconcile()) return;

            float positionError;
            int bufferIndex;
            
            bufferIndex = lastServerState.tick % k_bufferSize;
            if (bufferIndex - 1 < 0) return; 
            
            StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; 
            StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
            positionError = Vector3.Distance(rewindState.position, clientState.position);

            if (positionError > reconciliationThreshold) {
                ReconcileState(rewindState);
                reconciliationCooldown.Start();
            }

            lastProcessedState = rewindState;
        }

        private void ReconcileState(StatePayload rewindState)
        {
            transform.position = rewindState.position;
            transform.rotation = rewindState.rotation;
            rb.velocity = rewindState.velocity;
            rb.angularVelocity = rewindState.angularVelocity;

            if (!rewindState.Equals(lastServerState)) return;
            
            clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);
            
            int tickToReplay = lastServerState.tick;

            while (tickToReplay < timer.CurrentTick) {
                int bufferIndex = tickToReplay % k_bufferSize;
                StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
                clientStateBuffer.Add(statePayload, bufferIndex);
                tickToReplay++;
            }
        }

        private bool ShouldReconcile()
        {
            bool isNewServerState = !lastServerState.Equals(default);
            bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) 
                                                   || !lastProcessedState.Equals(lastServerState);

            return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationCooldown.IsRunning;
        }

        [ServerRpc]
        void SendToServerRpc(InputPayload input)
        {
            serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
            clientCube.transform.position = input.position.With(y: 4);
            serverInputQueue.Enqueue(input);
        }

        StatePayload ProcessMovement(InputPayload input)
        {
            Move(input.inputVector);

            return new StatePayload()
            {
                tick = input.tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rb.velocity,
                angularVelocity = rb.angularVelocity
            };
        }

        void Move(Vector2 inputVector)
        {
            float verticalInput = AdjustInput(input.Move.y);
            float horizontalInput = AdjustInput(input.Move.x);

            float motor = maxMotorTorque * verticalInput;
            float steering = maxSteeringAngle * horizontalInput;

            UpdateAxles(motor, steering);
            UpdateBanking(horizontalInput);

            kartVelocity = transform.InverseTransformDirection(rb.velocity);

            if (IsGrounded)
            {
                HandleGroundedMovement(verticalInput, horizontalInput);
            }
            else
            {
                HandleAirborneMovement(verticalInput, horizontalInput);
            }
        }

        private void HandleGroundedMovement(float verticalInput, float horizontalInput)
        {
            if (Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(kartVelocity.z) > 1)
            {
                float turnMultiplier = Mathf.Clamp01(turnCurve.Evaluate(kartVelocity.magnitude / maxSpeed));
                rb.AddTorque(Vector3.up * horizontalInput * Mathf.Sign(kartVelocity.z) * turnStrength * 100f *
                             turnMultiplier);
            }

            if (!input.IsBraking)
            {
                float targetSpeed = verticalInput * maxSpeed;
                Vector3 forwardWithoutY = transform.forward.With(y: 0).normalized;
                rb.velocity = Vector3.Lerp(rb.velocity, forwardWithoutY * targetSpeed, timer.MinTimeBetweenTicks);
            }

            float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
            float lateralG = Mathf.Abs(Vector3.Dot(rb.velocity, transform.right));
            float downForceFactor = Mathf.Max(speedFactor, lateralG / lateralGScale);
            rb.AddForce(-transform.up * downForce * rb.mass * downForceFactor);

            float speed = rb.velocity.magnitude;
            Vector3 centerOfMassAdjustment = (speed > thresholdSpeed)
                ? new Vector3(0f, 0f,
                    Mathf.Abs(verticalInput) > 0.1f ? Mathf.Sign(verticalInput) * centerOfMassOffset : 0f)
                : Vector3.zero;
            rb.centerOfMass = originalCenterOfMass + centerOfMassAdjustment;
        }

        private void UpdateBanking(float horizontalInput)
        {
            float targetBankAngle = horizontalInput * -maxBankAngle;
            Vector3 currentEuler = transform.localEulerAngles;
            currentEuler.z = Mathf.LerpAngle(currentEuler.z, targetBankAngle, Time.deltaTime * bankSpeed);
            transform.localEulerAngles = currentEuler;
        }

        private void HandleAirborneMovement(float verticalInput, float horizontalInput)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity + Vector3.down * gravity, Time.deltaTime * gravity);
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

        private void HandleSteering(AxleInfo axleInfo, float steering)
        {
            if (axleInfo.steering)
            {
                float steeringMultiplier = input.IsBraking ? driftSteerMultiplier : 1f;
                axleInfo.leftWheel.steerAngle = steering * steeringMultiplier;
                axleInfo.rightWheel.steerAngle = steering * steeringMultiplier;
            }
        }

        private void HandleMotor(AxleInfo axleInfo, float motor)
        {
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }

        private void HandleBrakesAndDrift(AxleInfo axleInfo)
        {
            if (axleInfo.motor)
            {
                if (input.IsBraking)
                {
                    // Apply ABS to prevent wheel lock-up
                    axleInfo.leftWheel.brakeTorque = brakeTorque;
                    axleInfo.rightWheel.brakeTorque = brakeTorque;

                    // rb.constraints = RigidbodyConstraints.FreezePositionX;
                    float newZ = Mathf.SmoothDamp(rb.velocity.z, 0, ref brakeVelocity, 1f);
                    rb.velocity = rb.velocity.With(z: newZ);

                    ApplyDriftFriction(axleInfo.leftWheel);
                    ApplyDriftFriction(axleInfo.rightWheel);
                }
                else
                {
                    //rb.constraints = RigidbodyConstraints.None;
                    // Remove brake torque
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.brakeTorque = 0;

                    // Reset friction to original values
                    ResetDriftFriction(axleInfo.leftWheel);
                    ResetDriftFriction(axleInfo.rightWheel);
                }
            }
        }

        private void ApplyABS(WheelCollider wheel)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                float slipRatio = Mathf.Abs(hit.forwardSlip);
                float absSlipThreshold = 0.3f;

                if (slipRatio >= absSlipThreshold)
                {
                    // Reduce brake torque to prevent lock-up
                    currentBrakeTorque =
                        Mathf.MoveTowards(currentBrakeTorque, 0, brakeTorqueIncreaseRate * Time.deltaTime);
                }
                else
                {
                    // Gradually increase brake torque
                    currentBrakeTorque = Mathf.MoveTowards(currentBrakeTorque, brakeTorque,
                        brakeTorqueIncreaseRate * Time.deltaTime);
                }

                wheel.brakeTorque = currentBrakeTorque;
            }
            else
            {
                wheel.brakeTorque = 0;
            }
        }

        private void ApplyDriftFriction(WheelCollider wheel)
        {
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

            // Increase stiffness to improve grip
            forwardFriction.stiffness = Mathf.Lerp(forwardFriction.stiffness, 2.0f, Time.deltaTime * 5f);
            sidewaysFriction.stiffness = Mathf.Lerp(sidewaysFriction.stiffness, 2.0f, Time.deltaTime * 5f);

            wheel.forwardFriction = forwardFriction;
            wheel.sidewaysFriction = sidewaysFriction;
        }

        private void ResetDriftFriction(WheelCollider wheel)
        {
            AxleInfo axleInfo = axleInfos.FirstOrDefault(axle => axle.leftWheel == wheel || axle.rightWheel == wheel);
            if (axleInfo == null) return;

            // Smoothly reset stiffness to original values
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

            forwardFriction.stiffness = Mathf.Lerp(forwardFriction.stiffness,
                axleInfo.originalForwardFriction.stiffness, Time.deltaTime * 5f);
            sidewaysFriction.stiffness = Mathf.Lerp(sidewaysFriction.stiffness,
                axleInfo.originalSidewaysFriction.stiffness, Time.deltaTime * 5f);

            wheel.forwardFriction = forwardFriction;
            wheel.sidewaysFriction = sidewaysFriction;
        }

        private WheelFrictionCurve UpdateFriction(WheelFrictionCurve friction)
        {
            friction.stiffness = input.IsBraking
                ? Mathf.SmoothDamp(friction.stiffness, .5f, ref driftVelocity, Time.deltaTime * 2f)
                : 1f;
            return friction;
        }

        public void SetInput(IDrive input)
        {
            this.input = input;
        }

        float AdjustInput(float input)
        {
            return input switch
            {
                >= .7f => 1f,
                <= -.7f => -1f,
                _ => input
            };
        }
    }
}