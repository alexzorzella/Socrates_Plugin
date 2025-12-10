using System.Collections.Generic;
using System.Linq;

namespace NewSocratesDialogue {
    public class NextSection : ZFacet {
        readonly List<DialogueSection> choices;

        public NextSection(params DialogueSection[] choices) {
            this.choices = choices.ToList();
        }

        public DialogueSection Next() {
            return choices[0];
        }
        
        // public ZFacet Clone() {
        // return new NextSection(choices.Select(item => new NewDialogueSection(item)).ToList());
        // }
    }
}