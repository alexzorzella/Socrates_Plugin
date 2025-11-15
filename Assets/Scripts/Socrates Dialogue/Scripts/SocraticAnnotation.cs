using UnityEngine;

public static class SocraticAnnotation {
    public enum RichTextType {
        WAVE,
        COLOR,
        DELAY,
        SHAKE,
        ITALIC
    }

    public static readonly char parseStartParse = '[';
    public static readonly char parseEndParse = ']';
    public static readonly char parseEndParsePair = '!';
    public static readonly char parseInputValueIndicator = ',';

    public static readonly float displayTextDelay = 0.01F;

    public static readonly float displayMinorPunctuationDelay = 0.25F;
    public static readonly float displayMajorPunctuationDelay = 0.35F;

    public static readonly bool waveWarpTextVertices = true;
    public static readonly string waveParseInfo = "wave";
    public static readonly float waveFreqMultiplier = 0.025F;
    public static readonly float waveAmplitude = 7F;
    public static readonly float waveSpeed = 9F;

    public static readonly string delayParseInfo = "delay";

    public static readonly string colorParseInfo = "color";

    public static readonly string shakeParseInfo = "shake";

    public static readonly string italicParseInfo = "italic";
}

public class SocraticAnnotationParse {
    public string dynamicValue = "";
    public int endCharacterLocation = -1;
    public bool executedAction = false;
    public SocraticAnnotationParse linkedParse;
    public bool openingParse = true;
    public SocraticAnnotation.RichTextType richTextType;
    public int startCharacterLocation = -1;

    public bool ContainsDynamicValue() {
        return !string.IsNullOrEmpty(dynamicValue);
    }

    public float GetDynamicValueAsFloat() {
        float result;
        result = float.Parse(dynamicValue);
        return result;
    }

    public Color GetDynamicValueAsColor(float alpha) {
        var result = Color.black;
        var compare = dynamicValue.ToLower();

        if (compare == "red") {
            result = Color.red;
        } else if (compare == "orange") {
            result = Color.red + Color.yellow;
        } else if (compare == "yellow") {
            result = Color.yellow;
        } else if (compare == "green") {
            result = Color.green;
        } else if (compare == "blue") {
            result = Color.blue;
        } else if (compare == "purple") {
            result = Color.magenta + Color.blue;
        } else {
            var colorFromHexCode = Color.black;
            ColorUtility.TryParseHtmlString(compare, out colorFromHexCode);
            result = colorFromHexCode;
        }

        return new Color(result.r, result.g, result.b, alpha);
    }
}