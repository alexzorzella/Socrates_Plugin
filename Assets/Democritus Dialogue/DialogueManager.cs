using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public Dialogue_Superclass.DialogueSection currentSection;

    public SocraticVertexModifier nameText;
    public SocraticVertexModifier contentText;

    [Header("Options")]
    public Vector3 origin = new Vector3(0, -220F, 0);
    public float spacing = -45F;
    public GameObject dialogueChoice;
    public GameObject clickToContinue;

    public Animator anim;

    public Transform parentChoicesTo;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Return) && OpenPanel())
        //{
        //    ProceedToNext();
        //}
    }

    public void StartDialogue(Dialogue_Superclass.DialogueSection start)
    {
        anim.SetBool("open", true);
        ClearAllOptions();
        currentSection = start;
        DisplayDialogue();
    }

    public bool OpenPanel()
    {
        return anim.GetBool("open");
    }

    public void ProceedToNext()
    {
        if (currentSection.GetAction() != null && !contentText.TextHasBeenDisplayed())
        {
            return;
        }

        if (currentSection.GetNextSection() != null)
        {
            currentSection = currentSection.GetNextSection();
            DisplayDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    public void DisplayDialogue()
    {
        if (currentSection == null)
        {
            EndDialogue();
            return;
        }

        bool monologue = typeof(Dialogue_Superclass.Monologue).IsInstanceOfType(currentSection);

        clickToContinue.SetActive(monologue);

        ClearAllOptions();

        SocraticVertexModifier.PrepareParsesAndSetText("", contentText.GetComponent<TextMeshProUGUI>(), contentText, true, true);

        DisplayText();
    }

    public void DisplayText()
    {
        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetSpeakerName(), nameText.GetComponent<TextMeshProUGUI>(), nameText, true, true, currentSection);
        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetTitle(), contentText.GetComponent<TextMeshProUGUI>(), contentText, false, false, currentSection);
        
        if (typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection))
        {
            StartCoroutine(DisplayOptions());
            //display all of the options as a way to pick them
        }
    }

    public void EndDialogue()
    {
        anim.SetBool("open", false);
    }

    public void ClearAllOptions()
    {
        GameObject[] currentDialogueOptions = GameObject.FindGameObjectsWithTag("DialogueChoice");

        foreach (var entry in currentDialogueOptions)
        {
            entry.GetComponent<Animator>().SetBool("exit", true);
            Destroy(entry, 0.2F);
        }
    }

    public IEnumerator DisplayOptions()
    {
        int i = 0;

        Dialogue_Superclass.Choices choices = (Dialogue_Superclass.Choices)currentSection;

        foreach (var option in choices.choices)
        {
            GameObject s = Instantiate(dialogueChoice, Vector3.zero, Quaternion.identity);
            s.transform.SetParent(parentChoicesTo, false);

            DialogueOptionDisplay optionDisplayBehavior = s.GetComponent<DialogueOptionDisplay>();
            optionDisplayBehavior.SetParams(option.Item1, option.Item2);

            yield return new WaitForSeconds(optionDisplayBehavior.AnimationLength());

            i++;
        }
    }
}