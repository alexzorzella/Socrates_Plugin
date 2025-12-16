using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEngine;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly string reference;
        DialogueSection next;

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
        
        public DialogueSection Next() {
            TryCache();
            
            return next;
        }
    }
}