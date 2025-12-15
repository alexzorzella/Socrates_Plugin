using System.Collections.Generic;

namespace SocratesDialogue {
    public static class SocraticAnnotation {
        public enum RichTextType {
            WAVE,
            DELAY,
            SHAKE,
            // GRADIENT,
            // FUNCTION,
            // SOUND
        }

        public const char parseStartChar = '[';
        public const char parseEndChar = ']';
        public const char parseClosePairChar = '!';
        public const char parseValueSeparator = ',';

        public const float displayDelayPerChar = 0.01F;

        public const float minorPunctuationDisplayDelay = 0.25F;
        public const float majorPunctuationDisplayDelay = 0.35F;

        public const bool waveWarpTextVertices = true;
        public const float waveFreqMultiplier = 0.025F;
        public const float waveAmplitude = 7F;
        public const float waveSpeed = 9F;

        public static readonly Dictionary<string, RichTextType> annotationTags = new() {
            { "wave", RichTextType.WAVE },
            { "delay", RichTextType.DELAY },
            { "shake", RichTextType.SHAKE },
            // { "gradient", RichTextType.GRADIENT },
            // { "notify", RichTextType.FUNCTION },
            // { "sound", RichTextType.SOUND },
        };

        public const char richTextStart = '<';
        public const char richTextEnd = '>';

        public static readonly char[] minorPunctuation = { ',', '–' };
        public static readonly char[] majorPunctuation = { '.', '?', '!', '~', ':', ':', '(', ')', ';', '—' };
    }
}