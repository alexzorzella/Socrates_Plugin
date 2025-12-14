using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class NextSection : ZFacet {
        readonly List<DialogueSection> choices;

        public NextSection(params DialogueSection[] choices) {
            this.choices = choices.ToList();
        }

        public DialogueSection Next(int choiceIndex = 0) {
            return choices[choiceIndex];
        }
    }
}