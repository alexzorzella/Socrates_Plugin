using UnityEditor;
using UnityEngine;

public class SocratesInfo : EditorWindow
{
    [MenuItem("Socrates Workflow/_About")]
    static void ShowWindow()
    {
        GetWindow<SocratesInfo>("About Socrates Workflow");
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;

        GUILayout.Box("Socrates Workflow Unity plugin by Alex Zorzella.\nYou are on version 1.0.0\nSocrates auto imports " +
            "\nessential scripts into your project such as an audio manager, script for saving, " +
            "\nand other very useful features. It also auto imports with the Unity 2D Extras which are not accesible from " +
            "\nthe package manager or Unity Store Currently. This is subject to change.");
        GUILayout.Box("----------------" +
            "\nSocrates Workflow Features:" +
            "\n Audio Manager :: Instant sound effects and music from any scene." +
            "\n Camera Movement :: Smooth, Snappy, Predictive, and Scrolling all come bundled." +
            "\n Save System :: Save your game with serialized binary and keep data locked up." +
            "\n Game Assets :: Import game assets from anywhere in your project with a static class." +
            "\n Basic Controllers :: Make player movement fast with the imported player controllers." +
            "\n Shapes :: Pick from a variety of default sprites to prototype." +
            "\n Essential Math :: Math that just doesn't come with Unity for some reason." +
            "\n Mixers :: No hassle setting up mixers for your game. Easy as 1, 2, 3!" +
            "\n Parallax :: Very specific, but adds punch to your backgrounds with scrolling." +
            "\n Fonts :: Stop searching endlessly and just pick from a variety of fonts!" +
            "\n Dialogue System :: Very specific, but adds character to your game when needed." +
            "\n--" +
            "\n Thanks for using Socrates Workflow, Alex.");
    }
}