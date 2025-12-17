using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SocratesDialogue {
    public class DialogueEvent : ZDialogueFacet {
        static readonly Regex eventMatch = new(@"^([a-zA-Z0-9\-_]+)\((.*)\)");

        string eventTag = "";
        string parameters = "";
        
        public DialogueEvent(string rawInput) {
            Match regexMatch = eventMatch.Match(rawInput);

            if (!regexMatch.Success) {
                return;
            }

            eventTag = regexMatch.Groups[1].Value;

            if (regexMatch.Groups.Count > 2) {
                parameters = regexMatch.Groups[2].Value;
            }
        }

        public string GetTag() {
            return eventTag;
        }

        public string GetParameters() {
            return parameters;
        }
    }
}