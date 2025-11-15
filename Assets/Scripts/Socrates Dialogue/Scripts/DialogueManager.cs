using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour {
    public SocratesDialogue.DialogueSection currentSection;

    public SocVertModifier nameText;
    public SocVertModifier contentText;

    readonly Vector3 origin = new Vector3(0, -220F, 0);
    
    const float spacing = -45F;

    public Transform parentChoicesTo;

    public Transform parentTextTo;

    CanvasGroup canvasGroup;

    PCamera cam;

    void Start() {
        GetComponents();
    }

    void GetComponents() {
        cam = FindFirstObjectByType<PCamera>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    void Update() {
        PrepForDisplayOptions();
        DisplayOptions();
    }

    void PrepForDisplayOptions() {
        if (optionsBeenDisplayed) {
            return;
        }

        //Checks if the current section has choices and checks if the text has been completely displayed
        if (typeof(SocratesDialogue.Choices).IsInstanceOfType(currentSection)
            && contentText.TextHasBeenDisplayed()) {
            ResetChoicesDisplayVariables();
            optionsBeenDisplayed = true;
        }
    }

    const float toggleTime = 0.15F;
    bool talking = false;
    
    void SetDialoguePanelVisible(bool visible) {
        LeanTween.cancel(canvasGroup.gameObject);

        canvasGroup.alpha = visible ? 0 : 1;
        LeanTween.value(canvasGroup.alpha, visible ? 1 : 0, toggleTime)
            .setOnUpdate((value) => { canvasGroup.alpha = value; });
    }

    public void StartDialogue(SocratesDialogue.DialogueSection start) {
        if (start == null) {
            Debug.LogWarning("No dialogue section passed.");
            return;
        }
        
        // canvasGroup.interactable = true;
        // anim.SetBool("open", true);

        talking = true;
        
        SetDialoguePanelVisible(true);
        LeanTween.delayedCall(toggleTime, DisplayDialogue);

        // AudioManager.i.Play("dialogue_box_open");
        ClearAllOptions();
        currentSection = start;

        //Inventory.i.Hide();
        TooltipScreenspaceUI.Hide();

        if (typeof(SocratesDialogue.Choices).IsInstanceOfType(currentSection)) {
            // clickToContinue.SetActive(false);
        }

        ClearContentText();

        SocVertModifier.ParseAndSetText(currentSection.GetSpeakerName(), nameText, true, true,
            currentSection);
        SocVertModifier.ParseAndSetText("", contentText, true, true, currentSection);

        promptToDisplay = true;
    }

    bool promptToDisplay;

    public bool Talking() {
        return talking;
        // return anim.GetBool("open");
    }

    public void TryContinueConversaion() {
        if (!talking) {
            return;
        }
        
        ContinueConversation();
    }
    
    public void ContinueConversation() {
        if (!talking) {
            return;
        }
        
        bool isMonologue = currentSection is SocratesDialogue.Monologue;

        if (isMonologue && !contentText.TextHasBeenDisplayed()) {
            SocVertModifier.ParseAndSetText(currentSection.GetTitle(), contentText, true,
                currentSection: currentSection);
            // AudioManager.i.Play("dialogue_select", 1.2F, 1.2F);
            return;
        }

        if (displayingChoices) {
            return;
        }

        if (currentSection.GetAction() != null && !contentText.TextHasBeenDisplayed()) {
            return;
        }

        // AudioManager.i.Play("dialogue_select");

        if (currentSection.GetNextSection() != null) {
            currentSection.SetActionExecution(false);
            currentSection = currentSection.GetNextSection();
            DisplayDialogue();
        } else {
            EndDialogue();
        }
    }

    public void DisplayDialogue() {
        if (currentSection == null) {
            EndDialogue();
            return;
        }

        bool isMonologue = typeof(SocratesDialogue.Monologue).IsInstanceOfType(currentSection);

        nameText.SetDialogueSfx(currentSection.GetDialogueSound());
        
        // clickToContinue.SetActive(isMonologue);

        ClearAllOptions();

        //SocraticVertexModifier.PrepareParsesAndSetText("", contentText.GetComponent<TextMeshProUGUI>(), contentText, true, true);

        DisplayText();
    }

    public void DisplayText() {
        optionsBeenDisplayed = false;
        ClearContentText();

        SocVertModifier.ParseAndSetText(currentSection.GetSpeakerName(), nameText, true, true,
            currentSection);
        SocVertModifier.ParseAndSetText(currentSection.GetTitle(), contentText, false, false,
            currentSection);
    }

    void ClearContentText() {
        if (contentText != null) {
            Destroy(contentText.gameObject);
        }

        GameObject t = ResourceLoader.InstantiateObject("DialogueText", Vector2.zero, Quaternion.identity);
        t.transform.SetParent(parentTextTo);
        t.GetComponent<RectTransform>().localPosition = Vector2.zero;
        t.GetComponent<RectTransform>().sizeDelta = parentTextTo.GetComponent<RectTransform>().sizeDelta;
        t.GetComponent<RectTransform>().localScale = Vector3.one;

        contentText = t.GetComponent<SocVertModifier>();
    }

    void EndDialogue() {
        LeanTween.delayedCall(0.02F, () => {
            talking = false;
        });

        currentSection = null;
        canvasGroup.interactable = false;
        ClearAllOptions();

        SetDialoguePanelVisible(false);
    }

    public void ClearAllOptions() {
        GameObject[] currentDialogueOptions = GameObject.FindGameObjectsWithTag("DialogueChoice");

        foreach (var entry in currentDialogueOptions) {
            entry.GetComponent<Animator>().SetBool("exit", true);
            Destroy(entry, 0.2F);
        }
    }

    float currentOptionDelay = 0;
    int indexOfCurrentChoice = 0;
    [HideInInspector] public bool displayingChoices;
    bool optionsBeenDisplayed;

    public void ResetChoicesDisplayVariables() {
        displayingChoices = true;
        indexOfCurrentChoice = 0;
    }

    public void DisplayOptions() {
        //Returns if the current section doesn't prompt choices
        if (!typeof(SocratesDialogue.Choices).IsInstanceOfType(currentSection))
            return;

        //Casts the dialogue into choices
        SocratesDialogue.Choices choices = (SocratesDialogue.Choices)currentSection;

        //Checks if it's supposed to be displaying choices
        if (displayingChoices) {
            if (indexOfCurrentChoice < choices.choices.Count) {
                if (currentOptionDelay <= 0) {
                    Tuple<string, SocratesDialogue.DialogueSection> option = choices.choices[indexOfCurrentChoice];

                    GameObject s = ResourceLoader.InstantiateObject("DialogueChoice", Vector3.zero, Quaternion.identity);
                    s.transform.SetParent(parentChoicesTo, false);

                    DialogueOptionDisplay optionDisplayBehavior = s.GetComponent<DialogueOptionDisplay>();
                    currentOptionDelay = optionDisplayBehavior.AnimationLength();

                    optionDisplayBehavior.SetParams(option.Item1, option.Item2);

                    // AudioManager.i.Play("dialogue_display");

                    indexOfCurrentChoice++;
                } else {
                    currentOptionDelay -= Time.deltaTime;
                }
            } else {
                displayingChoices = false;
            }
        }
    }
}