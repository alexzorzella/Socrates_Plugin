using System;
using System.Collections.Generic;
using SocratesDialogue;

public class Dialogue {
    readonly List<DialogueSection> sections;

    public Dialogue(List<DialogueSection> sections) {
        this.sections = sections;
    }

    public DialogueSection GetSectionById(string id) {
        DialogueSection result = Array.Find(sections.ToArray(), s => s.GetReferenceId() == id);
        return result;
    }
}