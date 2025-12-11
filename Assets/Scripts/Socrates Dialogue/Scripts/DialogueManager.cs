using System.Collections.Generic;
using UnityEngine;

namespace SocratesDialogue {
    public class DialogueManager : MonoBehaviour {
        static DialogueManager _i;
        
        void Start() {
            DontDestroyOnLoad(gameObject);
            dialoguePanel = GetComponentInChildren<DialoguePanel>();
            
            if(dialoguePanel == null) {
                Debug.LogWarning($"There was no dialogue panel found in {gameObject.name}'s children.");
            }
        }

        /// <summary>
        /// Returns the static instance of dialogue. If there is none, it loads one, sets
        /// the instance to that, and returns the instance.
        /// </summary>
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
        DialogueSection currentSection;
        readonly List<DialogueListener> listeners = new();

        /// <summary>
        /// Registers the passed listener to listen to this dialogue manager's events.
        /// </summary>
        /// <param name="newListener"></param>
        public void RegisterListener(DialogueListener newListener) {
            listeners.Add(newListener);
        }

        /// <summary>
        /// Starts a new dialogue interaction beginning with the passed starting dialogue section.
        /// </summary>
        /// <param name="start"></param>
        public void StartDialogue(DialogueSection start) {
            if (start == null) {
                Debug.LogWarning("No dialogue section passed.");
                return;
            }

            NotifyOfDialogueBegun();

            LeanTween.delayedCall(DialoguePanel.toggleTime, () => { SetCurrentSection(start); });
        }

        /// <summary>
        /// Notifies all listeners that the dialogue has begun.
        /// </summary>
        void NotifyOfDialogueBegun() {
            foreach (var listener in listeners) {
                listener.OnDialogueBegun();
            }
        }
        
        /// <summary>
        /// Notifies all listeners that the current dialogue seciton has changed.
        /// </summary>
        void NotifyOfSectionChange() {
            foreach (var listener in listeners) {
                listener.OnSectionChanged(currentSection);
            }
        }
        
        /// <summary>
        /// Notifies all listeners that the dialogue has ended.
        /// </summary>
        void NotifyOfDialogueEnded() {
            foreach (var listener in listeners) {
                listener.OnDialogueEnded();
            }
        }

        /// <summary>
        /// Sets the current dialogue section to the new dialogue section, optionally notifyfing all
        /// listeners that the dialogue section has changed.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="doNotNotify"></param>
        void SetCurrentSection(DialogueSection section, bool doNotNotify = false) {
            currentSection = section;

            if (!doNotNotify) {
                NotifyOfSectionChange();
            }
        }

        /// <summary>
        /// Ends the dialogue if the current section has no next section. Otherwise, it checks if the dialogue has
        /// finished displaying. If it has, it continues to the next section. Otherwise, it fully displays the current
        /// dialogue section's content.
        /// </summary>
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

        /// <summary>
        /// Ends the current conversation and notifies all listeners.
        /// </summary>
        void EndDialogue() {
            SetCurrentSection(null, true);
            NotifyOfDialogueEnded();
        }

        /// <summary>
        /// Returns true when in conversation.
        /// </summary>
        /// <returns></returns>
        public bool Talking() {
            return currentSection != null;
        }
    }
}