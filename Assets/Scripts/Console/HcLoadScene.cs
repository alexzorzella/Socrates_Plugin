using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class HcLoadScene : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        if (parameters.Length < 2) {
            return "Please specify a scene name";
        }
        
        bool sceneExists = SceneManager.GetSceneByName(parameters[1]) != null;

        if (sceneExists) {
            GnaTransition.LoadScene(parameters[1]);
            JConsole.i.CloseConsole();
            return $"Loading {parameters[1]}...";
        }

        return $"{parameters[1]} doesn't exist.";
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

            for (var i = 0; i < sceneCount; i++) {
                options.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            }
        }

        return options;
    }
}