using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewSocratesDialogue {
    public class DialogueManager : MonoBehaviour {
        static DialogueManager _i;

        void Start() {
            DontDestroyOnLoad(gameObject);
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
            
            LeanTween.delayedCall(DialoguePanel.toggleTime, () => { currentSection = start; });
        }

        void NotifyOfDialogueBegun() {
            foreach (var listener in listeners) {
                listener.OnDialogueBegun();
            }
        }

        void NotifyOfSectionChanged() {
            foreach (var listener in listeners) {
                listener.OnSectionChanged(currentSection);
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
            
            if (currentSection == null) {
                EndDialogue();
                return;
            }

            currentSection = currentSection.GetFacet<NextSection>().Next();
        }

        void EndDialogue() {
            currentSection = null;
            NotifyOfDialogueEnded();
        }
        
        public bool Talking() {
            return currentSection != null;
        }
    }
}