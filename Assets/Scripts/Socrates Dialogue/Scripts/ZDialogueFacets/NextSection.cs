using System.Text.RegularExpressions;
using UnityEngine;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly string prompt;
        readonly string leadsTo;
        DialogueSection next;
        
        static readonly Regex optionReader = new(@"^(.*),(.*)$");
        
        public NextSection(string rawInput) {
            var optionMatch = optionReader.Match(rawInput);
            
            if (!optionMatch.Success) {
                leadsTo = rawInput;
            } else if(optionMatch.Groups.Count == 3) {
                prompt = optionMatch.Groups[1].Value;
                leadsTo = optionMatch.Groups[2].Value;
            }

            TryCache();
        }

        public NextSection(string prompt, string leadsTo) {
            this.prompt = prompt;
            this.leadsTo = leadsTo;
            TryCache();
        }
        
        public NextSection(DialogueSection next) {
            this.next = next;
        }

        void TryCache() {
            try {
                if (next == null) {
                    next = DialogueManifest.GetSectionByReference(leadsTo);
                }
            }
            catch {
                Debug.LogWarning($"Didn't find a dialogue section with reference {leadsTo}.");
            }
        }

        public void SetNextSection(DialogueSection nextSection) {
            next = nextSection;
        }
        
        public DialogueSection GetNextSection() {
            TryCache();
            
            return next;
        }

        public string GetNextSectionReference() {
            return leadsTo;
        }

        public string Prompt() {
            return prompt;
        }
    }
}