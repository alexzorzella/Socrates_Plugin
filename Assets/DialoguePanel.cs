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
        SocVertModifier.ParseAndSetText("", nameText, true);
        SocVertModifier.ParseAndSetText("", contentText, true);
        
        SetDialoguePanelVisible(true);
        Debug.Log("Begun");
    }

    public void OnSectionChanged(NewDialogueSection newSection) {
        string name = newSection.GetSpeaker();
        string content = newSection.GetContent();
        
        
        SocVertModifier.ParseAndSetText(name, nameText, true);
        SocVertModifier.ParseAndSetText(content, contentText, muted: false);
    }

    public bool OnStandby() {
        return contentText.TextHasBeenDisplayed();
    }
    
    public void OnDialogueEnded() {
        SetDialoguePanelVisible(false);
    }
    
    // Is passing the current section here really the best idea?
    public void DisplayTextFully(NewDialogueSection currentSection) {
        SocVertModifier.ParseAndSetText(currentSection.GetContent(), contentText, true);
    }
}