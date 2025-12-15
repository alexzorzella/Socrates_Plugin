using SocratesDialogue;
using UnityEngine;

public class DialoguePanel : MonoBehaviour, DialogueListener {
    CanvasGroup canvasGroup;

    public const float toggleTime = 0.15F;

    public SocVertModifier nameText;
    public SocVertModifier contentText;
    
    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        GetComponentInParent<DialogueManager>().RegisterListener(this);
    }

    /// <summary>
    /// Sets the dialogue panel's visibility.
    /// </summary>
    /// <param name="visible"></param>
    void SetDialoguePanelVisible(bool visible) {
        LeanTween.cancel(canvasGroup.gameObject);

        canvasGroup.alpha = visible ? 0 : 1;
        LeanTween.value(canvasGroup.alpha, visible ? 1 : 0, toggleTime)
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
    /// <param name="section"></param>
    public void OnSectionChanged(DialogueSection section) {
        string name = section.GetFacet<DialogueSpeaker>().ToString();
        string content = section.GetFacet<DialogueContent>().ToString();
        
        nameText.SetText(name);
        contentText.SetText(content, scroll: true, muted: false);
        
        contentText.SetDialogueSfx(section.GetFacet<DialogueSound>().ToString());
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