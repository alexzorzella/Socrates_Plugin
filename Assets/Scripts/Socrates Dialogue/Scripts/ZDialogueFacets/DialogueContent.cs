using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueContent : ZDialogueFacet {
        readonly string content;

        public DialogueContent(string content) {
            this.content = content;
        }

        public override string ToString() {
            return DialogueManifest.ReplaceTokensIn(content);
        }
    }
}