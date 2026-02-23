using JetBrains.Annotations;
using SocratesDialogue;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoice : MonoBehaviour, IPointerClickHandler {
    DialogueManager dialogueManager;
    string nextSectionRef;
    
    TextMeshProUGUI contentText;

    const float fadeInTime = 0.3F;

    CanvasGroup canvasGroup;

    /// <summary>
    /// Initializes the dialogue choice and fades it in after a given amount of time.
    /// </summary>
    /// <param name="dialogueManager"></param>
    /// <param name="contents"></param>
    /// <param name="nextSectionRef"></param>
    /// <param name="fadeInAfter"></param>
    /// <param name="index"></param>
    public void Initialize(
        DialogueManager dialogueManager, 
        string contents, 
        string nextSectionRef, 
        float fadeInAfter = 0, 
        int index = 0) {
        canvasGroup = GetComponent<CanvasGroup>();
        
        this.dialogueManager = dialogueManager;
        this.nextSectionRef = nextSectionRef;
        
        contentText = GetComponentInChildren<TextMeshProUGUI>();
        contentText.text = contents;

        if (fadeInAfter > 0) {
            canvasGroup.alpha = 0;
            
            LeanTween.cancel(gameObject);
            LeanTween.delayedCall(fadeInAfter + (fadeInTime * index),
                () => {
                    LeanTween.value(gameObject, 0, 1, fadeInTime).
                        setOnUpdate((alpha) => {
                            canvasGroup.alpha = alpha;
                        });
                });
        }
        else {
            canvasGroup.alpha = 1;
        }
    }

    /// <summary>
    /// Fades the choice object out before destroying it.
    /// </summary>
    public void Destroy() {
        LeanTween.cancel(gameObject);
        
        LeanTween.value(gameObject, 1, 0, fadeInTime).
            setOnUpdate((alpha) => {
                canvasGroup.alpha = alpha;
            }).setOnComplete(() => {
                LeanTween.cancel(gameObject); 
                Destroy(gameObject);
            });
    }
    
    /// <summary>
    /// Continues the conversation when the UI element is clicked.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData) {
        dialogueManager.ContinueConversation(nextSectionRef);
    }
}