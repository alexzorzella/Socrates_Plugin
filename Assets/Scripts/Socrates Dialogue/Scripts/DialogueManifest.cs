using System;
using System.Collections.Generic;
using SocratesDialogue;

public static class DialogueManifest {
    static readonly List<string> dialogueFilenames = new() {
        "test_dialogue",
        "test_dialogue 1",
        "test_dialogue 2",
        "test_dialogue 3",
        "test_dialogue 4",
        "test_dialogue 5",
        "test_dialogue 6",
        "test_dialogue 7",
        "test_dialogue 8"
    };
    
    static void ParseFiles() {
        sectionsByReference = new();
        
        foreach (string filename in dialogueFilenames) {
            DialogueParser.ParseFile(filename);
        }
    }
    
    static int counter = 0;

    static string GetUniqueReference() {
        while (sectionsByReference.ContainsKey(counter.ToString())) {
            counter++;
        }
        
        string result = counter.ToString();
        counter++;
        
        return result;
    }
    
    static Dictionary<string, DialogueSection> sectionsByReference;

    public static DialogueSection GetSectionByReference(string reference) {
        if (sectionsByReference == null) {
            ParseFiles();
        }
        
        if (sectionsByReference.ContainsKey(reference)) {
            return sectionsByReference[reference];
        }
        
        throw new NullReferenceException($"{reference} doesn't reference a dialogue section.");
    }

    public static string AddEntry(string uniqueReference, DialogueSection current) {
        if (sectionsByReference == null) {
            ParseFiles();
        }
        
        if (sectionsByReference.ContainsKey(uniqueReference) || string.IsNullOrEmpty(uniqueReference)) {
            uniqueReference = GetUniqueReference();
        }
        
        sectionsByReference.Add(uniqueReference, current);

        return uniqueReference;
    }
}