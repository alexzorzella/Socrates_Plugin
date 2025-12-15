using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly DialogueSection next;

        public NextSection(DialogueSection next) {
            this.next = next;
        }

        public DialogueSection Next() {
            return next;
        }
    }
}