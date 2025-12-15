using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SocratesDialogue;
using UnityEngine;

namespace SocratesDialogue {
    public static class DialogueParser {
        /// <summary>
        /// Parses dialogue from the passed .tsv file found in the StreamingAssets/Localization folder.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DialogueSection ParseFile(string filename) {
            List<DialogueSection> results = new();

            var filepath = Path.Combine(Application.streamingAssetsPath, "Localization", $"{filename}.tsv");

            if (!File.Exists(filepath)) {
                Debug.LogWarning($"No dialogue file named {filename}.tsv found");
                return null;
            }

            string contents = File.ReadAllText(filepath);

            string[] lines = contents.Split('\n');

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];

                List<ZDialogueFacet> facets = ParseFacetsFrom(line);

                DialogueSection current = new DialogueSection(facets);

                if (!current.HasFacet<DialogueContent>()) {
                    break;
                }
                
                if (i > 0) {
                    results.Last().AddFacet(new NextSection(current));
                }

                results.Add(current);
            }

            return results[0];
        }

        static readonly Regex facetReader = new(@"^([a-zA-Z]+):[ ]*([^\[\]]+)$");
        
        static readonly Dictionary<string, Func<object, ZDialogueFacet>> tokenToFacet = new() {
            { "name", passedValue => new DialogueSpeaker((string)passedValue) },
            { "speaker", passedValue => new DialogueSpeaker((string)passedValue) },
            { "content", passedValue => new DialogueContent((string)passedValue) },
            { "dialogue", passedValue => new DialogueContent((string)passedValue) },
            { "sound", passedValue => new DialogueSound((string)passedValue) },
            { "delay", passedValue => new CharDelay((string)passedValue) }
            // { "event", typeof(DialogueEvent) },
            // { "ref", typeof(DialogueRef) },
            // { "reference", typeof(DialogueRef) },
            // { "option", typeof(Next) },
            // { "soundbite", typeof(DialogueSoundbite) }
        };
        
        static List<ZDialogueFacet> ParseFacetsFrom(string line) {
            List<ZDialogueFacet> results = new();
            
            string[] entries = line.Split('\t');

            foreach (string entry in entries) {
                Match regexMatch = facetReader.Match(entry);

                if (string.IsNullOrWhiteSpace(entry)) {
                    continue;
                }
                
                if (!regexMatch.Success) {
                    Debug.LogError($"{entry} couldn't be parsed.");
                    continue;
                }

                string token = regexMatch.Groups[1].Value;
                token = token.ToLower();
                
                if (!tokenToFacet.ContainsKey(token)) {
                    Debug.LogWarning($"There is no token called '{token}'. Input '{entry}' couldn't be parsed.");
                    continue;
                }
                
                string passedValue = regexMatch.Groups[2].Value;
                    
                ZDialogueFacet facet = tokenToFacet[token](passedValue);
                
                results.Add(facet);
            }

            return results;
        }

        /// <summary>
        /// Returns test dialogue parsed from Assets/StreamingAssets/Localization/test_dialogue.tsv
        /// </summary>
        /// <returns></returns>
        public static DialogueSection TestDialogue() {
            return ParseFile("test_dialogue");
        }
    }
}