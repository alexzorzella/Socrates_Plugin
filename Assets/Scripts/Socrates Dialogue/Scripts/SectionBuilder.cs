using System;
using SocratesDialogue;

public class SectionBuilder {
    readonly DialogueSection section = new();

    public string GetReference(string fallback = "") {
        if (!section.HasFacet<DialogueReference>()) {
            section.AddFacet(new DialogueReference(fallback));
        }

        return section.GetFacet<DialogueReference>().ToString();
    }
    
    public SectionBuilder(string speaker, string content, string reference = "") {
        section.AddFacet(new DialogueSpeaker(speaker));
        section.AddFacet(new DialogueContent(content));
        if(!string.IsNullOrEmpty(reference)) { section.AddFacet(new DialogueReference(reference)); }
    }

    public SectionBuilder WithSoundbite(string soundName) {
        section.AddFacet(new DialogueSoundbite(soundName));
        return this;
    }
    
    public SectionBuilder WithDialogueSound(string soundName) {
        section.AddFacet(new DialogueSound(soundName));
        return this;
    }

    public SectionBuilder WithDelay(float delay) {
        section.AddFacet(new CharDelay(delay));
        return this;
    }
       
    public SectionBuilder WithNextSection(DialogueSection nextSection) {
        section.AddFacet(new NextSection(nextSection));
        return this;
    }

    public SectionBuilder WithNextSection(string nextSection) {
        section.AddFacet(new NextSection().WithNextSectionRef(nextSection));
        return this;
    }
    
    public SectionBuilder WithChoice(string prompt, string leadsTo) {
        section.AddFacet(new NextSection(prompt, leadsTo));
        return this;
    }

    public SectionBuilder WithAction(Action action, DialogueActionTime dialogueActionTime) {
        section.AddFacet(new DialogueAction(action, dialogueActionTime));
        return this;
    }

    public bool HasNextSection() {
        return section.HasFacet<NextSection>();
    }
    
    public DialogueSection Build() {
        return section;
    }
}