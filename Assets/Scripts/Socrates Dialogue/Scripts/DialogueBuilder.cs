using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SocratesDialogue {
    public class DialogueBuilder {
        readonly List<SectionBuilder> sectionBuilders = new();

        public int GetCurrentSectionBuilderCount() {
            return sectionBuilders.Count;
        }

        public SectionBuilder GetLastSectionBuilder() {
            return sectionBuilders.Last();
        }

        public DialogueBuilder WithSection(SectionBuilder section) {
            section.GetOrSetReferenceIdToFallback(fallback: $"section_{sectionBuilders.Count}");
            sectionBuilders.Add(section);
            return this;
        }

        public DialogueBuilder WithSequentialSections(List<SectionBuilder> passedSectionBuilders) {
            for (int i = 0; i < passedSectionBuilders.Count; i++) {
                SectionBuilder sectionBuilder = passedSectionBuilders[i];

                if (i < passedSectionBuilders.Count - 1) {
                    if (!sectionBuilder.HasNextSection()) {
                        sectionBuilder.WithNextSection(passedSectionBuilders[i + 1].GetOrSetReferenceIdToFallback($"section_{sectionBuilders.Count}"));
                    }
                }

                WithSection(sectionBuilder);
            }

            return this;
        }

        public DialogueBuilder WithSequentialSections(params SectionBuilder[] passedSectionBuilders) {
            return WithSequentialSections(passedSectionBuilders.ToList());
        }

        public Dialogue Bake() {
            List<DialogueSection> sections = new List<DialogueSection>();
            
            foreach (var entry in sectionBuilders) {
                DialogueSection builtSection = entry.Build();
                DialogueManifest.TryAddEntry(entry.GetOrSetReferenceIdToFallback(), builtSection);
                sections.Add(builtSection);
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