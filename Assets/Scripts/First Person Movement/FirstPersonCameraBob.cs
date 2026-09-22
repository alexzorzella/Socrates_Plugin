using UnityEngine;

namespace FirstPersonMovement {
    public class FirstPersonCameraBob : MonoBehaviour, StateMachineListener {
        Transform hand;
        readonly Vector2 handOffset = new(-1.75F, 0F);
        const float handZ = 2.4F;
        
        [SerializeField] [Range(0, 2)] float walkRange = 0.10F;
        [SerializeField] [Range(0, 2)] float walkSpeed = 0.15F;
        
        Transform headTransform;
        FirstPersonPhysicsHandler playerMovement;
        
        MultiAudioSource footsteps;
        MultiAudioSource breathing;

        [SerializeField] [Range(1, 30)] float breathingCooldown = 15F;
        [SerializeField] [Range(0, 5)] float breathingBuffer = 0.2F;
        
        float currentBreathingCooldown;
        float currentBreathingBuffer = 0.2F;
        
        int idleId = -1;
        int walkId = -1;
        
        float headY;
        
        bool stepped;
        bool hasBreathed;
        
        [SerializeField] float crouchHeightOffset = 1F;
        [SerializeField] float crouchMoveTime = 0.5F;
        
        void Awake() {
            Initialize();
        }

        void Initialize() {
            GetComponents();
            InitializeTweens();
            InitializeAudio();
           
            SnapHandToCameraView(); 
            headY = headTransform.localPosition.y;
        }

        void GetComponents() {
            hand = GetComponentInChildren<FirstPersonHandVisuals>().transform;
            headTransform = transform.parent;
            playerMovement = transform.root.GetComponent<FirstPersonPhysicsHandler>();
        }
        
        void InitializeTweens() {
            walkId = LeanTween.moveLocal(gameObject, new Vector3(0, walkRange, 0), walkSpeed).setLoopPingPong()
                .setOnUpdate((value) => {
                    if (value < 0.01F && !stepped) {
                        if (playerMovement.IsGrounded()) {
                            footsteps.PlayRandom();
                        }
                        
                        stepped = true;
                    } else if (value > 0.09 && stepped) {
                        stepped = false;
                    }
                }).setEase(LeanTweenType.easeOutQuad).id;
            
            LeanTween.delayedCall(0.01F, () => { LeanTween.pause(walkId); });

            idleId = LeanTween.moveLocal(gameObject, new Vector3(0, walkRange * 1.2F, 0), walkSpeed * 5F).
                setLoopPingPong().setEase(LeanTweenType.easeInOutQuad).id;
        }

        void InitializeAudio() {
            footsteps = MultiAudioSource.FromResources(gameObject, "footsteps", 4);
            breathing = MultiAudioSource.FromResource(gameObject, "breathing");
        }
        
        void SnapHandToCameraView() {
            Camera camera = Camera.main;

            float frustrumHeight = 2F * handZ * Mathf.Tan(camera.fieldOfView * 0.5F * Mathf.Deg2Rad);
            float frustrumWidth = frustrumHeight * camera.aspect;

            Vector3 localBottomRight = new Vector3(
                frustrumWidth / 2F + handOffset.x,
                -frustrumHeight / 2F + handOffset.y,
                handZ);

            hand.localPosition = localBottomRight;
        }

        void Update() {
            HandleBreathingEffects();
        }

        void HandleBreathingEffects() {
            if (currentBreathingCooldown > 0) {
                currentBreathingCooldown -= Time.deltaTime;
            }

            if (currentBreathingBuffer <= 0 && !hasBreathed) {
                breathing.PlayRoundRobin();
                
                currentBreathingCooldown = breathingCooldown;
                hasBreathed = true;
            }
            else {
                currentBreathingBuffer -= Time.deltaTime;
            }
        }

        void HandleIdle() {
            if (walkId >= 0) { LeanTween.pause(walkId); }
            if (idleId >= 0) { LeanTween.resume(idleId); }

            if (currentBreathingCooldown <= 0) {
                currentBreathingBuffer = breathingBuffer;
                hasBreathed = false;
            }
        }
        
        void HandleWalk() {
            if (walkId >= 0) { LeanTween.resume(walkId); }
            if (idleId >= 0) { LeanTween.pause(idleId); }

            hasBreathed = true;
        }

        void HandleAirborne() {
            if (walkId >= 0) { LeanTween.pause(walkId); }
            if (idleId >= 0) { LeanTween.pause(idleId); }
        }
        
        public void OnStateMachineStateChange(StateMachineState from, StateMachineState to) {
            string stateName = to.GetName();
            
            if (stateName == "idle") {
                HandleIdle();
            } else if (stateName == "walk" || stateName == "run"){
                HandleWalk(); 
            } else {
                HandleAirborne();
            }
            
            LeanTween.cancel(headTransform.gameObject);
            float gimbalPosition = headY - (to.GetName() == "crouching" ? crouchHeightOffset : 0);
            LeanTween.moveLocalY(headTransform.gameObject, gimbalPosition, crouchMoveTime) .setEase(LeanTweenType.easeOutExpo);
        }
    }
}