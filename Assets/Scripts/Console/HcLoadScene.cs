using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class HcLoadScene : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        var sceneExists = SceneManager.GetSceneByName(parameters[1]) != null;

        if (sceneExists) {
            NATransition.i.LoadScene(parameters[1]);
            JConsole.i.CloseConsole();
        }

        return sceneExists ? $"Loading {parameters[1]}..." : $"{parameters[1]} doesn't exist.";
    }

    public string CommandHelp() {
        return "(string sceneName), Loads the passed scene";
    }

    public string Keyword() {
        return "loadScene";
    }

    public List<string> AutocompleteOptions() {
        if (options.Count <= 0) {
            var sceneCount = SceneManager.sceneCountInBuildSettings;

            for (var i = 0; i < sceneCount; i++)
                options.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
        }

        return options;
    }
}