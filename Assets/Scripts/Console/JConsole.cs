using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Dialogue_Superclass;

public class JConsole : MonoBehaviour {
    static JConsole _i;

    public static char commandPrefix = '/';

    public TextMeshProUGUI commandOutputText;
    public TMP_InputField inputField;
    public TextMeshProUGUI autocompleteText;
    public TextMeshProUGUI autocompleteOptionsText;
    public RectTransform autocompleteBackgroundRect;

    public int selectedAutocompleteOption;

    public CanvasGroup group;
    public RectTransform scrollViewport;

    [HideInInspector] public bool visible;

    readonly List<string> autocompleteCommands = new();
    RectTransform autocompleteOptionsRect;

    RectTransform canvasRect;

    [HideInInspector] public List<HCommand> commands = new();
    int currentLine;

    readonly List<string> history = new();
    int historyIndex = -1;

    public static JConsole i {
        get {
            if (_i == null) {
                var x = Resources.Load<JConsole>("JConsole");

                _i = Instantiate(x);
            }

            return _i;
        }
    }

    void Awake() {
        if (_i != null)
            if (_i != this)
                Destroy(gameObject);

        _i = this;
        DontDestroyOnLoad(gameObject);

        commands.Add(new HCCommandList());
        commands.Add(new HCTestDialogue());
        commands.Add(new HCLoadScene());
        commands.Add(new HCForceQuit());

        foreach (var command in commands) autocompleteCommands.Add(command.Keyword());

        autocompleteOptionsRect = autocompleteOptionsText.GetComponent<RectTransform>();

        canvasRect = GetComponent<RectTransform>();

        UpdateVisuals();
    }


    void Update() {
        if (_i != null)
            if (_i != this)
                Destroy(gameObject);

        Autocomplete();

        ConsoleFunctionality();
        ScrollHistory();
    }

    void ScrollAutocomplete(int wrapAt) {
        var scrollAmount = 0;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            scrollAmount = -1;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) scrollAmount = 1;

        if (scrollAmount != 0) {
            selectedAutocompleteOption = IncrementWithOverflow.Run(selectedAutocompleteOption, wrapAt, scrollAmount);
            inputField.MoveToEndOfLine(false, true);
        }
    }

    string GetLastWord(string input) {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var result = "";

        var words = input.Split(' ');

        result = words[words.Length - 1];

        result = Regex.Replace(result, "[^A-Za-z0-9_'+.?!]", "");

        return result;
    }

    void Autocomplete() {
        if (string.IsNullOrWhiteSpace(inputField.text)) {
            autocompleteOptionsText.text = "";
            autocompleteText.text = "/";

            autocompleteBackgroundRect.sizeDelta = Vector2.zero;

            selectedAutocompleteOption = -1;
            return;
        }

        var lastWord = GetLastWord(inputField.text);

        var sourceAutocompleteFrom = new List<string>();

        var lastWordIndex = inputField.text.Split(' ').Length;

        if (lastWordIndex == 1)
            sourceAutocompleteFrom = autocompleteCommands;
        else if (lastWordIndex == 2)
            if (GetCurrentCommand() != null)
                sourceAutocompleteFrom = GetCurrentCommand().AutocompleteOptions();

        var autocompleteOptions = new List<string>();

        foreach (var option in sourceAutocompleteFrom)
            if (option.StartsWith(lastWord))
                autocompleteOptions.Add(option);

        if (autocompleteOptions.Count <= 0) {
            autocompleteOptionsText.text = "";
            autocompleteText.text = "/";

            autocompleteBackgroundRect.sizeDelta = Vector2.zero;

            selectedAutocompleteOption = -1;

            return;
        }

        if (selectedAutocompleteOption < 0) selectedAutocompleteOption = 0;

        ScrollAutocomplete(autocompleteOptions.Count);

        var finalAutocomplete = "";

        var selectedOption = autocompleteOptions[selectedAutocompleteOption];

        if (!string.IsNullOrWhiteSpace(selectedOption)) {
            finalAutocomplete = inputField.text;
            finalAutocomplete += selectedOption.Substring(lastWord.Length, selectedOption.Length - lastWord.Length);
        }

        var optionsPrompt = "";

        if (autocompleteOptions.Count > 1)
            for (var i = 0; i < autocompleteOptions.Count; i++) {
                if (i == selectedAutocompleteOption) optionsPrompt += "<color=yellow>";

                optionsPrompt += autocompleteOptions[i];

                if (i == selectedAutocompleteOption) optionsPrompt += "</color>";

                if (i < autocompleteOptions.Count - 1) optionsPrompt += "\n";
            }

        autocompleteOptionsText.text = optionsPrompt;

        autocompleteText.text = finalAutocomplete;

        autocompleteText.ForceMeshUpdate();
        autocompleteOptionsText.ForceMeshUpdate();

        if (!string.IsNullOrWhiteSpace(autocompleteText.text)) {
            var textInfo = autocompleteText.textInfo;

            var indexOfLastWord = inputField.text.Length - lastWord.Length;

            Vector2 promptPosition = textInfo.characterInfo[indexOfLastWord].bottomLeft;

            var worldBottomLeft = autocompleteText.transform.TransformPoint(promptPosition);

            autocompleteBackgroundRect.anchoredPosition =
                new Vector2(worldBottomLeft.x / canvasRect.localScale.x, 145F);

            var textSize = autocompleteOptionsText.GetRenderedValues(false);
            var paddingSize = new Vector2(8, 8);

            autocompleteBackgroundRect.sizeDelta = textSize + paddingSize;
        }
        else {
            autocompleteBackgroundRect.sizeDelta = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            inputField.text = finalAutocomplete;
            inputField.MoveToEndOfLine(false, true);

            selectedAutocompleteOption = -1;
            autocompleteOptions.Clear();
        }

        if (Input.GetKey(KeyCode.LeftShift))
            if (Input.GetKeyDown(KeyCode.Backspace))
                inputField.text = "";
    }

    void ScrollHistory() {
        if (visible)
            if (history.Count > 0 && selectedAutocompleteOption < 0)
                if (Input.anyKeyDown) {
                    if (Input.GetKey(KeyCode.UpArrow))
                        ScrollHistoryBy(1);
                    else if (Input.GetKey(KeyCode.DownArrow))
                        ScrollHistoryBy(-1);
                    else
                        historyIndex = -1;
                }
    }

    void ScrollHistoryBy(int amount) {
        historyIndex = IncrementWithOverflow.Run(historyIndex, history.Count, amount);
        inputField.text = history[historyIndex];
        inputField.caretPosition = history[historyIndex].Length;
    }

    void ConsoleFunctionality() {
        var textSize = commandOutputText.GetRenderedValues(false);

        scrollViewport.sizeDelta = textSize;

        if (SlashKey() && !visible) {
            visible = true;
            UpdateVisuals();
        }
        else if (EscapeKey()) {
            visible = !visible;
            UpdateVisuals();
        }

        Time.timeScale = visible ? 0 : 1;

        if (visible && ReturnKey()) TryCommand();
    }

    public void UpdateVisuals() {
        group.alpha = visible ? 1 : 0;
        group.interactable = visible;
        group.blocksRaycasts = visible;

        ClearInputField();
    }

    public static bool Open() {
        return i.visible;
    }

    static bool SlashKey() {
        return Input.GetKeyDown(KeyCode.Slash);
    }

    static bool EscapeKey() {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    static bool ReturnKey() {
        return Input.GetKeyDown(KeyCode.Return);
    }

    public HCommand GetCurrentCommand() {
        var input = inputField.text;

        var split = input.Split(' ');
        var selectedCommand = Array.Find(commands.ToArray(), c => commandPrefix + c.Keyword() == split[0]);

        return selectedCommand;
    }

    public void TryCommand() {
        var input = inputField.text;

        if (string.IsNullOrEmpty(inputField.text)) return;

        if (input[0] == commandPrefix) {
            var selectedCommand = GetCurrentCommand();

            history.Add(input);

            if (selectedCommand == null) {
                WriteLine("Command not recognized.");
                ClearInputField();
                return;
            }

            WriteLine($"<color=yellow>{selectedCommand.CommandFunction(input.Split(' '))}</color>");
        }
        else {
            WriteLine("Command not recognized.");
        }

        ClearInputField();
    }

    void ClearInputField() {
        inputField.Select();
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void WriteLine(string add) {
        commandOutputText.text += $"\n> {add}";
        currentLine++;
    }

    public DialogueManager GetDialogueManager() {
        return FindObjectOfType<DialogueManager>();
    }
}

public interface HCommand {
    string Keyword();
    string CommandHelp();
    List<string> AutocompleteOptions();
    string CommandFunction(params string[] parameters);
}

public class ConsoleUtility {
    public static string ParameterParse(string[] parameters) {
        var result = "";

        for (var i = 1; i < parameters.Length; i++)
            if (parameters.Length < 2)
                result += parameters[i];
            else if (i < parameters.Length - 1)
                result += parameters[i] + " ";
            else
                result += parameters[i];

        return result;
    }
}

public class HCCommandList : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        foreach (var command in JConsole.i.commands)
            JConsole.i.WriteLine($"{command.Keyword()} {command.CommandHelp()}");

        return $"Listed {JConsole.i.commands.Count} commands.";
    }

    public string CommandHelp() {
        return "";
    }

    public string Keyword() {
        return "help";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}

public class HCTestDialogue : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        JConsole.i.GetDialogueManager().StartDialogue(Dialogue());
        JConsole.i.visible = false;
        JConsole.i.UpdateVisuals();

        return "Testing dialogue...";
    }

    public string Keyword() {
        return "testDialogue";
    }

    public string CommandHelp() {
        return "";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }

    public DialogueSection Dialogue() {
        var localName = "Alex";
        var sound = "dialogue";

        var l = new Monologue(localName,
            "This [color,yellow]dialogue system[!color] and [color,yellow]text interpreter[!color] took [wave]years[!wave] of updating to get here, thanks to my dad.",
            sound);
        var delay = new Monologue(localName,
            "This, well, this is supposed to test the delay. Really? I think it worked!", sound, l);
        var color = new Monologue(localName, "This is testing whether the [color,yellow]colorful text[!color] works.",
            sound, delay);
        var shake = new Monologue(localName, "This is testing whether the [shake,7]shaky text works[!shake].", sound,
            color);
        var wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        var basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        var loopQuestion = new Choices(localName, "Now, try again?", sound,
            ChoiceList(Choice("Yes", basic), Choice("No", null)));
        l.next = loopQuestion;

        return basic;
    }
}

public class HCLoadScene : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        var sceneExists = SceneManager.GetSceneByName(parameters[1]) != null;

        if (sceneExists)
            SceneManager.LoadScene(parameters[1]);

        return sceneExists ? $"Loading {parameters[1]}..." : $"{parameters[1]} doesn't exist.";
    }

    public string CommandHelp() {
        return "(string sceneName)";
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

public class HCForceQuit : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        Application.Quit();

        return "Quitting...";
    }

    public string CommandHelp() {
        return "";
    }

    public string Keyword() {
        return "quit";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}