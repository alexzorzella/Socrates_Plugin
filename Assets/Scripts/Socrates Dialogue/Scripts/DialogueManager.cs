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
        
        public Transform choiceParent;
        readonly List<GameObject> choiceObjects = new();

        void ClearChoiceObjects() {
            while (choiceObjects.Count > 0) {
                Destroy(choiceObjects[0]);
                choiceObjects.RemoveAt(0);
            }
        }

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
            ClearChoiceObjects();
            
            currentSection = section;

            if (!doNotNotify) {
                NotifyOfSectionChange();
            }
            
            if (section != null && section.CountOfFacetType<NextSection>() > 1) {
                List<NextSection> choices = section.GetFacets<NextSection>();

                foreach (var choice in choices) {
                    GameObject dialogueChoiceObject = ResourceLoader.InstantiateObject("DialogueChoice", choiceParent);
                    DialogueChoice dialogueChoice = dialogueChoiceObject.GetComponent<DialogueChoice>();
                    dialogueChoice.Initialize(this, choice.Prompt(), choice.LeadsToRef());
                    choiceObjects.Add(dialogueChoiceObject);
                }
            }
        }

        /// <summary>
        /// Ends the dialogue if the current section has no next section. Otherwise, it checks if the dialogue has
        /// finished displaying. If it has, it continues to the next section. Otherwise, it fully displays the current
        /// dialogue section's content.
        /// </summary>
        public void ContinueConversation(string reference = "") {
            int nextSectionCount = 0;
            
            // Count the number of choices. Monologues have one next section (no choices).
            // Branching events have two or more sections (more than one choice).
            // For now, there's no way to only have one choice.
            if (currentSection != null) {
                nextSectionCount = currentSection.CountOfFacetType<NextSection>();
            }
            
            // Return if there's no conversation or if the current dialogue is a
            // branching dialogue and there was no choice passed.
            if (!Talking() || (nextSectionCount > 1 && string.IsNullOrWhiteSpace(reference))) {
                return;
            }

            DialogueSection nextSection = null;

            // Cached the reference's associated dialogue section if one was passed
            if (!string.IsNullOrWhiteSpace(reference)) {
                try {
                    nextSection = DialogueManifest.GetSectionByReference(reference);
                } catch {
                    Debug.LogWarning($"Reference {reference} has no associated dialogue section.");
                    nextSection = null;
                }
            } else if (nextSectionCount == 1) {
                // Otherwise, if the current section is a Monologue (only one
                // section next), cache it.
                nextSection = currentSection.GetFacet<NextSection>().LeadsTo();
            }
            
            // If the next section is null, end the dialogue and return
            if (nextSection == null) {
                EndDialogue();
                return;
            }

            // If the dialogue panel has finished displaying the text, navigate
            // to the next section
            if (dialoguePanel.OnStandby()) {
                SetCurrentSection(nextSection);
            } else {
                // Otherwise, fully display the text
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