using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.QueryViews.Changesets;
using SocratesDialogue;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour, DialogueListener {
    RectTransform rectTransform;
    CanvasGroup canvasGroup;

    public SocVertModifier nameText;
    public SocVertModifier contentText;
    
    public const float fadeTime = 0.15F;
    const float moveTime = 0.25F;
    static readonly Vector2 origin = new Vector2(0, 79);

    public VerticalLayoutGroup choiceParent;
    RectTransform choiceParentRect;
    
    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        choiceParentRect = choiceParent.GetComponent<RectTransform>();
        
        GetComponentInParent<DialogueManager>().RegisterListener(this);
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
    /// <param name="section"></param>
    public void OnSectionChanged(DialogueSection section) {
        string name = section.GetFacet<DialogueSpeaker>().ToString();
        string content = section.GetFacet<DialogueContent>().ToString();
        
        nameText.SetText(name);
        contentText.SetText(content, scroll: true, muted: false);

        string soundName =
            section.GetFacet<DialogueSound>() != null ? 
                section.GetFacet<DialogueSound>().ToString() : 
                SocraticAnnotation.defaultSoundName;
        
        contentText.SetDialogueSfx(soundName);

        if (section.GetFacet<DialogueSoundbite>() != null) {
            string soundbiteName = section.GetFacet<DialogueSoundbite>().ToString();
            contentText.PlaySoundbite(soundbiteName);
        }

        int choiceCount = section.CountOfFacetType<NextSection>();
        
        if (choiceCount > 1) {
            Move(origin + new Vector2(0, choiceParentRect.sizeDelta.y));
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