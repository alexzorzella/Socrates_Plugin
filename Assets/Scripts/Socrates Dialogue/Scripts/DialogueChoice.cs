using JetBrains.Annotations;
using SocratesDialogue;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueChoice : MonoBehaviour, IPointerClickHandler {
    DialogueManager dialogueManager;
    string reference;
    
    TextMeshProUGUI contentText;

    const float fadeInTime = 0.3F;

    CanvasGroup canvasGroup;

    public void Initialize(DialogueManager dialogueManager, string contents, string reference, float fadeInAfter = 0, int index = 0) {
        canvasGroup = GetComponent<CanvasGroup>();
        
        this.dialogueManager = dialogueManager;
        this.reference = reference;
        
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
    
    public void OnPointerClick(PointerEventData eventData) {
        dialogueManager.ContinueConversation(reference);
    }
}