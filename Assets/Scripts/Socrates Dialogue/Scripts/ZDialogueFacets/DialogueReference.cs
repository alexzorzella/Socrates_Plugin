using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueReference : ZDialogueFacet {
        readonly string reference;

        public DialogueReference(string reference) {
            this.reference = reference;
        }

        public override string ToString() {
            return reference;
        }
    }
}