using System;
using System.Collections.Generic;
using System.Linq;
using Smooth;
using TMPro;
using Unity.Cinemachine;
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
        public DateTime timestamp;
        public ulong networkObjectId;
        public Vector3 inputVector;
        public Vector3 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref timestamp);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref inputVector);
            serializer.SerializeValue(ref position);
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public ulong networkObjectId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public float ping;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref networkObjectId);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref angularVelocity);
            serializer.SerializeValue(ref ping);
        }
    }

    public class KartController : NetworkBehaviour
    {
        [Header("Axle Information")] [SerializeField]
        AxleInfo[] axleInfos;

        [Header("Motor Attributes")] [SerializeField]
        float maxMotorTorque = 3000f;

        [SerializeField] float maxSpeed;

        [Header("Steering Attributes")] [SerializeField]
        float maxSteeringAngle = 30f;

        [SerializeField] AnimationCurve turnCurve;
        [SerializeField] float turnStrength = 1500f;

        [Header("Braking and Drifting")] [SerializeField]
        float driftSteerMultiplier = 1.5f; 

        [SerializeField] float brakeTorque = 10000f;

        [Header("Physics")] [SerializeField] Transform centerOfMass;
        [SerializeField] float downForce = 100f;
        [SerializeField] float gravity = Physics.gravity.y;
        [SerializeField] float lateralGScale = 10f; 

        [Header("Banking")] [SerializeField] float maxBankAngle = 5f;
        [SerializeField] float bankSpeed = 2f;

        [Header("Refs")] [SerializeField] InputReader playerInput;
        [SerializeField] Circuit circuit;
        [SerializeField] AIDriverData driverData;
        [SerializeField] CinemachineCamera playerCamera;
        [SerializeField] AudioListener playerAudioListener;

        IDrive input;
        Rigidbody rb;

        Vector3 kartVelocity;
        float brakeVelocity;
        float driftVelocity;

        Vector3 originalCenterOfMass;

        public bool IsGrounded = true;
        public Vector3 Velocity => kartVelocity;
        public float MaxSpeed => maxSpeed;


        NetworkTimer networkTimer;
        const float k_serverTickRate = 60f; 
        const int k_bufferSize = 1024;


        CircularBuffer<StatePayload> clientStateBuffer;
        CircularBuffer<InputPayload> clientInputBuffer;
        StatePayload lastServerState;
        StatePayload lastProcessedState;
        
        CircularBuffer<StatePayload> serverStateBuffer;
        Queue<InputPayload> serverInputQueue;

        [Header("Netcode")] [SerializeField] float reconciliationCooldownTime = 1f;
        [SerializeField] float reconciliationThreshold = 10f;
        [SerializeField] GameObject serverCube;
        [SerializeField] GameObject clientCube;

        CountdownTimer reconciliationTimer;

        [Header("Netcode Debug")] [SerializeField]
        TextMeshPro networkText;

        [SerializeField] TextMeshPro playerText;
        [SerializeField] TextMeshPro serverRpcText;
        [SerializeField] TextMeshPro clientRpcText;

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

            networkTimer = new NetworkTimer(k_serverTickRate);
            clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

            serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
            serverInputQueue = new Queue<InputPayload>();

            reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
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

            networkText.SetText(
                $"Player {NetworkManager.LocalClientId} Host: {NetworkManager.IsHost} Server: {IsServer} Client: {IsClient}");
            if (!IsServer) serverRpcText.SetText("Not Server");
            if (!IsClient) clientRpcText.SetText("Not Client");

            playerCamera.Priority = 100;
            playerAudioListener.enabled = true;
        }

        void Update()
        {
            networkTimer.Update(Time.deltaTime);
            reconciliationTimer.Tick(Time.deltaTime);

            playerText.SetText(
                $"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {kartVelocity.magnitude:F1}");
            if (Input.GetKeyDown(KeyCode.Q))
            {
                transform.position += transform.forward * 20f;
            }
        }

        void FixedUpdate()
        {
            while (networkTimer.ShouldTick())
            {
                HandleClientTick();
                HandleServerTick();
            }
        }

        void HandleServerTick()
        {
            if (!IsServer) return;

            while (serverInputQueue.Count > 0)
            {
                InputPayload inputPayload = serverInputQueue.Dequeue();

                float ping = (float)(DateTime.Now - inputPayload.timestamp).TotalMilliseconds;

                int bufferIndex = inputPayload.tick % k_bufferSize;

                if (inputPayload.networkObjectId == NetworkObjectId && IsOwner)
                {
                    StatePayload statePayload = new StatePayload()
                    {
                        tick = inputPayload.tick,
                        networkObjectId = NetworkObjectId,
                        position = transform.position,
                        rotation = transform.rotation,
                        velocity = rb.linearVelocity,
                        angularVelocity = rb.angularVelocity,
                        ping = ping
                    };
                    serverStateBuffer.Add(statePayload, bufferIndex);
                    SendToClientRpc(statePayload);
                    continue;
                }
                
                StatePayload clientStatePayload = ProcessMovement(inputPayload);
                clientStatePayload.ping = ping;
                serverStateBuffer.Add(clientStatePayload, bufferIndex);
                SendToClientRpc(clientStatePayload);
            }
        }

        void HandleClientTick()
        {
            if (!IsOwner) return;

            var currentTick = networkTimer.CurrentTick;
            var bufferIndex = currentTick % k_bufferSize;

            InputPayload inputPayload = new InputPayload()
            {
                tick = currentTick,
                timestamp = DateTime.Now,
                networkObjectId = NetworkObjectId,
                inputVector = input.Move,
                position = transform.position
            };

            clientInputBuffer.Add(inputPayload, bufferIndex);
            SendToServerRpc(inputPayload);

            StatePayload statePayload = ProcessMovement(inputPayload);
            clientStateBuffer.Add(statePayload, bufferIndex);

            HandleServerReconciliation();
        }

        [ClientRpc]
        void SendToClientRpc(StatePayload statePayload)
        {
            clientRpcText.SetText(
                $"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position} Ping: {statePayload.ping}");
            serverCube.transform.position = statePayload.position.With(y: 4);
            if (!IsOwner) return;
            lastServerState = statePayload;
        }

        [ServerRpc(RequireOwnership = false)]
        void SendToServerRpc(InputPayload input)
        {
            serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
            clientCube.transform.position = input.position.With(y: 4);
            serverInputQueue.Enqueue(input);
        }

        bool ShouldReconcile()
        {
            bool isNewServerState = !lastServerState.Equals(default);
            bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
                                                   || !lastProcessedState.Equals(lastServerState);

            return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning;
        }

        void HandleServerReconciliation()
        {
            if (!ShouldReconcile()) return;

            float positionError;
            int bufferIndex;

            bufferIndex = lastServerState.tick % k_bufferSize;
            if (bufferIndex < 0) return; 

            StatePayload rewindState = lastServerState;
            StatePayload clientState = clientStateBuffer.Get(bufferIndex);
            positionError = Vector3.Distance(rewindState.position, clientState.position);

            if (positionError > reconciliationThreshold)
            {
                ReconcileState(rewindState);
                reconciliationTimer.Start();
            }

            lastProcessedState = rewindState;
        }

        void ReconcileState(StatePayload rewindState)
        {
            transform.position = rewindState.position;
            transform.rotation = rewindState.rotation;
            rb.linearVelocity = rewindState.velocity;
            rb.angularVelocity = rewindState.angularVelocity;

            clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);
            
            int tickToReplay = rewindState.tick + 1; 

            while (tickToReplay <= networkTimer.CurrentTick)
            {
                int bufferIndex = tickToReplay % k_bufferSize;
                InputPayload inputPayload = clientInputBuffer.Get(bufferIndex);
                StatePayload statePayload = ProcessMovement(inputPayload);
                clientStateBuffer.Add(statePayload, bufferIndex);
                tickToReplay++;
            }
        }

        StatePayload ProcessMovement(InputPayload inputPayload)
        {
            bool shouldProcessMovement = false;

            if (IsOwner)
            {
                shouldProcessMovement = true;
            }
            else if (IsServer)
            {
                shouldProcessMovement = true;
            }

            if (shouldProcessMovement)
            {
                Move(inputPayload.inputVector);
            }

            return new StatePayload()
            {
                tick = inputPayload.tick,
                networkObjectId = NetworkObjectId,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rb.linearVelocity,
                angularVelocity = rb.angularVelocity
            };
        }

        void Move(Vector2 inputVector)
        {
            float verticalInput = AdjustInput(inputVector.y);
            float horizontalInput = AdjustInput(inputVector.x);

            float motor = maxMotorTorque * verticalInput;
            float steering = maxSteeringAngle * horizontalInput;

            UpdateAxles(motor, steering);
            UpdateBanking(horizontalInput);

            kartVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            if (IsGrounded)
            {
                HandleGroundedMovement(verticalInput, horizontalInput);
            }
            else
            {
                HandleAirborneMovement(verticalInput, horizontalInput);
            }
        }

        void HandleGroundedMovement(float verticalInput, float horizontalInput)
        {
            if (Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(kartVelocity.z) > 1)
            {
                float turnMultiplier = Mathf.Clamp01(turnCurve.Evaluate(kartVelocity.magnitude / maxSpeed));
                rb.AddTorque(Vector3.up *
                             (horizontalInput * Mathf.Sign(kartVelocity.z) * turnStrength * 100f * turnMultiplier));
            }
            
            if (!input.IsBraking)
            {
                float targetSpeed = verticalInput * maxSpeed;
                Vector3 forwardWithoutY = transform.forward.With(y: 0).normalized;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, forwardWithoutY * targetSpeed,
                    networkTimer.MinTimeBetweenTicks);
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
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
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

        void HandleSteering(AxleInfo axleInfo, float steering)
        {
            if (axleInfo.steering)
            {
                float steeringMultiplier = input.IsBraking ? driftSteerMultiplier : 1f;
                axleInfo.leftWheel.steerAngle = steering * steeringMultiplier;
                axleInfo.rightWheel.steerAngle = steering * steeringMultiplier;
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

                    float newZ = Mathf.SmoothDamp(rb.linearVelocity.z, 0, ref brakeVelocity, 1f);
                    rb.linearVelocity = rb.linearVelocity.With(z: newZ);

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
                IsGrounded = true;
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
    }
}
