using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SocratesDialogue {
    public class FancyText {
        readonly string rawText;
        string cleanedText; // TODO: Make this readonly
        readonly List<AnnotationToken> annotationTokens = new();

        /// <summary>
        /// The list of annotations in the text.
        /// </summary>
        /// <returns></returns>
        public List<AnnotationToken> GetAnnotationTokens() {
            return annotationTokens;
        }

        public FancyText(string rawText, bool noPunctuationAnnotation = false) {
            this.rawText = rawText;

            AnnotateByMarkup();
            LinkTokens();
            CleanText(noPunctuationAnnotation);
        }

        /// <summary>
        /// Caches annotations according to markup found in the text.
        /// </summary>
        /// <param name="startAt"></param>
        void AnnotateByMarkup(int startAt = 0) {
            for (var i = startAt; i < rawText.Length; i++) {
                if (rawText[i] == SocraticAnnotation.parseStartChar) {
                    AnnotationToken newToken = new();
                    newToken.startCharIndex = i;

                    var compareProfileTo = "";
                    var dynamicValue = "";
                    var readingDynamicValue = false;

                    for (var f = i + 1; f < rawText.Length; f++) {
                        if (rawText[f] == SocraticAnnotation.parseEndChar) {
                            newToken.endCharIndex = f;

                            if (compareProfileTo.Contains(SocraticAnnotation.waveTag))
                                newToken.richTextType = SocraticAnnotation.RichTextType.WAVE;
                            else if (compareProfileTo.Contains(SocraticAnnotation.delayTag))
                                newToken.richTextType = SocraticAnnotation.RichTextType.DELAY;
                            else if (compareProfileTo.Contains(SocraticAnnotation.shakeTag))
                                newToken.richTextType = SocraticAnnotation.RichTextType.SHAKE;
                            else
                                Debug.LogError($"'{compareProfileTo}' -- Parse section did not have a valid input.");

                            var contents = "";

                            for (var c = i; c < f; c++) contents += rawText[c];

                            if (contents.Contains(SocraticAnnotation.parseClosePairChar)) newToken.opener = false;

                            if (readingDynamicValue) newToken.passedValue = dynamicValue;

                            annotationTokens.Add(newToken);

                            AnnotateByMarkup(f);
                            return;
                        }

                        compareProfileTo += rawText[f];

                        if (readingDynamicValue) dynamicValue += rawText[f];

                        if (rawText[f] == SocraticAnnotation.parseValueSeparator) readingDynamicValue = true;
                    }
                }
            }
        }

        /// <summary>
        /// Pairs opening and closing annotation tokens together.
        /// </summary>
        void LinkTokens() {
            List<AnnotationToken> openingTokens = new();
            List<AnnotationToken> closingTokens = new();

            foreach (var token in annotationTokens) {
                if (token.opener) {
                    openingTokens.Add(token);
                }
                else {
                    closingTokens.Add(token);
                }
            }

            foreach (var opener in openingTokens) {
                foreach (var closer in closingTokens) {
                    if (opener.richTextType == closer.richTextType && closer.linkedToken == null) {
                        opener.linkedToken = closer;
                        closer.linkedToken = opener;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Cleans the text by realigning its annotation indices to account for the annotation markup being eaten,
        /// eats the annotation markup, and annotates it by its punctuation by default.
        /// </summary>
        /// <param name="noPunctuationAnnotation"></param>
        void CleanText(bool noPunctuationAnnotation = false) {
            cleanedText = rawText;

            RealignAnnotationTokenIndices();
            DiceAndDevourRichTextTokens();

            if (!noPunctuationAnnotation) {
                AnnotateByPunctuation();
            }
        }

        /// <summary>
        /// Realigns the annotation tokens according to its markup.
        /// </summary>
        void RealignAnnotationTokenIndices() {
            int totalCharactersSnipped = 0;

            foreach (var token in annotationTokens) {
                if (token.startCharIndex != token.endCharIndex) {
                    token.startCharIndex -= totalCharactersSnipped;
                    token.endCharIndex -= totalCharactersSnipped;
                    var start = token.startCharIndex;
                    var length = token.endCharIndex - token.startCharIndex + 1;
                    cleanedText = cleanedText.Remove(start, length);
                    totalCharactersSnipped += length;
                }
            }
        }

        class RichTextToken {
            public int startIndex;
            public int length;

            public RichTextToken(int startIndex) {
                this.startIndex = startIndex;
            }
        }

        /// <summary>
        /// Locates and removes the markup from the text.
        /// </summary>
        void DiceAndDevourRichTextTokens() {
            List<RichTextToken> cache = DiceRichTextTokens();

            DevourRichTextTokens(cache);
        }

        /// <summary>
        /// Locates and returns the markup annotation tokens found in the originally passed text.
        /// </summary>
        /// <returns></returns>
        List<RichTextToken> DiceRichTextTokens() {
            List<RichTextToken> result = new();

            for (int i = 0; i < cleanedText.Length; i++) {
                if (cleanedText[i] == '<') {
                    result.Add(new RichTextToken(i));
                }
                else if (cleanedText[i] == '>') {
                    result[^1].length = i - result[^1].startIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Removes the markup annotation tokens from the originally passed text.
        /// </summary>
        /// <param name="richTextTokens"></param>
        void DevourRichTextTokens(List<RichTextToken> richTextTokens) {
            foreach (var token in annotationTokens) {
                int totalOffset = 0;

                foreach (var richTextToken in richTextTokens) {
                    if (token.startCharIndex > richTextToken.startIndex) {
                        totalOffset += richTextToken.length + 1;
                    }
                }

                token.startCharIndex -= totalOffset;
                token.endCharIndex -= totalOffset;
            }
        }

        /// <summary>
        /// Annotates the text with delays at punctuation marks.
        /// Note! Account for annotation by punctuation while testing or
        /// initialize fancy text with the 'noPunctuationAnnotation' flag.
        /// </summary>
        /// <param name="startAt"></param>
        void AnnotateByPunctuation(int startAt = 0) {
            int invisibleChars = 0;
            bool readingInvisibleChar = false;

            for (var i = startAt; i < cleanedText.Length; i++) {
                char currentChar = cleanedText[i];

                // Begin eating rich text tags
                if (currentChar == SocraticAnnotation.richTextStart) {
                    readingInvisibleChar = true;
                }

                // Continue eating rich text tags
                if (readingInvisibleChar) {
                    invisibleChars++;
                }

                // Finish eating rich text tags
                if (currentChar == SocraticAnnotation.richTextEnd) {
                    readingInvisibleChar = false;
                }

                // If the current character is punctuation, is in bounds, and the parser isn't currently
                // eating an invisible character, a delay token is added to the list of Socratic annotations.
                if (char.IsPunctuation(currentChar) && i < cleanedText.Length - 1 && !readingInvisibleChar) {
                    char nextChar = cleanedText[i + 1];

                    if (nextChar == ' ' || nextChar == SocraticAnnotation.richTextStart) {
                        bool minorDelay = SocraticAnnotation.minorPunctuation.Contains(currentChar);
                        bool majorDelay = SocraticAnnotation.majorPunctuation.Contains(currentChar);

                        if (minorDelay || majorDelay) {
                            AnnotationToken newToken = new();

                            newToken.startCharIndex = i + 1 - invisibleChars;
                            newToken.endCharIndex = i + 1 - invisibleChars;
                            newToken.richTextType = SocraticAnnotation.RichTextType.DELAY;
                            newToken.passedValue = minorDelay
                                ? SocraticAnnotation.minorPunctuationDisplayDelay.ToString()
                                : SocraticAnnotation.majorPunctuationDisplayDelay.ToString();

                            annotationTokens.Add(newToken);
                        }
                    }
                }
            }
        }

        public override string ToString() {
            return cleanedText;
        }
    }
}