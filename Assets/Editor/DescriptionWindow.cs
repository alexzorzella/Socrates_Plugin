using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow {
    void OnGUI() {
        GUILayout.TextField(
            "Socrates Plugin\n" +
            "For Socrates, from Alex Zorzella and 'Z' Zorzella. Please read the readme for more information.");

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