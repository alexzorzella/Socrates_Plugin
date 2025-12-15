using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class DialogueSound : ZDialogueFacet {
        readonly string sound;

        public DialogueSound(string sound) {
            this.sound = sound;
        }

        public override string ToString() {
            return sound;
        }
    }
}