using System;
using System.Collections.Generic;
using SocratesDialogue;

public class DialogueManifest {
    static int counter = 0;

    public static string GetUniqueReference() {
        while (sectionsByReference.ContainsKey(counter.ToString())) {
            counter++;
        }
        
        string result = counter.ToString();
        counter++;
        
        return result;
    }
    
    static readonly Dictionary<string, DialogueSection> sectionsByReference = new();

    public static DialogueSection GetSectionByReference(string reference) {
        if (sectionsByReference.ContainsKey(reference)) {
            return sectionsByReference[reference];
        }
        
        throw new NullReferenceException($"{reference} doesn't reference a dialogue section.");
    }

    public static string AddEntry(string uniqueReference, DialogueSection current) {
        if (sectionsByReference.ContainsKey(uniqueReference)) {
            uniqueReference = GetUniqueReference();
        }
        
        sectionsByReference.Add(uniqueReference, current);

        return uniqueReference;
    }
}