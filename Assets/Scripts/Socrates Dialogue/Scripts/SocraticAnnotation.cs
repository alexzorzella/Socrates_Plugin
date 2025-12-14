namespace SocratesDialogue {
    public static class SocraticAnnotation {
        public enum RichTextType {
            WAVE,
            DELAY,
            SHAKE
        }

        public const char parseStartChar = '[';
        public const char parseEndChar = ']';
        public const char parseClosePairChar = '!';
        public const char parseValueSeparator = ',';

        public const float displayDelayPerChar = 0.1F;

        public const float minorPunctuationDisplayDelay = 0.25F;
        public const float majorPunctuationDisplayDelay = 0.35F;

        public const string waveTag = "wave";
        public const bool waveWarpTextVertices = true;
        public const float waveFreqMultiplier = 0.025F;
        public const float waveAmplitude = 7F;
        public const float waveSpeed = 9F;

        public const string delayTag = "delay";

        public const string shakeTag = "shake";

        public const char richTextStart = '<';
        public const char richTextEnd = '>';

        public static readonly char[] minorPunctuation = { ',', '–' };
        public static readonly char[] majorPunctuation = { '.', '?', '!', '~', ':', ':', '(', ')', ';', '—' };
    }
}