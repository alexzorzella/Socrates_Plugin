using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueOptionDisplay : MonoBehaviour
{
    public TextMeshProUGUI contentText;
    public DS.DialogueSection leadsTo;

    private DialogueManager manager;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        manager = FindObjectOfType<DialogueManager>();
    }

    public void SetParams(string optionText, DS.DialogueSection nextDialogueSection)
    {
        contentText.text = optionText;
        leadsTo = nextDialogueSection;
    }

    public void ProceedOnClick()
    {
        manager.currentSection = leadsTo;
        manager.DisplayDialogue();
    }

    public float AnimationLength()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }

        return anim.GetCurrentAnimatorStateInfo(0).length;
    }    
}