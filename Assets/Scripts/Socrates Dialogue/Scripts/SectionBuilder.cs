using System;
using System.Collections.Generic;

namespace SocratesDialogue {
    public class SectionBuilder {
        string referenceId = null;
        readonly List<ZDialogueFacet> facets = new();

        public string GetOrSetReferenceIdToFallback(string fallback = null) {
            if (referenceId == null && !string.IsNullOrEmpty(fallback)) {
                referenceId = fallback;
            }

            return referenceId;
        }

        /// <summary>
        /// This constructor is meant to be called from the parser.
        /// </summary>
        /// <param name="referenceId"></param>
        /// <param name="facets"></param>
        public SectionBuilder(string referenceId, List<ZDialogueFacet> facets) {
            this.referenceId = referenceId;
            this.facets = facets;
        }
        
        public SectionBuilder(string speaker, string content, string reference = null) {
            facets.Add(new DialogueSpeaker(speaker));
            facets.Add(new DialogueContent(content));
            if (!string.IsNullOrEmpty(reference)) {
                referenceId = reference;
            }
        }

        public SectionBuilder WithSoundbite(string soundName) {
            facets.Add(new DialogueSoundbite(soundName));
            return this;
        }

        public SectionBuilder WithDialogueSound(string soundName) {
            facets.Add(new DialogueSound(soundName));
            return this;
        }

        public SectionBuilder WithDelay(float delay) {
            facets.Add(new CharDelay(delay));
            return this;
        }

        public SectionBuilder WithNextSection(DialogueSection nextSection) {
            facets.Add(new NextSection(nextSection));
            return this;
        }

        bool hasNextSection;

        public SectionBuilder WithNextSection(string nextSection) {
            facets.Add(new NextSection(nextSection));
            hasNextSection = true;
            return this;
        }

        public SectionBuilder WithChoice(string prompt, string leadsTo = null) {
            facets.Add(new NextSection(prompt, leadsTo));
            return this;
        }

        public SectionBuilder WithAction(Action action,
            DialogueActionTime dialogueActionTime = DialogueActionTime.AFTER_DISPLAYING_TEXT) {
            facets.Add(new DialogueAction(action, dialogueActionTime));
            return this;
        }

        public bool HasNextSection() {
            return hasNextSection;
        }

        public DialogueSection Build() {
            return new DialogueSection(referenceId, facets);
        }
    }
}