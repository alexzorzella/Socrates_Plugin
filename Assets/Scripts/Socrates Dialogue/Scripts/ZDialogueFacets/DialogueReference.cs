using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueReference : ZDialogueFacet {
        string reference;

        public DialogueReference(string reference) {
            this.reference = reference;
        }

        public override string ToString() {
            return reference;
        }

        public void SetReference(string newReference) {
            reference = newReference;
        }
        
        public string GetReference() {
            return reference;
        }
    }
}