using System.Text.RegularExpressions;
using UnityEngine;

namespace SocratesDialogue {
    public class NextSection : ZDialogueFacet {
        readonly string choicePrompt;
        string nextSectionRef;
        DialogueSection nextSection;
        
        static readonly Regex optionReader = new(@"^(.*),(.*)$");
        
        // public NextSection(string rawInput) {
        //     var optionMatch = optionReader.Match(rawInput);
        //    
        //     if (!optionMatch.Success) {
        //         nextSectionRef = rawInput;
        //     } else if(optionMatch.Groups.Count == 3) {
        //         choicePrompt = optionMatch.Groups[1].Value;
        //         nextSectionRef = optionMatch.Groups[2].Value;
        //     }
        //
        //     TryCache();
        // }
        
        public NextSection(string nextSectionRef) {
            this.nextSectionRef = nextSectionRef;
        }
        
        public NextSection(string choicePrompt, string nextSectionRef) {
            this.choicePrompt = choicePrompt;
            this.nextSectionRef = nextSectionRef;
            TryCache();
        }
        
        public NextSection(DialogueSection nextSection) {
            this.nextSection = nextSection;
        }

        void TryCache() {
            try {
                if (nextSection == null) {
                    nextSection = DialogueManifest.GetSectionByReference(nextSectionRef);
                }
            }
            catch {
                Debug.LogWarning($"Didn't find a dialogue section with reference {nextSectionRef}.");
            }
        }

        public void SetNextSection(DialogueSection nextSection) {
            this.nextSection = nextSection;
        }
        
        public DialogueSection GetNextSection() {
            TryCache();
            
            return nextSection;
        }

        public string GetNextSectionReference() {
            return nextSectionRef;
        }

        public string Prompt() {
            return choicePrompt;
        }

        public override string ToString() {
            return $"next (ref): {nextSectionRef}, (actual): {nextSection.GetReferenceId()}";
        }
    }
}