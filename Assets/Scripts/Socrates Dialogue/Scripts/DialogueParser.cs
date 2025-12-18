using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using System.Runtime.CompilerServices;
using UnityEditor;

[assembly: InternalsVisibleTo("EditMode")]

namespace SocratesDialogue {
    public static class DialogueParser {
        const int maxEmptyLinesBeforeBreak = 50;
        
        internal enum ParsingMode { TAG_BY_CELL, TAG_BY_COLUMN, TOKEN_DEF, SKIP_LINE }
        
        /// <summary>
        /// Parses dialogue from the passed .tsv file found in the StreamingAssets/Localization folder.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DialogueSection ParseFile(string filename) {
            List<List<DialogueSection>> results = new();

            var filepath = Path.Combine(Application.streamingAssetsPath, "Dialogue", $"{filename}.tsv");

            if (!File.Exists(filepath)) {
                Debug.LogWarning($"No dialogue file named {filename}.tsv found");
                return null;
            }

            string contents = File.ReadAllText(filepath);

            string[] lines = contents.Split('\n');

            // TODO: This can be refactored so that the loop just waits for the first line with
            // content, and then treats that until the next empty line as a block.
            int emptyLineCount = 0;
            int currentConversationIndex = 0;
            results.Add(new List<DialogueSection>());

            ParsingMode parsingMode = ParsingMode.SKIP_LINE;

            string[] columns = Array.Empty<string>();
            
            // For each line in the .tsv
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];

                List<ZDialogueFacet> facets = new();

                if (ParsingModeFromLine(line) == ParsingMode.SKIP_LINE) {
                    parsingMode = ParsingMode.SKIP_LINE;
                }
                
                switch (parsingMode) {
                    case ParsingMode.TOKEN_DEF:
                        string[] entries = line.Split('\t');
                        DialogueManifest.AddReference(entries[0], entries[1]);
                        
                        continue;
                }
                
                parsingMode = ParsingModeFromLine(line);
                
                switch (parsingMode) {
                    // The line is empty and will be skipped
                    case ParsingMode.SKIP_LINE:
                        // The columns are cleared
                        columns = Array.Empty<string>();
                        
                        emptyLineCount++;

                        // Break when encountering too many empty lines
                        if (emptyLineCount > maxEmptyLinesBeforeBreak) {
                            break;
                        }

                        // If the current conversation has lines, make a new one
                        if (results[currentConversationIndex].Count > 0) {
                            currentConversationIndex++;
                            results.Add(new List<DialogueSection>());
                        }

                        continue;
                    // From the next line until an empty one, the cells' attributes
                    // will be determined by the entries in this one
                    case ParsingMode.TAG_BY_COLUMN:
                        string[] entries = line.Split('\t');
                        columns = entries;
                        continue;
                    // From the next line until an empty one, the cells will
                    // define the tokens' replacements
                    case ParsingMode.TOKEN_DEF:
                        continue;
                }

                facets = ParseFacetsFrom(line, columns);
                
                // Create a new instance of a dialogue section passing the facets
                DialogueSection newSection = new DialogueSection(facets);

                emptyLineCount = 0;

                string uniqueReference = "";
                
                // If the user explicitly points to the next section, use that reference
                if (newSection.HasFacet<DialogueReference>()) {
                    uniqueReference = newSection.GetFacet<DialogueReference>().ToString();
                }
                
                // Added the unique reference to the manifest paired with the new section.
                // If the unique reference is empty or whitespace, it'll generate a new,
                // unique reference for it.
                uniqueReference = DialogueManifest.AddEntry(uniqueReference, newSection);
                
                // If this isn't the first line of conversation,
                if (results[currentConversationIndex].Count > 0) {
                    DialogueSection lastSection = results[currentConversationIndex].Last();

                    // and the last section didn't have choices
                    if (lastSection.CountOfFacetType<NextSection>() <= 0) {
                        // the last dialogue section should lead to this one.
                        lastSection.AddFacet(new NextSection(uniqueReference));   
                    }
                }
                
                // Add the new section to the current cached conversation
                results[currentConversationIndex].Add(newSection);
            }

            if (results.Count < 0 || results[0].Count < 0) {
                return null;
            }
            
            // Debug.Log($"Parsed {results.Count} conversation(s) from {filename}");
            
            return results[0][0];
        }

        internal static ParsingMode ParsingModeFromLine(string line) {
            ParsingMode result = ParsingMode.TAG_BY_CELL;

            string[] entries = line.Split('\t');

            string firstEntry = entries[0];
            
            if (!string.IsNullOrWhiteSpace(firstEntry)) {
                if (firstEntry == "token") {
                    result = ParsingMode.TOKEN_DEF;
                } else {
                    foreach(var token in tokenToFacet) {
                        if (firstEntry == token.Key) {
                            result = ParsingMode.TAG_BY_COLUMN;
                            break;
                        }
                    }
                }
            } else {
                result = ParsingMode.SKIP_LINE;
            }

            return result;
        }

        static readonly Regex facetReader = new(@"^([a-zA-Z]+):[ ]*(.*)$");

        static readonly Dictionary<string, Func<object, ZDialogueFacet>> tokenToFacet = new() {
            { "name", passedValue => new DialogueSpeaker((string)passedValue) },
            { "speaker", passedValue => new DialogueSpeaker((string)passedValue) },
            { "content", passedValue => new DialogueContent((string)passedValue) },
            { "option", passedValue => new NextSection((string)passedValue) },
            { "next", passedValue => new NextSection((string)passedValue) },
            { "choice", passedValue => new NextSection((string)passedValue) },
            { "dialogue", passedValue => new DialogueContent((string)passedValue) },
            { "sound", passedValue => new DialogueSound((string)passedValue) },
            { "delay", passedValue => new CharDelay((string)passedValue) },
            { "event", passedValue => new DialogueEvent((string)passedValue) },
            { "soundbite", passedValue => new DialogueSoundbite((string)passedValue) },
            { "ref", passedValue => new DialogueReference((string)passedValue) },
            { "reference", passedValue => new DialogueReference((string)passedValue) }
        };

        static List<ZDialogueFacet> ParseFacetsFrom(string line, string[] columns) {
            List<ZDialogueFacet> results = new();

            string[] entries = line.Split('\t');

            for (int i = 0; i < entries.Length; i++) {
                string token = "";
                string passedValue = "";

                bool noColumns = columns.Length <= 0;
                bool columnEmpty = (i >= columns.Length) || string.IsNullOrWhiteSpace(columns[i]);
                
                if (noColumns || columnEmpty) {
                    string entry = entries[i];
                    
                    Match regexMatch = facetReader.Match(entry);
    
                    if (string.IsNullOrWhiteSpace(entry)) {
                        continue;
                    }
    
                    if (!regexMatch.Success) {
                        // Debug.LogError($"{entry} couldn't be parsed.");
                        continue;
                    }
    
                    token = regexMatch.Groups[1].Value;
                    token = token.ToLower();
    
                    if (!tokenToFacet.ContainsKey(token)) {
                        Debug.LogWarning($"There is no token called '{token}'. Input '{entry}' couldn't be parsed.");
                        continue;
                    }
    
                    passedValue = regexMatch.Groups[2].Value;
                } else {
                    token = columns[i];
                    passedValue = entries[i];
                }

                if (string.IsNullOrWhiteSpace(token)) {
                    continue;
                }
                
                ZDialogueFacet facet = tokenToFacet[token](passedValue);
                results.Add(facet);
            }

            return results;
        }
    }
}