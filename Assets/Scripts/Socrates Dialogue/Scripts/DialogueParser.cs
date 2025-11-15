using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static SocratesDialogue;

public static class DialogueParser {
    public static DialogueSection ParseFile(string filename) {
        List<DialogueSection> results = new();

        var filepath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Localization", $"{filename}.tsv");
        
        if (!File.Exists(filepath)) {
            Debug.LogWarning($"No dialogue file named {filename}.tsv found");
            return null;
        }
        
        string contents = File.ReadAllText(filepath);
	
        string[] lines = contents.Split('\n');

        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
        
            string[] entries = line.Split('\t');

            if (entries.Length < 2) {
                continue;
            }
            
            string speaker = entries[0];
            string content = entries[1];

            string sound = "dialogue";

            if (entries.Length > 2) {
                string soundToken = entries[2].Replace("\r", string.Empty).Replace("\n", string.Empty);
                if (!string.IsNullOrEmpty(soundToken)) {
                    sound = soundToken;
                }
            }

            if (string.IsNullOrEmpty(content)) {
                break;
            }
            
            Monologue current = new Monologue(speaker, content, sound);

            if (i > 0) {
                results.Last().SetNext(current);
            }
        
            results.Add(current);
        }
    
        return results[0];
    }

    public static DialogueSection TestDialogue() {
        return ParseFile("dialogue");
    }
}