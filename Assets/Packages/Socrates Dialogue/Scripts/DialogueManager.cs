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

    public Transform parentTextTo;
    public GameObject textObject;

    public CanvasGroup canvasGroup;

    PCamera cam;

    private void Start()
    {
        GetComponents();
    }

    private void GetComponents()
    {
        clickToContinue.SetActive(false);
        cam = FindObjectOfType<PCamera>();
    }

    private void Update()
    {
        PrepForDisplayOptions();
        DisplayOptions();

        if(promptToDisplay)
        {
            CheckIfDialogueAnimationComplete();
        }
    }

    private void PrepForDisplayOptions()
    {
        if (optionsBeenDisplayed)
            //|| anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0F
        {
            return;
        }

        //Checks if the current section has choices and checks if the text has been completely displayed
        if (typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection) 
            && contentText.TextHasBeenDisplayed())
        {
            ResetChoicesDisplayVariables();
            optionsBeenDisplayed = true;
        }
    }

    public void StartDialogue(Dialogue_Superclass.DialogueSection start)
    {
        canvasGroup.interactable = true;
        anim.SetBool("open", true);
        // AudioManager.i.Play("dialogue_box_open");
        ClearAllOptions();
        currentSection = start;

        //Inventory.i.Hide();
        TooltipScreenspaceUI.Hide();

        if (typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection))
        {
            clickToContinue.SetActive(false);
        }

        ClearContentText();

        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetSpeakerName(), nameText, true, true, currentSection);
        SocraticVertexModifier.PrepareParsesAndSetText("", contentText, true, true, currentSection);

        promptToDisplay = true;
    }

    bool promptToDisplay;

    private void CheckIfDialogueAnimationComplete()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0F &&
            anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("open"))
        {
            promptToDisplay = false;

            DisplayDialogue();
        }
    }

    public bool Talking()
    {
        return anim.GetBool("open");
    }

    public void ProceedToNext()
    {
        bool isMonologue = typeof(Dialogue_Superclass.Monologue).IsInstanceOfType(currentSection);

        if (isMonologue && !contentText.TextHasBeenDisplayed())
        {
            SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetTitle(), contentText, true, currentSection: currentSection);
            // AudioManager.i.Play("dialogue_select", 1.2F, 1.2F);
            return;
        }

        if (displayingChoices)
        {
            return;
        }

        if (currentSection.GetAction() != null && !contentText.TextHasBeenDisplayed())
        {
            return;
        }

        // AudioManager.i.Play("dialogue_select");

        if (currentSection.GetNextSection() != null)
        {
            currentSection.SetActionExecution(false);
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

        bool isMonologue = typeof(Dialogue_Superclass.Monologue).IsInstanceOfType(currentSection);

        clickToContinue.SetActive(isMonologue);

        ClearAllOptions();

        //SocraticVertexModifier.PrepareParsesAndSetText("", contentText.GetComponent<TextMeshProUGUI>(), contentText, true, true);

        DisplayText();
    }

    public void DisplayText()
    {
        optionsBeenDisplayed = false;
        ClearContentText();

        //SocraticVertexModifier.PrepareParsesAndSetText("", contentText.GetComponent<TextMeshProUGUI>(), contentText, true, true);
        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetSpeakerName(), nameText, true, true, currentSection);
        SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetTitle(), contentText, false, false, currentSection);
        //SocraticVertexModifier.PrepareParsesAndSetText(currentSection.GetTitle(), contentText.GetComponent<TextMeshProUGUI>(), contentText, true, true, currentSection);
    }

    private void ClearContentText()
    {
        if (contentText != null)
        {
            Destroy(contentText.gameObject);
        }

        GameObject t = Instantiate(textObject, Vector2.zero, Quaternion.identity);
        t.transform.SetParent(parentTextTo);
        t.GetComponent<RectTransform>().localPosition = Vector2.zero;
        t.GetComponent<RectTransform>().sizeDelta = parentTextTo.GetComponent<RectTransform>().sizeDelta;
        t.GetComponent<RectTransform>().localScale = Vector3.one;

        contentText = t.GetComponent<SocraticVertexModifier>();
    }

    public void EndDialogue()
    {
		if(cam != null)
		{
			cam.SetTargetWithTag("Player");
		}
        
        clickToContinue.SetActive(false);
        canvasGroup.interactable = false;
        ClearAllOptions();
        
        anim.SetBool("open", false);
        // AudioManager.i.Play("dialogue_box_close");

        //Inventory.i.DisplayHotbar();
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

    public void ResetChoicesDisplayVariables()
    {
        displayingChoices = true;
        indexOfCurrentChoice = 0;
    }

    public void DisplayOptions()
    {
        //Returns if the current section doesn't prompt choices
        if (!typeof(Dialogue_Superclass.Choices).IsInstanceOfType(currentSection))
            return;

        //Casts the dialogue into choices
        Dialogue_Superclass.Choices choices = (Dialogue_Superclass.Choices)currentSection;

        //Checks if it's supposed to be displaying choices
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

                    // AudioManager.i.Play("dialogue_display");

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