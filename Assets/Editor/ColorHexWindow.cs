using UnityEditor;
using UnityEngine;

public class ColorHexWindow : EditorWindow {
    Color inputColor;
    string outputHexCode;

    void OnGUI() {
        inputColor = EditorGUILayout.ColorField("Input Color", inputColor);
        outputHexCode = ColorUtility.ToHtmlStringRGB(inputColor);
        GUILayout.TextArea($"#{outputHexCode}");
    }

    [MenuItem("Socrates Plugin/Color Code")]
    public static void ShowWindow() {
        GetWindow<ColorHexWindow>("Hex Code Converter");
    }
}