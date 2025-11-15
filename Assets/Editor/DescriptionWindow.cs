using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow {
    void OnGUI() {
        GUILayout.TextField(
            "Socrates Plugin\n" +
            "(c) 2021-2025 Alex Zorzella in association with Luiz-Otàvio Zorzella, All Rights Reserved");

        GUILayout.Box(ResourceLoader.i.Socrates, GUILayout.Width(50), GUILayout.Height(90));
    }

    [MenuItem("Socrates Plugin/About")]
    public static void ShowWindow() {
        GetWindow<DescriptionWindow>("About");
    }
}