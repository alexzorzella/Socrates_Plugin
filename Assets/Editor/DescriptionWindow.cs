using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow
{
    [MenuItem("Socrates Plugin/About")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DescriptionWindow>("About");
    }

    private void OnGUI()
    {
        GUILayout.TextField(
            $"Socrates Plugin\n" +
            $"(c) 2021 Alex Zorzella\n" +
            $"Rights reserved to all content but '2D Extras' and 'TextMeshPro'\n" +
            $"Bug fixes done by Luiz Zorzella\n" +
            $"Dedicated to Socrates (2004-2019) / Your legacy will live on.");
        GUILayout.Box(GameAssets.i.Socrates, GUILayout.Width(50), GUILayout.Height(90));
    }
}