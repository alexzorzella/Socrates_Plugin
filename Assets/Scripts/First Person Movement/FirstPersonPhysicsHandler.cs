// Adapted from Liam Rousselle's 2025 QuakeCharacterController.cs
// https://github.com/LiamRousselle/Unity-Quake-Movement

using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonMovement {
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonPhysicsHandler : MonoBehaviour, JumpStateListener, StateMachineListener {
        CharacterController characterController;

        float height;
        
        [Header("Movement")]
        [SerializeField] [Range(0, 30)] float maxWalkSpeed = 14f;
        [SerializeField] [Range(1, 5)] float runSpeedMultiplier = 2F;
        [SerializeField] [Range(0, 30)] float jumpPower = 9.64f;
        [SerializeField] [Range(0, 1)] float crouchSpeedMultiplier = 0.5F;
        [SerializeField] [Range(0, 1)] float crouchHeightMultiplier = 0.5F;
        
        bool running;
        bool wallrunning;
        bool crouching;
        
        [Header("External Forces")]
        [SerializeField] [Range(0, 15)] float friction = 4.0f;
        [SerializeField] [Range(0, 50)] float gravity = 28.6f; 

        [Header("Collision")]
        [SerializeField] LayerMask groundMask;
        
        [Header("Acceleration Settings")]
        [SerializeField] [Range(0, 25)] float groundAcceleration = 10.0f;
        [SerializeField] [Range(0, 25)] float airAcceleration = 10.0f;
        [SerializeField] [Range(0, 10)] float minAirSpeed = 1.07f;
        [SerializeField] [Range(0, 10)] float stopSpeed = 3.57f;

        [Header("Grounded State")] [SerializeField] Transform groundCheckTransform;
        [SerializeField] float groundCheckRadius = 0.5F;
        bool isGrounded;

        [SerializeField] Transform ceilingCheckTransform;
        [SerializeField] float ceilingCheckRadius = 0.5F;
        bool collidingWithCeiling;
        
        Vector3 desiredMoveDirection = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        
        static readonly Vector3 xzPlane = new(1.0f, 0.0f, 1.0f);
        
        readonly List<FirstPersonPhysicsListener> movementListeners = new();

        public void RegisterMovementListener(FirstPersonPhysicsListener listener) {
            movementListeners.Add(listener);
        }
        
        void NotifyGroundedState() {
            foreach (FirstPersonPhysicsListener listener in movementListeners) { listener.NotifyGroundedState(isGrounded); }
        }

        void Awake() {
            characterController = GetComponent<CharacterController>();
            height = characterController.height;
        }
        
        float GetMovementSpeed() {
            float result = maxWalkSpeed;

            if (running) {
                result *= runSpeedMultiplier;
            } else if (crouching) {
                result *= crouchSpeedMultiplier;
            }
            
            return result;
        }
        
        void EvaluateOnGround() {
            bool isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundMask.value);

            if (this.isGrounded != isGrounded) {
                this.isGrounded = isGrounded;
                NotifyGroundedState();
            }
        }

        void OnCrouchStateChanged() {
            characterController.height = (crouching ? height * crouchHeightMultiplier : height);
        }
        
        public void UpdateMovement(Vector3 desiredMoveDirection, float deltaTime) {
            this.desiredMoveDirection = desiredMoveDirection * GetMovementSpeed();
            
            EvaluateOnGround();
            
            ApplyGravity(deltaTime);
            ApplyFriction(deltaTime);

            Accelerate(deltaTime, isGrounded ? groundAcceleration : airAcceleration);

            characterController.Move(velocity * deltaTime);
        }

        void ApplyGravity(float deltaTime) {
            if (!isGrounded) {
                velocity.y -= deltaTime * gravity * (wallrunning && velocity.y < 0 ? 0.2F : 1);
            }
            
            bool currentlyCollidingWithCeiling =
                Physics.CheckSphere(ceilingCheckTransform.position, ceilingCheckRadius, groundMask.value);
                
            if (collidingWithCeiling != currentlyCollidingWithCeiling) {
                collidingWithCeiling = currentlyCollidingWithCeiling;

                if (collidingWithCeiling) {
                    velocity.y = 0;
                }
            }
        }

        void ApplyFriction(float deltaTime) {
            float speed = Vector3.Scale(xzPlane, velocity).magnitude;
            if (speed < 0.01f) {
                velocity = Vector3.Scale(velocity, Vector3.up);
                return;
            }

            float drop = 0.0f;
            if (isGrounded) {
                float control = speed < stopSpeed ? stopSpeed : speed;
                drop += control * friction * deltaTime;
            }

            float newSpeed = Mathf.Max(speed - drop, 0.0f) / speed;
            velocity *= newSpeed;
        }
        
        void Accelerate(float deltaTime, float acceleration) {
            float alignment = Vector3.Dot(velocity, desiredMoveDirection.normalized);
            float minAcceleration = GetMovementSpeed() - alignment;
            
            if (minAcceleration <= 0)
                return;

            float finalAcceleration = Mathf.Min(acceleration * deltaTime * GetMovementSpeed(), minAcceleration);
            velocity += finalAcceleration * Vector3.Scale(desiredMoveDirection.normalized, xzPlane);
        }

        public Vector3 GetVelocity() {
            return velocity;
        }
        
        public bool IsGrounded() {
            return isGrounded;
        }
        
        public void NotifyOfJumpState(JumpStateEvent jumpStateEvent) {
            switch (jumpStateEvent) {
                case JumpStateEvent.BEGIN_JUMP:
                    velocity.y = jumpPower;
                    break;
                case JumpStateEvent.BEGIN_FALL:
                    break;
            }
        }

        public void OnStateMachineStateChange(StateMachineState from, StateMachineState to) {
            running = to.GetName() == "run";
            wallrunning = to.GetName() == "wallrunning";
            crouching = to.GetName() == "crouching";
            
            if((from != null && from.GetName() == "crouching") || to.GetName() == "crouching") {
                OnCrouchStateChanged();
            }
        }
        
        void OnDrawGizmos() {
            Gizmos.DrawSphere(groundCheckTransform.position, groundCheckRadius);
            Gizmos.DrawSphere(ceilingCheckTransform.position, ceilingCheckRadius);
        }

        public void JumpAwayFrom(Vector3 normal) {
            velocity = normal * jumpPower;
            velocity.y = jumpPower;
        }
    }
}