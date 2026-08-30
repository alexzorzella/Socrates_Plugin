using System.Collections.Generic;
using UnityEngine;

namespace SocratesDialogue {
    public class DialogueBuilder {
        readonly List<SectionBuilder> sectionBuilders = new();

        public DialogueBuilder WithSection(SectionBuilder section) {
            section.GetOrSetReferenceIdToFallback(fallback: $"section_{sectionBuilders.Count}");
            sectionBuilders.Add(section);
            return this;
        }

        public DialogueBuilder WithSequentialSections(params SectionBuilder[] passedSectionBuilders) {
            for (int i = 0; i < passedSectionBuilders.Length; i++) {
                SectionBuilder sectionBuilder = passedSectionBuilders[i];

                if (i < passedSectionBuilders.Length - 1) {
                    if (!sectionBuilder.HasNextSection()) {
                        sectionBuilder.WithNextSection(passedSectionBuilders[i + 1].GetOrSetReferenceIdToFallback($"section_{sectionBuilders.Count}"));
                    }
                }

                WithSection(sectionBuilder);
            }

            return this;
        }

        public Dialogue Bake(List<DialogueSection> sections = null) {
            if (sections == null) {
                sections = new List<DialogueSection>();
                
                foreach (var entry in sectionBuilders) {
                    sections.Add(entry.Build());
                }
            }

            Dialogue dialogue = new Dialogue(sections);

            foreach (var section in sections) {
                List<NextSection> nextSections = section.GetFacets<NextSection>();

                foreach (var nextSection in nextSections) {
                    string nextSectionReferenceId = nextSection.GetNextSectionReference();

                    DialogueSection sectionById = dialogue.GetSectionById(nextSectionReferenceId);

                    if (sectionById != null) {
                        nextSection.SetNextSection(sectionById);
                    }
                    else {
                        Debug.LogWarning($"No dialogue section with id '{nextSectionReferenceId}' found.");
                    }
                }
            }

            return dialogue;
        }
    }
}