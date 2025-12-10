using NewSocratesDialogue;
using TMPro;
using UnityEngine;

// TODO: Reimplement behavior
public class DialogueOptionDisplay : MonoBehaviour {
    public TextMeshProUGUI contentText;

    DialogueManager manager;
    Animator anim;

    void Start() {
        anim = GetComponent<Animator>();
        manager = FindFirstObjectByType<DialogueManager>();
    }

    public void SetParams(string optionText) {
        contentText.text = optionText;
        // leadsTo = nextDialogueSection;
    }

    public void ProceedOnClick() {
        // if(manager.displayingChoices)
        // {
        //     return;
        // }
        //
        // // AudioManager.i.Play("dialogue_select");
        //
        // manager.currentSection = leadsTo;
        // manager.DisplayDialogue();
    }

    public float AnimationLength() {
        if (anim == null) {
            anim = GetComponent<Animator>();
        }

        return anim.GetCurrentAnimatorStateInfo(0).length;
    }
}