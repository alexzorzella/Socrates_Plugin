using UnityEngine;

public class DeltaCameraFuncTest : MonoBehaviour {
    void Update() {
        if (Input.GetMouseButtonDown(0) && !FindObjectOfType<DialogueManager>().Talking())
            FindObjectOfType<DeltaCameraShake>().Shake(Shakepedia.GetProfileClone(Shakepedia.MINOR));
    }
}