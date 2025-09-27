using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow {
    void OnGUI() {
        GUILayout.TextField(
            "Socrates Plugin\n" +
            "(c) 2021-2023 Alex Zorzella, All Rights Reserved\n" +
            "In association with Luiz-Otàvio Zorzella\n");

        GUILayout.Box(GameAssets.i.Socrates, GUILayout.Width(50), GUILayout.Height(90));
    }

    [MenuItem("Socrates Plugin/About")]
    public static void ShowWindow() {
        GetWindow<DescriptionWindow>("About");
    }
}