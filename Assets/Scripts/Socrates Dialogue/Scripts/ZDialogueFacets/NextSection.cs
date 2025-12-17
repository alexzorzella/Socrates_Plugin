using System.Text.RegularExpressions;
using UnityEngine;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly string prompt;
        readonly string reference;
        DialogueSection next;
        
        static readonly Regex optionReader = new(@"^(.*),(.*)$");
        
        public NextSection(string rawInput) {
            var optionMatch = optionReader.Match(rawInput);
            
            if (!optionMatch.Success) {
                reference = rawInput;
            } else if(optionMatch.Groups.Count == 3) {
                prompt = optionMatch.Groups[1].Value;
                reference = optionMatch.Groups[2].Value;
            }

            TryCache();
        }
        
        public NextSection(DialogueSection next) {
            this.next = next;
        }

        void TryCache() {
            try {
                if (next == null) {
                    next = DialogueManifest.i.GetSectionByReference(reference);
                }
            }
            catch {
                Debug.LogWarning($"Didn't find a dialogue section with reference {reference}.");
            }
        }
        
        public DialogueSection LeadsTo() {
            TryCache();
            
            return next;
        }

        public string LeadsToRef() {
            return reference;
        }

        public string Prompt() {
            return prompt;
        }
    }
}