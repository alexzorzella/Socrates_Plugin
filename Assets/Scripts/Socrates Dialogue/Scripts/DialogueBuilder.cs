using System;
using System.Collections.Generic;
using SocratesDialogue;
using UnityEngine;

public class DialogueBuilder {
    readonly List<SectionBuilder> sectionBuilders = new();
    
    public DialogueBuilder WithSection(SectionBuilder section) {
        section.GetOrSetReferenceIdToFallback(fallback: (sectionBuilders.Count - 1).ToString());
        sectionBuilders.Add(section);
        return this;
    }

    public DialogueBuilder WithSequentialSections(params SectionBuilder[] sectionBuilders) {
        for (int i = 0; i < sectionBuilders.Length; i++) {
            SectionBuilder sectionBuilder = sectionBuilders[i];

            if (i < sectionBuilders.Length - 1) {
                if (!sectionBuilder.HasNextSection())
                    sectionBuilder.WithNextSection(sectionBuilders[i + 1].GetOrSetReferenceIdToFallback());
            }

            WithSection(sectionBuilder);
        }

        return this;
    }
    
    public Dialogue Bake() {
        List<DialogueSection> sections = new();
        
        foreach (var entry in sectionBuilders) {
            sections.Add(entry.Build());
        }
        
        Dialogue dialogue = new Dialogue(sections);

        foreach (var section in sections) {
            List<NextSection> nextSections = section.GetFacets<NextSection>();

            foreach (var nextSection in nextSections) {
                string nextSectionReferenceId = nextSection.GetNextSectionReference();

                DialogueSection sectionById = dialogue.GetSectionById(nextSectionReferenceId);
                
                if (sectionById != null) {
                    nextSection.SetNextSection(sectionById);
                } else {
                    Debug.LogWarning($"No dialogue section with id '{nextSectionReferenceId}' found.");
                }
            }
        }
        
        return dialogue;
    }
}