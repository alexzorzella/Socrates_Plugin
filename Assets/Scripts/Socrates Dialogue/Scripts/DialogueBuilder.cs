using System;
using System.Collections.Generic;
using SocratesDialogue;

public class DialogueBuilder {
    readonly Dictionary<string, SectionBuilder> sectionCache = new();
    readonly Dictionary<string, DialogueSection> manifest = new();

    bool hasBeenBaked = false;
    
    public DialogueBuilder WithSection(SectionBuilder section) {
        if (hasBeenBaked) {
            throw new Exception("You may not add sections to a baked DialogueBuilder!");
        }
        
        string reference = section.GetReference((sectionCache.Count - 1).ToString());
        sectionCache.Add(reference, section);
        return this;
    }
    
    public DialogueBuilder WithSequentialSections(params SectionBuilder[] sectionBuilders) {
        if (hasBeenBaked) {
            throw new Exception("You may not add sections to a baked DialogueBuilder!");
        }
        
        for (int i = 0; i < sectionBuilders.Length; i++) {
            SectionBuilder sectionBuilder = sectionBuilders[i];
            
            if (i < sectionBuilders.Length - 1) {
                if (!sectionBuilder.HasNextSection())
                    sectionBuilder.WithNextSection(sectionBuilders[i + 1].GetReference());
            }
            
            WithSection(sectionBuilder);
        }

        return this;
    }
    
    public void Bake() {
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

        hasBeenBaked = true;
    }

    public DialogueSection GetSectionById(string key) {
        if (!hasBeenBaked) {
            throw new Exception("You must build a DialogueBuilder before using its contents!");
        }
        
        return manifest.ContainsKey(key) ? manifest[key] : null;
    }
}