using UnityEngine;

namespace FirstPersonMovement {
    public class FirstPersonHandVisuals : MonoBehaviour {
        const float minRollInterval = 10F;
        const float maxRollInterval = 20F;
        float currentRollInterval = 5F;

        Animator anim;

        MultiAudioSource knuckles;

        void Start() {
            anim = GetComponent<Animator>();
            knuckles = MultiAudioSource.FromResources(gameObject, "knuckles", 2);
        }

        void Update() {
            if (currentRollInterval <= 0) {
                currentRollInterval = UnityEngine.Random.Range(minRollInterval, maxRollInterval);
                anim.Play("rolling");
                knuckles.PlayRandomPitch();
            } else {
                currentRollInterval -= Time.deltaTime;
            }
        }
    }
}