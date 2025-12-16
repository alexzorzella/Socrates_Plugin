using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.QueryViews.Changesets;
using SocratesDialogue;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour, DialogueListener, SocratesTextListener {
    DialogueManager dialogueManager;
    DialogueSection currentSection;
    
    RectTransform rectTransform;
    CanvasGroup canvasGroup;

    public SocratesText nameText;
    public SocratesText contentText;
    
    public const float fadeTime = 0.15F;
    const float moveTime = 0.25F;
    static readonly Vector2 origin = new Vector2(0, 79);
    static readonly Vector2 padding = new Vector2(0, 10);

    public VerticalLayoutGroup choiceLayoutGroup;
    RectTransform choiceParent;
    
    readonly List<GameObject> choiceObjects = new();

    void ClearChoiceObjects() {
        while (choiceObjects.Count > 0) {
            Destroy(choiceObjects[0]);
            choiceObjects.RemoveAt(0);
        }
    }
    
    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        choiceParent = choiceLayoutGroup.GetComponent<RectTransform>();
        
        dialogueManager = GetComponentInParent<DialogueManager>();
        dialogueManager.RegisterListener(this);
        
        contentText.RegisterListener(this);
    }

    /// <summary>
    /// Sets the dialogue panel's visibility.
    /// </summary>
    /// <param name="visible"></param>
    void SetDialoguePanelVisible(bool visible) {
        LeanTween.cancel(canvasGroup.gameObject);

        canvasGroup.alpha = visible ? 0 : 1;
        LeanTween.value(canvasGroup.alpha, visible ? 1 : 0, fadeTime)
            .setOnUpdate((value) => { canvasGroup.alpha = value; });
    }

    /// <summary>
    /// Clears the name and content text and sets the panel to visible whenever a conversation is started.
    /// </summary>
    public void OnDialogueBegun() {
        nameText.ClearText();
        contentText.ClearText();
        
        SetDialoguePanelVisible(true);
    }

    /// <summary>
    /// Sets the name text fully and queues the content text to scroll, populated by new dialogue section
    /// data whenever the dialogue section changes.
    /// </summary>
    /// <param name="newSection"></param>
    public void OnSectionChanged(DialogueSection newSection) {
        currentSection = newSection;
        
        string name = newSection.GetFacet<DialogueSpeaker>().ToString();
        string content = newSection.GetFacet<DialogueContent>().ToString();
        
        nameText.SetText(name);
        contentText.SetText(content, scroll: true, muted: false);

        string soundName =
            newSection.GetFacet<DialogueSound>() != null ? 
                newSection.GetFacet<DialogueSound>().ToString() : 
                SocraticAnnotation.defaultSoundName;
        
        contentText.SetDialogueSfx(soundName);

        if (newSection.GetFacet<DialogueSoundbite>() != null) {
            string soundbiteName = newSection.GetFacet<DialogueSoundbite>().ToString();
            contentText.PlaySoundbite(soundbiteName);
        }
    }

    /// <summary>
    /// Runs whenever the current dialogue section's content is fully displayed for the first time.
    /// </summary>
    public void OnFullyDisplayed() {
        ClearChoiceObjects();
        
        if (currentSection != null && currentSection.CountOfFacetType<NextSection>() > 1) {
            List<NextSection> choices = currentSection.GetFacets<NextSection>();
            GameObject dialogueChoiceObject = null;

            int index = 0;
            foreach (var choice in choices) {
                dialogueChoiceObject = ResourceLoader.InstantiateObject("DialogueChoice", choiceParent);
                DialogueChoice dialogueChoice = dialogueChoiceObject.GetComponent<DialogueChoice>();
                dialogueChoice.Initialize(dialogueManager, choice.Prompt(), choice.LeadsToRef(), moveTime, index);
                choiceObjects.Add(dialogueChoiceObject);

                index++;
            }

            if (dialogueChoiceObject != null) {
                float dialogueChoiceHeight = dialogueChoiceObject.GetComponent<RectTransform>().sizeDelta.y;
                choiceParent.sizeDelta = new Vector2(choiceParent.sizeDelta.x, choices.Count * (dialogueChoiceHeight + choiceLayoutGroup.spacing));
            }
        }
        
        int choiceCount = currentSection.CountOfFacetType<NextSection>();
        
        if (choiceCount > 1) {
            Move(origin + new Vector2(0, choiceParent.rect.height) + padding);
        }
        else {
            Move(origin);
        }
    }

    void Move(Vector2 to) {
        LeanTween.cancel(gameObject);
        
        LeanTween.value
                (gameObject, rectTransform.anchoredPosition, to, moveTime).
            setOnUpdate((Vector2 newPos) => { rectTransform.anchoredPosition = newPos; }).
            setEase(LeanTweenType.easeOutQuad);
    }
    
    /// <summary>
    /// Returns whether the content text has finished displaying.
    /// </summary>
    /// <returns></returns>
    public bool OnStandby() {
        return contentText.TextHasBeenDisplayed();
    }
    
    /// <summary>
    /// Hides the panel when the dialogue ends.
    /// </summary>
    public void OnDialogueEnded() {
        SetDialoguePanelVisible(false);
    }
    
    /// <summary>
    /// Forces the content of the passed section to fully display.
    /// </summary>
    /// <param name="currentSection"></param>
    /// (Is passing the current section here really the best idea?)
    public void DisplayTextFully(DialogueSection currentSection) {
        contentText.SetText(currentSection.GetFacet<DialogueContent>().ToString());
    }
}