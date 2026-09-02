using UnityEditor;
using UnityEngine;

public class DescriptionWindow : EditorWindow {
    void OnGUI() {
        GUILayout.Label("Socrates Plugin");
        GUILayout.Label("For Socrates, from Alex Zorzella and 'Z' Zorzella.");
        
        if (GUILayout.Button("Documentation")) {
            Application.OpenURL("https://guides.highqualitybackup.com/socrates_plugin/about/");
        }
        
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