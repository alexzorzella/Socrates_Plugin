using UnityEngine;

public static class SocraticAnnotation {
    public enum RichTextType {
        WAVE,
        DELAY,
        SHAKE
    }

    public static readonly char parseStartChar = '[';
    public static readonly char parseEndChar = ']';
    public static readonly char parseEndParsePair = '!';
    public static readonly char parseInputValueSeparator = ',';

    public static readonly float displayTextDelay = 0.01F;

    public static readonly float displayMinorPunctuationDelay = 0.25F;
    public static readonly float displayMajorPunctuationDelay = 0.35F;

    public static readonly bool waveWarpTextVertices = true;
    public static readonly string waveParseInfo = "wave";
    public static readonly float waveFreqMultiplier = 0.025F;
    public static readonly float waveAmplitude = 7F;
    public static readonly float waveSpeed = 9F;

    public static readonly string delayParseInfo = "delay";

    public static readonly string shakeParseInfo = "shake";
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
        float result;
        result = float.Parse(passedValue);
        return result;
    }

    public Color GetDynamicValueAsColor(float alpha) {
        var result = Color.black;
        var compare = passedValue.ToLower();

        if (compare == "red") {
            result = Color.red;
        }
        else if (compare == "orange") {
            result = Color.red + Color.yellow;
        }
        else if (compare == "yellow") {
            result = Color.yellow;
        }
        else if (compare == "green") {
            result = Color.green;
        }
        else if (compare == "blue") {
            result = Color.blue;
        }
        else if (compare == "purple") {
            result = Color.magenta + Color.blue;
        }
        else {
            var colorFromHexCode = Color.black;
            ColorUtility.TryParseHtmlString(compare, out colorFromHexCode);
            result = colorFromHexCode;
        }

        return new Color(result.r, result.g, result.b, alpha);
    }
}