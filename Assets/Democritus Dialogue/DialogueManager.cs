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
        PrepForDisplayOptions();
        DisplayOptions();
    }

    private void PrepForDisplayOptions()
    {
        if(optionsBeenDisplayed)
        {
            return;
        }

        if (typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection) && contentText.TextHasBeenDisplayed())
        {
            ResetDisplayOptionsFlags();
            optionsBeenDisplayed = true;
        }
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
        if(displayingChoices)
        {
            return;
        }

        if (currentSection.GetAction() != null && !contentText.TextHasBeenDisplayed())
        {
            return;
        }

        AudioManager.i.Play("dialogue_select");

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
        optionsBeenDisplayed = false;

        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetSpeakerName(), nameText.GetComponent<TextMeshProUGUI>(), nameText, true, true, currentSection);
        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetTitle(), contentText.GetComponent<TextMeshProUGUI>(), contentText, false, false, currentSection);
    }

    public void EndDialogue()
    {
        ClearAllOptions();
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

    float currentOptionDelay = 0;
    int indexOfCurrentChoice = 0;
    [HideInInspector] public bool displayingChoices;
    private bool optionsBeenDisplayed;

    public void ResetDisplayOptionsFlags()
    {
        displayingChoices = true;
        indexOfCurrentChoice = 0;
    }

    public void DisplayOptions()
    {
        if (!typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection))
        {
            return;
        }

        Dialogue_Superclass.Choices choices = (Dialogue_Superclass.Choices)currentSection;

        if (displayingChoices)
        {
            if (indexOfCurrentChoice < choices.choices.Count)
            {
                if (currentOptionDelay <= 0)
                {
                    Tuple<string, Dialogue_Superclass.DialogueSection> option = choices.choices[indexOfCurrentChoice];
                    
                    GameObject s = Instantiate(dialogueChoice, Vector3.zero, Quaternion.identity);
                    s.transform.SetParent(parentChoicesTo, false);

                    DialogueOptionDisplay optionDisplayBehavior = s.GetComponent<DialogueOptionDisplay>();
                    currentOptionDelay = optionDisplayBehavior.AnimationLength();

                    optionDisplayBehavior.SetParams(option.Item1, option.Item2);

                    AudioManager.i.Play("dialogue_display");

                    indexOfCurrentChoice++;
                }
                else
                {
                    currentOptionDelay -= Time.deltaTime;
                }
            }
            else
            {
                displayingChoices = false;
            }
        }
    }
}