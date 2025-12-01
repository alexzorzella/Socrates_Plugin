using System.Collections.Generic;
using UnityEngine;

namespace NewSocratesDialogue {
    public class DialogueManager : MonoBehaviour {
        static DialogueManager _i;
        
        void Start() {
            DontDestroyOnLoad(gameObject);
            dialoguePanel = GetComponentInChildren<DialoguePanel>();
        }

        public static DialogueManager i {
            get {
                if (_i == null) {
                    DialogueManager x = Resources.Load<DialogueManager>("DialogueCanvas");

                    _i = Instantiate(x);
                }

                return _i;
            }
        }

        DialoguePanel dialoguePanel;
        
        NewDialogueSection currentSection;
        
        readonly List<DialogueListener> listeners = new();

        public void RegisterListener(DialogueListener newListener) {
            listeners.Add(newListener);
        }

        public void StartDialogue(NewDialogueSection start) {
            if (start == null) {
                Debug.LogWarning("No dialogue section passed.");
                return;
            }

            NotifyOfDialogueBegun();

            LeanTween.delayedCall(DialoguePanel.toggleTime, () => { SetCurrentSection(start); });
        }

        void NotifyOfDialogueBegun() {
            foreach (var listener in listeners) {
                listener.OnDialogueBegun();
            }
        }

        void SetCurrentSection(NewDialogueSection newSection, bool doNotNotify = false) {
            currentSection = newSection;

            if (!doNotNotify) {
                foreach (var listener in listeners) {
                    listener.OnSectionChanged(currentSection);
                }
            }
        }

        void NotifyOfDialogueEnded() {
            foreach (var listener in listeners) {
                listener.OnDialogueEnded();
            }
        }

        public void ContinueConversation() {
            if (!Talking()) {
                return;
            }

            if (!currentSection.HasFacet<NextSection>()) {
                EndDialogue();
                return;
            }

            if (dialoguePanel.OnStandby()) {
                SetCurrentSection(currentSection.GetFacet<NextSection>().Next());
            }
            else {
                dialoguePanel.DisplayTextFully(currentSection);
            }
        }

        void EndDialogue() {
            SetCurrentSection(null, true);
            NotifyOfDialogueEnded();
        }

        public bool Talking() {
            return currentSection != null;
        }
    }
}