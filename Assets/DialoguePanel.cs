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
        SetDialoguePanelVisible(true);
        Debug.Log("Begun");
    }

    public void OnSectionChanged(NewDialogueSection newSection) {
        string name = newSection.GetSpeaker();
        string content = newSection.GetContent();
        
        
        SocVertModifier.ParseAndSetText(name, nameText);
        SocVertModifier.ParseAndSetText(content, contentText, muted: false);
    }
    
    // public void DisplayText() {
    //     optionsBeenDisplayed = false;
    //     ClearContentText();
    //
    //     SocVertModifier.ParseAndSetText(currentSection.GetSpeakerName(), nameText, true, true,
    //         currentSection);
    //     SocVertModifier.ParseAndSetText(currentSection.GetTitle(), contentText, false, false,
    //         currentSection);
    // }
    //
    // void ClearContentText() {
    //     if (contentText != null) {
    //         Destroy(contentText.gameObject);
    //     }
    //
    //     GameObject t = ResourceLoader.InstantiateObject("DialogueText", Vector2.zero, Quaternion.identity);
    //     t.transform.SetParent(parentTextTo);
    //     t.GetComponent<RectTransform>().localPosition = Vector2.zero;
    //     t.GetComponent<RectTransform>().sizeDelta = parentTextTo.GetComponent<RectTransform>().sizeDelta;
    //     t.GetComponent<RectTransform>().localScale = Vector3.one;
    //
    //     contentText = t.GetComponent<SocVertModifier>();
    // }

    public void OnDialogueEnded() {
        SetDialoguePanelVisible(false);
    }
}