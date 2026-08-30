using System;
using System.Collections.Generic;
using System.Linq;

namespace SocratesDialogue {
    public class Dialogue {
        readonly List<DialogueSection> sections;

        public Dialogue(List<DialogueSection> sections) {
            this.sections = sections;
        }

        public DialogueSection GetSectionById(string id) {
            DialogueSection result = Array.Find(sections.ToArray(), s => s.GetReferenceId() == id);
            return result;
        }

        public DialogueSection GetFirstSection() {
            if (sections.Count == 0) { return null; }
            
            return sections.First();
        }
    }
}