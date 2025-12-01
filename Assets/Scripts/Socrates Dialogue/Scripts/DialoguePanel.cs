using NewSocratesDialogue;
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

    void SetDialoguePanelVisible(bool visible) {
        LeanTween.cancel(canvasGroup.gameObject);

        canvasGroup.alpha = visible ? 0 : 1;
        LeanTween.value(canvasGroup.alpha, visible ? 1 : 0, toggleTime)
            .setOnUpdate((value) => { canvasGroup.alpha = value; });
    }

    public void OnDialogueBegun() {
        nameText.ClearText();
        contentText.ClearText();
        
        SetDialoguePanelVisible(true);
    }

    public void OnSectionChanged(NewDialogueSection newSection) {
        string name = newSection.GetSpeaker();
        string content = newSection.GetContent();
        
        
        nameText.SetText(name, true);
        contentText.SetText(content, muted: false);
    }

    public bool OnStandby() {
        return contentText.TextHasBeenDisplayed();
    }
    
    public void OnDialogueEnded() {
        SetDialoguePanelVisible(false);
    }
    
    // Is passing the current section here really the best idea?
    public void DisplayTextFully(NewDialogueSection currentSection) {
        contentText.SetText(currentSection.GetContent(), true);
    }
}