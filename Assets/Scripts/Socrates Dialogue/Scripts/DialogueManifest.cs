using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SocratesDialogue;
using UnityEngine;

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
    static readonly Dictionary<string, string> tokenReplacements = new();

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

    public static void AddTokenReplacement(string token, string replaceWith) {
        if (string.IsNullOrWhiteSpace(token)) {
            return;
        }
        
        tokenReplacements.Add(token, replaceWith);
    }

    static readonly Regex tokenMatch = new("{([^{}]*)}|([^{}]+)");
    
    public static string ReplaceTokensIn(string input) {
        string result = "";
        
        Match regexMatch = tokenMatch.Match(input);

        while(regexMatch.Success) {
            string sentenceChunkRaw = regexMatch.Groups[0].Value;
            string sentenceChunk = regexMatch.Groups[1].Value;

            if (sentenceChunk.Length > 0 && sentenceChunkRaw[0] == '{') {
                result += GetReplacementFor(sentenceChunk);
            } else {
                result += sentenceChunkRaw;
            }

            regexMatch = regexMatch.NextMatch();
        }

        if (string.IsNullOrWhiteSpace(result)) {
            result = input;
        }

        return result;
    }
    
    public static string GetReplacementFor(string token) {
        string result = "";
        
        if (tokenReplacements.ContainsKey(token)) {
            result = tokenReplacements[token];
        }

        return result;
    }

    public static void UpdateReference(string forToken, string newReplacement) {
        if (tokenReplacements.ContainsKey(forToken)) {
            tokenReplacements[forToken] = newReplacement;
        } else {
            Debug.LogWarning($"The token '{forToken}' isn't present in the collection of replacements.");
        }
    }

    public static void ClearTokenReplacements() {
        tokenReplacements.Clear();
    }
}