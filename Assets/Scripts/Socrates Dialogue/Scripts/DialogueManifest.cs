using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SocratesDialogue;
using UnityEngine;

public static class DialogueManifest {
    static readonly List<string> dialogueFilenames = new() {
        "test_dialogue"
    };
    
    /// <summary>
    /// Parses each file listed in dialogueFilenames
    /// </summary>
    static void ParseFiles() {
        sectionsByReference = new();
        
        foreach (string filename in dialogueFilenames) {
            DialogueParser.ParseFile(filename);
        }
    }
    
    static Dictionary<string, DialogueSection> sectionsByReference;
    static readonly Dictionary<string, string> dialogueVariables = new();

    static int counter = 0;
    
    /// <summary>
    /// Returns a unique reference not present in the dialogue dictionary
    /// </summary>
    /// <returns></returns>
    static string GetUniqueReference() {
        while (sectionsByReference.ContainsKey(counter.ToString())) {
            counter++;
        }
        
        string result = counter.ToString();
        counter++;
        
        return result;
    }
    
    /// <summary>
    /// Returns a dialogue section with the passed reference if one exist.
    /// Makes sure that the dialogues have been parsed. If they haven't,
    /// it parses them before doing anything
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static DialogueSection GetSectionByReference(string reference) {
        if (sectionsByReference == null) {
            ParseFiles();
        }
        
        if (sectionsByReference.ContainsKey(reference)) {
            return sectionsByReference[reference];
        }
        
        throw new NullReferenceException($"{reference} doesn't reference a dialogue section.");
    }

    /// <summary>
    /// Adds a dialogue section linked with a passed reference. If the reference is already present
    /// in the dictionary, a new reference is generated and returned.
    /// </summary>
    /// <param name="uniqueReference"></param>
    /// <param name="current"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Adds the passed dialogue variable associated with its replacement to the
    /// dictionary of dialogue variables.
    /// </summary>
    /// <param name="variableReference"></param>
    /// <param name="replaceWith"></param>
    public static void AddDialogueVariable(string variableReference, string replaceWith) {
        if (string.IsNullOrWhiteSpace(variableReference)) {
            return;
        }
        
        dialogueVariables.Add(variableReference, replaceWith);
    }

    static readonly Regex tokenMatch = new("{([^{}]*)}|([^{}]+)");
    
    /// <summary>
    /// Replaces all dialogue variables in the input with their
    /// respective values.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ReplaceTokensIn(string input) {
        string result = "";
        
        Match regexMatch = tokenMatch.Match(input);

        while(regexMatch.Success) {
            string sentenceChunkRaw = regexMatch.Groups[0].Value;
            string sentenceChunk = regexMatch.Groups[1].Value;

            if (sentenceChunk.Length > 0 && sentenceChunkRaw[0] == '{') {
                result += GetDialogueVariableReplacementFor(sentenceChunk);
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
    
    /// <summary>
    /// Returns the replacement for the dialogue variable with the passed reference.
    /// </summary>
    /// <param name="variableReference"></param>
    /// <returns></returns>
    static string GetDialogueVariableReplacementFor(string variableReference) {
        string result = "";
        
        if (dialogueVariables.ContainsKey(variableReference)) {
            result = dialogueVariables[variableReference];
        }

        return result;
    }

    /// <summary>
    /// Updates the value of the dialogue variable with the passed variable
    /// reference to be the new replacement.
    /// </summary>
    /// <param name="variableReference"></param>
    /// <param name="newReplacement"></param>
    public static void UpdateDialogueVariableValue(string variableReference, string newReplacement) {
        if (dialogueVariables.ContainsKey(variableReference)) {
            dialogueVariables[variableReference] = newReplacement;
        } else {
            Debug.LogWarning($"The token '{variableReference}' isn't present in the collection of replacements.");
        }
    }

    /// <summary>
    /// Clears the dialogue variables.
    /// </summary>
    public static void ClearDialogueVariables() {
        dialogueVariables.Clear();
    }
}