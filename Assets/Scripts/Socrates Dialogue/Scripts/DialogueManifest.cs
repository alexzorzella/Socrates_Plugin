using System;
using System.Collections.Generic;
using SocratesDialogue;

public class DialogueManifest {
    static DialogueManifest _i;

    static readonly List<string> dialogueFilenames = new() { "test_dialogue" };

    DialogueManifest() {
        ParseFiles();
    }
    
    void ParseFiles() {
        foreach (string filename in dialogueFilenames) {
            DialogueParser.ParseFile(filename, this);
        }
    }
    
    public static DialogueManifest i {
        get {
            if (_i == null) {
                _i = new DialogueManifest();
            }
            return _i;
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
    
    static readonly Dictionary<string, DialogueSection> sectionsByReference = new();

    public DialogueSection GetSectionByReference(string reference) {
        if (sectionsByReference.ContainsKey(reference)) {
            return sectionsByReference[reference];
        }
        
        throw new NullReferenceException($"{reference} doesn't reference a dialogue section.");
    }

    public string AddEntry(string uniqueReference, DialogueSection current) {
        if (sectionsByReference.ContainsKey(uniqueReference) || string.IsNullOrEmpty(uniqueReference)) {
            uniqueReference = GetUniqueReference();
        }
        
        sectionsByReference.Add(uniqueReference, current);

        return uniqueReference;
    }
}