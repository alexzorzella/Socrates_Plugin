using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocraticAnnotation
{
    public static char parse_startParse = '[';
    public static char parse_endParse = ']';
    public static char parse_endParsePair = '!';
    public static char parse_inputValueIndicator = ',';

    public static float display_textDelay = 0.012F;

    public static float display_minorPunctuationDelay = 0.15F;
    public static float display_majorPunctuationDelay = 0.25F;

    public static bool wave_warpTextVerticies = true;
    public static string wave_parseInfo = "wave";
    public static float wave_freqMultiplier = .025F;
    public static float wave_amplitude = 7F;
    public static float wave_speed = 9F;

    public static string delay_parseInfo = "delay";

    public static string color_parseInfo = "color";

    public static string shake_parseInfo = "shake";

    public static string italic_parseInfo = "italic";

    public enum RichTextType { WAVE, COLOR, DELAY, SHAKE, ITALIC }
}

public class SocraticAnnotationParse
{
    public int startCharacterLocation = -1;
    public int endCharacterLocation = -1;
    public SocraticAnnotation.RichTextType richTextType;
    public bool openingParse = true;
    public SocraticAnnotationParse linkedParse;
    public string dynamicValue = "";
    public bool executedAction = false;

    public bool ContainsDynamicValue()
    {
        return !string.IsNullOrEmpty(dynamicValue);
    }

    public float GetDynamicValueAsFloat()
    {
        float result;
        result = float.Parse(dynamicValue);
        return result;
    }

    public Color GetDynamicValueAsColor(float alpha)
    {
        Color result = Color.black;
        string compare = dynamicValue.ToLower();

        if (compare == "red")
        {
            result = Color.red;
        } 
        else if(compare == "orange")
        {
            result = Color.red + Color.yellow;
        }
        else if (compare == "yellow")
        {
            result = Color.yellow;
        }
        else if (compare == "green")
        {
            result = Color.green;
        }
        else if (compare == "blue")
        {
            result = Color.blue;
        }
        else if (compare == "purple")
        {
            result = Color.magenta + Color.blue;
        }
        else
        {
            Color colorFromHexCode = Color.black;
            ColorUtility.TryParseHtmlString(compare, out colorFromHexCode);
            result = colorFromHexCode;
        }

        return new Color(result.r, result.g, result.b, alpha);
    }
}