using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueSoundbite : ZDialogueFacet {
        readonly string sound;

        public DialogueSoundbite(string sound) {
            this.sound = sound;
        }

        public override string ToString() {
            return sound;
        }
    }
}