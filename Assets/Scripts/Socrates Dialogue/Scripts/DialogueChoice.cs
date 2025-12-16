using SocratesDialogue;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// TODO: Reimplement behavior
public class DialogueChoice : MonoBehaviour, IPointerClickHandler {
    DialogueManager dialogueManager;
    string reference;
    
    TextMeshProUGUI contentText;

    public void Initialize(DialogueManager dialogueManager, string contents, string reference) {
        this.dialogueManager = dialogueManager;
        this.reference = reference;
        
        contentText = GetComponentInChildren<TextMeshProUGUI>();
        contentText.text = contents;
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        dialogueManager.ContinueConversation(reference);
    }
}