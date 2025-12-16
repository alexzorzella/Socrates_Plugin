using System.Text.RegularExpressions;
using UnityEngine;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly string prompt;
        readonly string reference;
        DialogueSection next;
        
        static readonly Regex optionReader = new(@"^(.*),(.*)$");
        
        public NextSection(string reference) {
            
            
            
            this.reference = reference;
            
            TryCache();
        }
        
        public NextSection(DialogueSection next) {
            this.next = next;
        }

        void TryCache() {
            try {
                if (next == null) {
                    next = DialogueManifest.GetSectionByReference(reference);
                }
            }
            catch {
                Debug.LogWarning($"Didn't find a dialogue section with reference {reference}.");
            }
        }
        
        public DialogueSection LeadsTo() {
            TryCache();
            
            return next;
        }

        public string LeadsToRef() {
            return reference;
        }

        public string Prompt() {
            return prompt;
        }
    }
}