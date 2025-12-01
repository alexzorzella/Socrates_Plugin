public static class SocraticAnnotation {
    public enum RichTextType {
        WAVE,
        DELAY,
        SHAKE
    }

    public static readonly char parseStartChar = '[';
    public static readonly char parseEndChar = ']';
    public static readonly char parseClosePairChar = '!';
    public static readonly char parseValueSeparator = ',';

    public static readonly float displayTextDelay = 0.01F;

    public static readonly float displayMinorPunctuationDelay = 0.25F;
    public static readonly float displayMajorPunctuationDelay = 0.35F;

    public static readonly string waveTag = "wave";
    public static readonly bool waveWarpTextVertices = true;
    public static readonly float waveFreqMultiplier = 0.025F;
    public static readonly float waveAmplitude = 7F;
    public static readonly float waveSpeed = 9F;

    public static readonly string delayTag = "delay";
    
    public static readonly string shakeTag = "shake";
}

public class AnnotationToken {
    public string passedValue = "";
    public int startCharIndex = -1;
    public int endCharIndex = -1;
    public bool executedAction = false;
    public AnnotationToken linkedToken;
    public bool opener = true;
    public SocraticAnnotation.RichTextType richTextType;

    public bool ContainsDynamicValue() {
        return !string.IsNullOrEmpty(passedValue);
    }

    public float GetDynamicValueAsFloat() {
        float result = 0;
        result = float.Parse(passedValue);
        return result;
    }
}