using System.Collections.Generic;
using System.Linq;

namespace NewSocratesDialogue {
    public class NextSection : ZFacet {
        readonly List<NewDialogueSection> choices;

        public NextSection(params NewDialogueSection[] choices) {
            this.choices = choices.ToList();
        }
        
        // public ZFacet Clone() {
        // return new NextSection(choices.Select(item => new NewDialogueSection(item)).ToList());
        // }
    }
}