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
                    AnnotationToken.Builder newTokenBuilder = new();
                    newTokenBuilder.WithStartCharIndex(i);

                    var compareProfileTo = "";
                    var dynamicValue = "";
                    var readingDynamicValue = false;

                    for (var f = i + 1; f < rawText.Length; f++) {
                        if (rawText[f] == SocraticAnnotation.parseEndChar) {
                            newTokenBuilder.WithEndCharIndex(f);

                            if (compareProfileTo.Contains(SocraticAnnotation.waveTag))
                                newTokenBuilder.WithRichTextType(SocraticAnnotation.RichTextType.WAVE);
                            else if (compareProfileTo.Contains(SocraticAnnotation.delayTag))
                                newTokenBuilder.WithRichTextType(SocraticAnnotation.RichTextType.DELAY);
                            else if (compareProfileTo.Contains(SocraticAnnotation.shakeTag))
                                newTokenBuilder.WithRichTextType(SocraticAnnotation.RichTextType.SHAKE);
                            else
                                Debug.LogError($"'{compareProfileTo}' -- Parse section did not have a valid input.");

                            var contents = "";

                            for (var c = i; c < f; c++) contents += rawText[c];

                            if (contents.Contains(SocraticAnnotation.parseClosePairChar)) {
                                newTokenBuilder.IsCloser();
                            }

                            if (readingDynamicValue) newTokenBuilder.WithPassedValue(dynamicValue);

                            AnnotationToken newToken = newTokenBuilder.Build();
                            
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
                if (token.IsOpener()) {
                    openingTokens.Add(token);
                }
                else {
                    closingTokens.Add(token);
                }
            }

            foreach (var opener in openingTokens) {
                foreach (var closer in closingTokens) {
                    if (opener.GetRichTextType() == closer.GetRichTextType() && closer.GetLinkedToken() == null) {
                        opener.LinkToken(closer);
                        closer.LinkToken(opener);
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
                if (token.GetStartCharIndex() != token.GetEndCharIndex()) {
                    token.ShiftCharIndices(totalCharactersSnipped);
                    var start = token.GetStartCharIndex();
                    var length = token.GetEndCharIndex() - token.GetStartCharIndex() + 1;
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
                    if (token.GetStartCharIndex() > richTextToken.startIndex) {
                        totalOffset += richTextToken.length + 1;
                    }
                }

                token.ShiftCharIndices(totalOffset);
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
                            AnnotationToken.Builder newTokenBuilder = new();

                            int charIndex = i + 1 - invisibleChars;
                            
                            newTokenBuilder.WithStartCharIndex(charIndex);
                            newTokenBuilder.WithEndCharIndex(charIndex);
                            newTokenBuilder.WithRichTextType(SocraticAnnotation.RichTextType.DELAY);

                            string passedValue = minorDelay
                                ? SocraticAnnotation.minorPunctuationDisplayDelay.ToString()
                                : SocraticAnnotation.majorPunctuationDisplayDelay.ToString();
                            
                            newTokenBuilder.WithPassedValue(passedValue);

                            AnnotationToken newToken = newTokenBuilder.Build();
                            
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