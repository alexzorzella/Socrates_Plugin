using System.Collections.Generic;
using System.Linq;
using SocratesDialogue;

public class DialogueBuilder {
    readonly Dictionary<string, SectionBuilder> sectionCache = new();

    public DialogueBuilder(params SectionBuilder[] sectionBuilders) {
        for (int i = 0; i < sectionBuilders.Length; i++) {
            WithSection(sectionBuilders[i]);
        }
    }

    public DialogueBuilder WithSection(SectionBuilder section) {
        string reference = section.GetReference((sectionCache.Count - 1).ToString());
        sectionCache.Add(reference, section);
        return this;
    }

    public DialogueSection Build() {
        Dictionary<string, DialogueSection> manifest = new();

        foreach (var entry in sectionCache) {
            manifest.Add(entry.Key, entry.Value.Build());
        }

        foreach (var entry in manifest) {
            DialogueSection section = entry.Value;
            List<NextSection> nextSections = section.GetFacets<NextSection>();

            foreach (var nextSection in nextSections) {
                string nextSectionReference = nextSection.GetNextSectionReference();

                if (manifest.ContainsKey(nextSectionReference)) {
                    nextSection.SetNextSection(manifest[nextSectionReference]);
                }
            }
        }

        return manifest.First().Value;
    }
}