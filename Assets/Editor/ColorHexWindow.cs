using UnityEngine;
using UnityEditor;

public class ColorHexWindow : EditorWindow
{
    Color inputColor;
    string outputHexCode;

    [MenuItem("Socrates Plugin/Color Code")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<ColorHexWindow>("Hex Code Converter");
    }

    private void OnGUI()
    {
        inputColor = EditorGUILayout.ColorField("Input Color", inputColor);
        outputHexCode = ColorUtility.ToHtmlStringRGB(inputColor);
        GUILayout.TextArea($"#{outputHexCode}");
    }
}