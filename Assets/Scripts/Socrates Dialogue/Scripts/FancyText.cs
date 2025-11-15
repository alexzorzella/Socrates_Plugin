using System.Collections.Generic;
using UnityEngine;

public class FancyText {
    readonly string rawText;
    string cleanedText; // TODO: How do I make this readonly?
    readonly List<AnnotationToken> annotationTokens = new();

    public FancyText(string rawText) {
        this.rawText = rawText;

        Parse();
        LinkTokens();
        CleanText();
    }

    void LinkTokens() {
        List<AnnotationToken> openingTokens = new();
        List<AnnotationToken> closingTokens = new();

        foreach (var token in annotationTokens) {
            if (token.opener) {
                openingTokens.Add(token);
            } else {
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

    void CleanText() {
        cleanedText = rawText;

        RealignAnnotationTokenIndices();
        DiceAndDevourRichTextTokens();
        
        AnnotateByPunctuation();
    }

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

    void DiceAndDevourRichTextTokens() {
        List<RichTextToken> cache = DiceRichTextTokens();
        
        DevourRichTextTokens(cache);
    }

    List<RichTextToken> DiceRichTextTokens() {
        List<RichTextToken> result = new();
            
        for (int i = 0; i < cleanedText.Length; i++) {
            if (cleanedText[i] == '<') {
                result.Add(new RichTextToken(i));
            } else if (cleanedText[i] == '>') {
                result[^1].length = i - result[^1].startIndex;
            }
        }

        return result;
    }
    
    void DevourRichTextTokens(List<RichTextToken> richTextTokens) {
        foreach (var token in annotationTokens) {
            int totalOffset = 0;
            
            foreach (var richTextToken in richTextTokens) {
                if (token.startCharIndex > richTextToken.startIndex) {
                    totalOffset += richTextToken.length;
                }
            }

            token.startCharIndex -= totalOffset;
            token.endCharIndex -= totalOffset;
        }
    }

    void Parse(int startAt = 0) {
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

                        Parse(f);
                        return;
                    }

                    compareProfileTo += rawText[f];

                    if (readingDynamicValue) dynamicValue += rawText[f];

                    if (rawText[f] == SocraticAnnotation.parseValueSeparator) readingDynamicValue = true;
                }
            }
        }
    }

    void AnnotateByPunctuation(int startAt = 0) {
        for (var i = startAt; i < cleanedText.Length; i++) {
            if (char.IsPunctuation(cleanedText[i]) && i < cleanedText.Length - 1) {
                var c = cleanedText[i];

                if (cleanedText[i + 1] == ' ') {
                    if (c == ',' ||
                        c == '.' ||
                        c == '?' ||
                        c == '!' ||
                        c == '~' ||
                        c == ':' ||
                        c == ':' ||
                        c == '(' ||
                        c == ')' ||
                        c == ';') {
                        AnnotationToken newToken = new();

                        newToken.startCharIndex = i + 1;
                        newToken.endCharIndex = i + 1;
                        newToken.richTextType = SocraticAnnotation.RichTextType.DELAY;
                        newToken.passedValue = c == ','
                            ? $"{SocraticAnnotation.displayMinorPunctuationDelay}"
                            : $"{SocraticAnnotation.displayMajorPunctuationDelay}";

                        annotationTokens.Add(newToken);
                    }
                }
            }
        }
    }

    public override string ToString() {
        return cleanedText;
    }

    public List<AnnotationToken> GetAnnotationTokens() {
        return annotationTokens;
    }
}