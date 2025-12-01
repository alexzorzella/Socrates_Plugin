using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow {
    void OnGUI() {
        GUILayout.TextField(
            "Socrates Plugin\n" +
            "(c) 2021-2025 Alex Zorzella in association with Luiz-Otàvio Zorzella, All Rights Reserved");

        Sprite socrates = ResourceLoader.LoadSprite("socrates");

        if (socrates != null) {
            GUILayout.Box(socrates.texture, GUILayout.Width(50), GUILayout.Height(90));
        }
    }

    [MenuItem("Socrates Plugin/About")]
    public static void ShowWindow() {
        GetWindow<DescriptionWindow>("About");
    }
}