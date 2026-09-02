using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SocratesDialogue {
    public class DialogueBuilder {
        readonly List<SectionBuilder> sectionBuilders = new();

        /// <summary>
        /// Returns the current number of section builders in the local list of SectionBuilder.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSectionBuilderCount() {
            return sectionBuilders.Count;
        }

        /// <summary>
        /// Returns the last SectionBuilder in the local list of SectionBuilder.
        /// </summary>
        /// <returns></returns>
        public SectionBuilder GetLastSectionBuilder() {
            return sectionBuilders.Last();
        }

        /// <summary>
        /// Adds the passed SectionBuilder to the local list of SectionBuilder and returns the instance of
        /// DialogueBuilder.
        /// </summary>
        /// <param name="sectionBuilder"></param>
        /// <returns></returns>
        public DialogueBuilder WithSection(SectionBuilder sectionBuilder) {
            sectionBuilder.GetOrSetReferenceIdToFallback(fallback: $"section_{sectionBuilders.Count}");
            sectionBuilders.Add(sectionBuilder);
            return this;
        }
        
        /// <summary>
        /// Adds each SectionBuilder in the passed list of SectionBuilder to the local list of SectionBuilder. Every
        /// section except for the last section gets a NextSection with the next SectionBuilder's referenceId. This
        /// makes it so that long, sequential strings of lines don't have to be manually linked together.
        /// </summary>
        /// <param name="passedSectionBuilders"></param>
        /// <returns></returns>
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

        /// <summary>
        /// An overload for WithSequentialSections that accepts the SectionBuilders as params so that
        /// a new list doesn't have to be manually made every time the function is called.
        /// </summary>
        /// <param name="passedSectionBuilders"></param>
        /// <returns></returns>
        public DialogueBuilder WithSequentialSections(params SectionBuilder[] passedSectionBuilders) {
            return WithSequentialSections(passedSectionBuilders.ToList());
        }

        /// <summary>
        /// Iterates through the local list of SectionBuilder and builds each one. Each built DialogueSection
        /// is passed to the DialogueManifest to be potentially added to the map of DialogueSection by reference.
        /// Each NextSection is then linked to the DialogueSections they're pointing to. These facets are
        /// linked here to support circular dependencies. The resulting Dialogue is returned.
        /// </summary>
        /// <returns></returns>
        public Dialogue Build() {
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