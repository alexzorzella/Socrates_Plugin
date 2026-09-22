using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonMovement {
    [RequireComponent(typeof(FirstPersonPhysicsHandler))]
    public class FirstPersonStateHandler : MonoBehaviour, FirstPersonPhysicsListener, StateMachineListener, JumpStateListener {
        FirstPersonPhysicsHandler firstPersonCharacterMovement;

        FirstPersonCamera firstPersonCamera;
        FirstPersonCameraBob firstPersonCameraBob;
        
        StateMachine stateMachine;
        JumpState jumpState;
        
        [Header("Jumping")]
        [SerializeField] [Range(0, 1)] float jumpLatency = 0.15F;
        float currentJumpLatency;

        [SerializeField] [Range(0, 5)] int jumps = 2;

        [Header("Wallrunning")]
        [SerializeField] bool infiniteWalljumps; 
        [SerializeField] [Range(0, 10)] int walljumps = 1;
        int currentWalljumps;
        
        bool running;
        
        [SerializeField] Vector3 wallrunRayOffset;
        [SerializeField] float wallrunRayLength = 1F;
        [SerializeField] LayerMask wallMask;
        
        bool wallrunLeft;
        bool wallrunRight;
        bool wallrunFront;
        bool wallrunBack;

        RaycastHit leftWall;
        RaycastHit rightWall;
        RaycastHit frontWall;
        RaycastHit backWall;

        bool wallrunning;
        
        void Awake() {
            Initialize();
        }
        
        void Initialize() {
            firstPersonCamera = GetComponentInChildren<FirstPersonCamera>();
            firstPersonCameraBob = firstPersonCamera.GetComponent<FirstPersonCameraBob>();

            if (firstPersonCamera == null || firstPersonCameraBob == null) {
                string firstPersonCameraOk = firstPersonCamera != null ? "(ok) " : "";
                string cameraBobOk = firstPersonCameraBob != null ? "(ok) " : "";
                Debug.LogError($"FirstPersonStateHandler requires a child with a FirstPersonCamera {firstPersonCameraOk}and CameraBob {cameraBobOk}component");
                return;
            }
            
            firstPersonCharacterMovement = GetComponent<FirstPersonPhysicsHandler>();
            firstPersonCharacterMovement.RegisterMovementListener(this);

            currentWalljumps = walljumps;
            
            CreateStateMachine();
        }
        
        void CreateStateMachine() {
            StateMachineState idle = new("idle");

            StateMachineState walk = new("walk");
            StateMachineState run = new("run");

            StateMachineState jump = new("jump");
            StateMachineState fall = new("fall");
            
            StateMachineState wallrun = new("wallrunning");
            
            StateMachineState crouch = new("crouching");
            
            idle.AddTransition(StateMachineEvent.WALK, walk);
            idle.AddTransition(StateMachineEvent.RUN, run);
            idle.AddTransition(StateMachineEvent.JUMP, jump);
            idle.AddTransition(StateMachineEvent.CROUCH, crouch);
            idle.AddTransition(StateMachineEvent.FALL, fall);
            idle.AddEntryTransition((object o) => Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0, walk);

             walk.AddTransition(StateMachineEvent.STOP, idle);
             walk.AddTransition(StateMachineEvent.JUMP, jump);
             walk.AddTransition(StateMachineEvent.CROUCH, crouch);
             walk.AddTransition(StateMachineEvent.RUN, run);
             walk.AddTransition(StateMachineEvent.FALL, fall);
             walk.AddEntryTransition((object o) => Input.GetKey(KeyCode.LeftShift), run);
             
             run.AddTransition(StateMachineEvent.STOP, idle);
             run.AddTransition(StateMachineEvent.JUMP, jump);
             run.AddTransition(StateMachineEvent.CROUCH, crouch);
             run.AddTransition(StateMachineEvent.FALL, fall);
             run.AddTransition(StateMachineEvent.STOP_RUNNING, walk);
             
            jump.AddTransition(StateMachineEvent.LAND, idle);
            jump.AddTransition(StateMachineEvent.FALL, fall);
            jump.AddTransition(StateMachineEvent.WALLRUN, wallrun);

            fall.AddTransition(StateMachineEvent.LAND, idle);
            fall.AddTransition(StateMachineEvent.WALLRUN, wallrun);
            fall.AddEntryTransition((object o) => firstPersonCharacterMovement.IsGrounded(), idle);
            
            wallrun.AddTransition(StateMachineEvent.JUMP, jump);
            wallrun.AddTransition(StateMachineEvent.LAND, idle);
            wallrun.AddTransition(StateMachineEvent.STOP_WALLRUNNING, fall);
            
            crouch.AddTransition(StateMachineEvent.UN_CROUCH, idle);
            
            List<StateMachineState> states = new() { idle, walk, run, wallrun, jump, fall, crouch }; 
            stateMachine = new StateMachine.Builder("Movement").WithStates(states).Build();

            stateMachine.RegisterListener(this);
            stateMachine.RegisterListener(firstPersonCameraBob);
            stateMachine.RegisterListener(firstPersonCharacterMovement);

            jumpState = new JumpState(stateMachine, jumps);
            
            jumpState.RegisterListener(firstPersonCharacterMovement);
            jumpState.RegisterListener(this);
        }

        bool IsAscending() {
            return jumpState != null && jumpState.IsAscending();
        }

        void OnJump() {
            jumpState.StartJump();
        }
            
        void OnFall() {
            jumpState.AddFall();
        }

        void OnLand() {
            stateMachine.Handle(StateMachineEvent.LAND);
            jumpState.Land();
            currentWalljumps = walljumps;
        }
            
        void HandleJumpState() {
            bool grounded = firstPersonCharacterMovement.IsGrounded();
            
            if (currentJumpLatency > 0) { currentJumpLatency -= Time.deltaTime; }
            
            if (grounded && currentJumpLatency > 0) {
                currentJumpLatency = 0;
                stateMachine.Handle(StateMachineEvent.JUMP);
            }

            if (IsAscending()) {
                if (firstPersonCharacterMovement.GetVelocity().y < 0) {
                    OnFall();
                }
            }
        }

        void HandleArialMovementInput() {
            bool grounded = firstPersonCharacterMovement.IsGrounded();
            
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (!wallrunning) {
                    if (grounded) {
                        currentJumpLatency = jumpLatency;
                    } else {
                        if (!jumpState.TryAddJump()) {
                            currentJumpLatency = jumpLatency;
                        }
                    }
                } else {
                    Vector3 wallNormal = Vector3.zero;

                    if (wallrunLeft) { wallNormal = leftWall.normal; }
                    else if (wallrunRight) { wallNormal = rightWall.normal; }
                    else if (wallrunFront) { wallNormal = frontWall.normal; }
                    else if (wallrunBack) { wallNormal = backWall.normal; }

                    if (currentWalljumps > 0 || infiniteWalljumps) {
                        firstPersonCharacterMovement.JumpAwayFrom(wallNormal);
                        currentWalljumps--;
                    }
                }
            }
            
            if (Input.GetKeyUp(KeyCode.Space)) {
                jumpState.ReleaseJumpKey();
            } 
        }
        
        void Update() {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            running = Input.GetKey(KeyCode.LeftShift);
            
            Vector3 movement = (transform.forward * moveY + transform.right * moveX).normalized;

            firstPersonCharacterMovement.UpdateMovement(movement, Time.deltaTime);

            if (moveX == 0 && moveY == 0) {
                stateMachine.Handle(StateMachineEvent.STOP);
            } else {
                stateMachine.Handle(StateMachineEvent.WALK);
            }

            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                stateMachine.Handle(StateMachineEvent.CROUCH);
            }

            if (Input.GetKeyUp(KeyCode.LeftControl)) {
                stateMachine.Handle(StateMachineEvent.UN_CROUCH);
            }
            
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                stateMachine.Handle(StateMachineEvent.RUN);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                stateMachine.Handle(StateMachineEvent.STOP_RUNNING);
            }
            
            EvaluateWallrun(moveX, moveY);
            
            HandleJumpState();
            HandleArialMovementInput();
            
            Vector3 headTilt = new(NormalizeToAbsolute(moveY), 0, -NormalizeToAbsolute(moveX));
            firstPersonCamera.TryUpdateHeadTilt(headTilt, running);
        }

        Vector3 WallrunRayOrigin() {
            return transform.position + wallrunRayOffset;
        }
        
        void EvaluateWallrun(float xInput, float yInput) {
            bool left = Physics.Raycast(WallrunRayOrigin(), -transform.right, out leftWall, wallrunRayLength, wallMask.value);
            bool right = Physics.Raycast(WallrunRayOrigin(), transform.right, out rightWall, wallrunRayLength, wallMask.value);
            bool front = Physics.Raycast(WallrunRayOrigin(), transform.forward, out frontWall, wallrunRayLength, wallMask.value);
            bool back = Physics.Raycast(WallrunRayOrigin(), -transform.forward, out backWall, wallrunRayLength, wallMask.value);

            wallrunLeft = left && xInput < 0;
            wallrunRight = right && xInput > 0;
            wallrunFront = front && yInput > 0;
            wallrunBack = back && yInput < 0;
            
            wallrunning = wallrunLeft || wallrunRight || wallrunBack || wallrunFront; 
            
            stateMachine.Handle(wallrunning ? StateMachineEvent.WALLRUN : StateMachineEvent.STOP_WALLRUNNING);
        }
        
        int NormalizeToAbsolute(float value) {
            if (value > 0) return 1;
            if (value < 0) return -1;
            
            return 0;
        }

        public void NotifyGroundedState(bool isGrounded) {
            if (isGrounded) {
                OnLand();
            }
        }

        string lastMovementStateDebugString = "Movement State: null";
        string lastJumpStateDebugString = "Jump State: null";
        
        void UpdateDebugText() {
            string debugInfo = $"{lastMovementStateDebugString}\n{lastJumpStateDebugString}";
            DebugView.i.SetDebugText("movement", debugInfo);
        }
        
        public void OnStateMachineStateChange(StateMachineState from, StateMachineState to) {
            lastMovementStateDebugString = $"Movement State: {from} → {to}";

            if (jumpState != null) {
                lastJumpStateDebugString = $"Jump State: {jumpState.CurrentState_Debug()}\n{jumpState.CurrentJumps_Debug()}";
            }
            
            UpdateDebugText();
        }

        public void NotifyOfJumpState(JumpStateEvent jumpStateEvent) {
            lastJumpStateDebugString = $"Jump State: {jumpState.CurrentState_Debug()} (Event: {jumpStateEvent})\n{jumpState.CurrentJumps_Debug()}";
            UpdateDebugText();

            if (jumpStateEvent == JumpStateEvent.BEGIN_FALL) {
                stateMachine.Handle(StateMachineEvent.FALL);
            }
        }

        void OnDrawGizmos() {
            Gizmos.DrawRay(WallrunRayOrigin(), Vector3.right * wallrunRayLength);
            Gizmos.DrawRay(WallrunRayOrigin(), Vector3.left * wallrunRayLength);
            Gizmos.DrawRay(WallrunRayOrigin(), Vector3.forward * wallrunRayLength);
            Gizmos.DrawRay(WallrunRayOrigin(), Vector3.back * wallrunRayLength);

            if (wallrunning) {
                Vector3 wallNormal = Vector3.zero;
                
                if (wallrunLeft) { wallNormal = leftWall.normal; }
                else if (wallrunRight) { wallNormal = rightWall.normal; }
                else if (wallrunBack) { wallNormal = backWall.normal; }

                Gizmos.DrawRay(transform.position, wallNormal * 2F);
            }
        }
    }
}