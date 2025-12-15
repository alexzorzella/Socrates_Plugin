using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueSpeaker : ZDialogueFacet {
        readonly string speaker;

        public DialogueSpeaker(string speaker) {
            this.speaker = speaker;
        }

        public override string ToString() {
            return speaker;
        }
    }
}