using UnityEngine;

namespace FirstPersonMovement {
    public class FirstPersonCamera : MonoBehaviour {
        const float headTiltIntensityX = 1.5F;
        const float headTiltIntensityZ = 2F;
        const float runningHeadTiltMultiplier = 2F;

        readonly Vector3 headTiltIntensity = new(headTiltIntensityX, 0, headTiltIntensityZ);

        Transform headTransform;
        Transform bodyTransform;

        int tweenId = -1;
        
        Vector3 headTilt;
        float cameraPitch;

        [SerializeField] [Range(0, 20F)] float sensitivity = 5f;
        [SerializeField] [Range(0, 1F)] float headRotationSpeed = 0.15F;

        void Awake() {
            Initialize();
        }

        void Initialize() {
            Cursor.lockState = CursorLockMode.Locked;
            headTransform = transform.parent;
            bodyTransform = transform.root;
        }

        void Update() {
            RotateHandheld();
        }

        void RotateHandheld() {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -90f, 90f);

            bodyTransform.Rotate(Vector3.up, mouseX);
            headTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        /// <summary>
        /// This may be called every frame, potentially passing the same gimbal tilt that's already
        /// cached here. It will only update if the passed tilt is different
        /// </summary>
        /// <param name="newHeadTilt"></param>
        /// <param name="running"></param>
        public void TryUpdateHeadTilt(Vector3 newHeadTilt, bool running = false) {
            newHeadTilt = Vector3.Scale(newHeadTilt, headTiltIntensity) * (running ? runningHeadTiltMultiplier : 1f);

            if (headTilt != newHeadTilt) {
                UpdateHeadTilt(newHeadTilt);
            }
        }

        /// <summary>
        /// Sets the head tilt and tweens the gimbal object's rotation to it
        /// </summary>
        /// <param name="newGimbalTilt"></param>
        public void UpdateHeadTilt(Vector3 newGimbalTilt) {
            if (tweenId >= 0) {
                LeanTween.cancel(tweenId);
            }

            headTilt = newGimbalTilt;

            tweenId = LeanTween.rotateLocal(gameObject, headTilt, headRotationSpeed).id;
        }

        /// <summary>
        /// Resets the head to the neutral position
        /// </summary>
        public void ResetHeadTilt() {
            UpdateHeadTilt(Vector3.zero);
        }
    }
}