using System.Collections.Generic;
using System.IO;
using System.Linq;
using NewSocratesDialogue;
using UnityEngine;

public static class DialogueParser {
    const string defaultDialogueSound = "dialogue";
    
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
        
            string[] entries = line.Split('\t');

            if (entries.Length < 2) {
                continue;
            }
            
            string speaker = entries[0];            // Pulls the speaker's name from the first column
            string content = entries[1];            // Pulls the section's content from the second column

            string sound = defaultDialogueSound;
            
            if (entries.Length > 2) {               // Pulls the dialogue sound name from the third column. If none was provided, it keeps the default.
                string soundToken = entries[2].Replace("\r", string.Empty).Replace("\n", string.Empty);
                if (!string.IsNullOrEmpty(soundToken)) {
                    sound = soundToken;
                }
            }
            
            if (string.IsNullOrEmpty(content)) {    // Breaks if the contents of the dialogue were empty.
                break;
            }
            
            DialogueSection current = new DialogueSection(speaker, content, sound);

            if (i > 0) {
                results.Last().AddFacet(new NextSection(current));
            }
        
            results.Add(current);
        }
    
        return results[0];
    }

    /// <summary>
    /// Returns test dialogue parsed from Assets/StreamingAssets/Localization/test_dialogue.tsv
    /// </summary>
    /// <returns></returns>
    public static DialogueSection TestDialogue() {
        return ParseFile("test_dialogue");
    }
}