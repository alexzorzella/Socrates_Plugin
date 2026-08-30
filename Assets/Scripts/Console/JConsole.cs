using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class JConsole : MonoBehaviour {
    static JConsole _i;

    static readonly char commandPrefix = '/';

    public TextMeshProUGUI commandOutputText;
    public TMP_InputField inputField;
    public TextMeshProUGUI autocompleteText;

    public TextMeshProUGUI autocompleteOptionsText;

    public RectTransform autocompleteBackgroundRect;

    public int selectedAutocompleteOption;

    public CanvasGroup terminalCanvasGroup;

    [HideInInspector] public bool suppressSystemMessages;

    public CanvasGroup messagesCanvasGroup;
    public RectTransform parentMessagesTo;

    public RectTransform scrollViewport;

    [HideInInspector] public bool visible;

    readonly List<string> autocompleteCommands = new();

    RectTransform canvasRect;

    [HideInInspector] public List<HCommand> commands = new();

    int currentMessages;

    readonly List<string> history = new();
    int historyIndex = -1;

    readonly List<string> logs = new();
    readonly List<JConsoleLogListener> logListeners = new();

    string machineName = "vnix";
    string username = "june";

    static readonly bool pauseTimeWhenActive = false;
    
    public void RegisterListener(JConsoleLogListener newListener) {
        logListeners.Add(newListener);
        newListener.ReceiveBacklog(logs);
    }

    void NotifyListenersOfSystemMessage(string message) {
        foreach (var listener in logListeners) {
            listener.OnSystemMessageLogged(message);
        }
    }
    
    void NotifyListenersOnWriteToConsole(string message) {
        foreach (var listener in logListeners) {
            listener.OnWriteToConsole(message);
        }
    }

    public void DisplaySystemMessage(string message, string nonTruncatedMessage = "") {
        string prefix = message[0] == '[' ? " " : " [System] ";
        string finalMessage = string.IsNullOrEmpty(nonTruncatedMessage) ? message : nonTruncatedMessage;
        string formattedMessage = $"({DateTime.Now}){prefix}{finalMessage}";
        
        WriteLine(formattedMessage);
        
        logs.Add(formattedMessage);
        NotifyListenersOfSystemMessage(formattedMessage);
        
        if (!suppressSystemMessages) {
            var rect = Instantiate(ResourceLoader.LoadObject("SystemMessage"), Vector2.zero, Quaternion.identity)
                .GetComponent<RectTransform>();
            rect.SetParent(parentMessagesTo);

            rect.localPosition = Vector2.zero;
            rect.localScale = Vector2.one;

            rect.gameObject.GetComponent<SystemMessage>().SetText(message);

            UpdateCurrentMessages(1, rect.sizeDelta.y);
        }
    }
    
    public static JConsole i {
        get {
            if (_i == null) {
                var x = Resources.Load<JConsole>("JConsole");

                _i = Instantiate(x);
            }

            return _i;
        }
    }

    void Start() {
        if (_i != null) {
            if (_i != this) Destroy(gameObject);
        } else {
            _i = this;
            DontDestroyOnLoad(gameObject);
        }

        commands.Add(new HcCommandList());
        commands.Add(new HcDebug());
        commands.Add(new HcClearConsole());
        commands.Add(new HcLoadScene());
        commands.Add(new HcDialogue());
        commands.Add(new HcMixerVolume());
        commands.Add(new HcSuppressMessages());
        commands.Add(new HcCloseConsole());
        commands.Add(new HcForceQuit());

        foreach (var command in commands) { autocompleteCommands.Add(command.Keyword()); }
        
        WriteLine($"Successfully loaded {commands.Count} commands");

        canvasRect = GetComponent<RectTransform>();

        UpdateVisuals();
        
        machineName = Environment.MachineName;
        username = Environment.UserName;
        
        ClearConsole();
        // WriteLine("<color=#00E5FF>friday.jam</color> installed");
        WriteLine("<color=#00E5FF>/help</color> for command list");
        WriteLine("<color=yellow>Ctrl + Tab</color> to close console"); 
    }

    void Update() {
        if (_i != null)
            if (_i != this)
                Destroy(gameObject);

        Autocomplete();

        ConsoleFunctionality();
        ScrollHistory();
    }

    public void ClearConsole() {
        commandOutputText.text = "";
    }

    public void UpdateCurrentMessages(int alterBy, float sizeY) {
        currentMessages += alterBy;
        parentMessagesTo.sizeDelta = new Vector2(parentMessagesTo.sizeDelta.x, sizeY * currentMessages);
    }

    void ScrollAutocomplete(int wrapAt) {
        var scrollAmount = 0;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            scrollAmount = -1;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame) scrollAmount = 1;

        if (scrollAmount != 0) {
            IncrementWithOverflow.Run(selectedAutocompleteOption, wrapAt, scrollAmount, out selectedAutocompleteOption);
            inputField.MoveToEndOfLine(false, true);
        }
    }

    string GetLastWord(string input) {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var result = "";

        var words = input.Split(' ');

        result = words[words.Length - 1];

        result = Regex.Replace(result, $"[{commandPrefix}]", "");
        
        return result;
    }

    void ClearAutocompleteOptions() {
        autocompleteOptionsText.text = "";
        autocompleteText.text = "";
            
        autocompleteBackgroundRect.sizeDelta = Vector2.zero;
        selectedAutocompleteOption = -1;
    }
    
    void Autocomplete() {
        if (string.IsNullOrWhiteSpace(inputField.text)) {
            ClearAutocompleteOptions();
            autocompleteText.text = "/";
            
            return;
        }

        if (inputField.text[0] != commandPrefix) {
            ClearAutocompleteOptions();
            return;
        }

        var lastWord = GetLastWord(inputField.text);

        var sourceAutocompleteFrom = new List<string>();

        string[] commands = inputField.text.Split("&&");

        if (commands.Length <= 0) {
            return;
        }
        
        var lastWordIndex = commands[^1].Split(' ').Length;

        if (lastWordIndex == 1) {
            sourceAutocompleteFrom = autocompleteCommands;
        }
        else if (lastWordIndex == 2) {
            List<HCommand> currentCommands = GetCurrentCommands(inputField.text);
            
            if (currentCommands.Count > 0 && currentCommands[^1] != null) {
                sourceAutocompleteFrom = currentCommands[^1].AutocompleteOptions();
            }
        }

        var autocompleteOptions = new List<string>();

        foreach (var option in sourceAutocompleteFrom) {
            if (option.StartsWith(lastWord)) {
                autocompleteOptions.Add(option);
            }
        }

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

        string selectedOption = "";
        
        if(autocompleteOptions.Count > 0) {
            if (selectedAutocompleteOption < 0 || selectedAutocompleteOption > autocompleteOptions.Count - 1) {
                selectedAutocompleteOption = 0;
            }
            
            selectedOption = autocompleteOptions[selectedAutocompleteOption];
        }

        if (!string.IsNullOrWhiteSpace(selectedOption)) {
            finalAutocomplete = inputField.text;
            finalAutocomplete += selectedOption.Substring(lastWord.Length, selectedOption.Length - lastWord.Length);
        }

        var optionsPrompt = "";

        if (autocompleteOptions.Count > 1) {
            for (var i = 0; i < autocompleteOptions.Count; i++) {
                if (i == selectedAutocompleteOption) optionsPrompt += "<color=yellow>";

                optionsPrompt += autocompleteOptions[i];

                if (i == selectedAutocompleteOption) optionsPrompt += "</color>";

                if (i < autocompleteOptions.Count - 1) optionsPrompt += "\n";
            }
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

        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            inputField.text = finalAutocomplete;
            inputField.MoveToEndOfLine(false, true);

            selectedAutocompleteOption = -1;
            autocompleteOptions.Clear();
        }
        
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame) {
            if (Keyboard.current.backspaceKey.wasPressedThisFrame) {
                inputField.text = "";
            }
        }
    }

    void ScrollHistory() {
        if (visible) {
            if (history.Count > 0 && selectedAutocompleteOption < 0) {
                if (Keyboard.current.anyKey.wasPressedThisFrame) {
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
                        ScrollHistoryBy(1);
                    } else if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
                        ScrollHistoryBy(-1);
                    } else {
                        historyIndex = -1;
                    }
                }
            }
        }
    }

    void ScrollHistoryBy(int amount) {
        historyIndex = IncrementWithOverflow.Run(historyIndex, history.Count, amount);
        inputField.text = history[historyIndex];
        inputField.caretPosition = history[historyIndex].Length;
    }

    void ConsoleFunctionality() {
        if (visible) {
            var textSize = commandOutputText.GetRenderedValues(false);
            scrollViewport.sizeDelta = textSize;
        }

        if (SlashKey() && !visible) {
            OpenConsole();
        } else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.tabKey.wasPressedThisFrame) {
            CloseConsole();
        } else if (EscapeKey()) {
            CloseConsole();
        }

        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.vKey.wasPressedThisFrame) {
            bool didHideAll = DebugView.i.ShowAllIfAtLeastOneObjectInactiveOtherwiseHideAll();
            DisplaySystemMessage("Debug view " + (didHideAll ? "off" : "on"));
        }
        
        if (visible && ReturnKey()) TryCommand();
    } 

    void OpenConsole() {
        visible = true;
        UpdateVisuals();
        if (pauseTimeWhenActive) { Time.timeScale = visible ? 0 : 1; }
    }

    public void CloseConsole() {
        visible = false;
        UpdateVisuals();
        SelectInputFieldAndSetText("/");
        if (pauseTimeWhenActive) { Time.timeScale = visible ? 0 : 1; }
    }

    public void UpdateVisuals() {
        terminalCanvasGroup.alpha = visible ? 1 : 0;
        terminalCanvasGroup.interactable = visible;
        terminalCanvasGroup.blocksRaycasts = visible;

        messagesCanvasGroup.alpha = !visible ? 1 : 0;
        messagesCanvasGroup.interactable = !visible;
        messagesCanvasGroup.blocksRaycasts = !visible;

        ClearInputField();
    }

    public static bool Open() {
        return i.visible;
    }

    static bool SlashKey() {
        return Keyboard.current.slashKey.wasPressedThisFrame;
    }

    static bool EscapeKey() {
        return Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    static bool ReturnKey() {
        return Keyboard.current.enterKey.wasPressedThisFrame;
    }

    List<HCommand> GetCurrentCommands(string rawInput) {
        string[] separatedCommands = rawInput.Split("&&");

        List<HCommand> result = new();
        
        foreach (string command in separatedCommands) {
            string[] splitCommand = command.Trim().Split(' ');
            HCommand selectedCommand = Array.Find(commands.ToArray(), c => commandPrefix + c.Keyword() == splitCommand[0]);

            if (selectedCommand != null) {
                result.Add(selectedCommand);
            }
        }

        return result;
    }

    public void SetCommandLineInputText(string newText) {
        inputField.text = newText;
    }

    public bool TryCommand(string overrideCommand = "") {
        string commandInput = !string.IsNullOrWhiteSpace(overrideCommand) ? overrideCommand : inputField.text;

        if (string.IsNullOrWhiteSpace(commandInput)) {
            return false;
        }

        if (commandInput[0] == commandPrefix) {
            List<HCommand> commands = GetCurrentCommands(commandInput);

            history.Add(commandInput);

            if (commands.Count <= 0) {
                WriteLine("Command(s) not recognized.");
                ClearInputField();
                return false;
            }

            string[] commandInputs = commandInput.Split("&&");
            
            for (int i = 0; i < commands.Count; i++) {
                string output = commands[i].CommandFunction(commandInputs[i].Trim().Split(' '));
                WriteLine($"<color=yellow>{output}</color>");
            }
        } else {
            WriteLine("Command not recognized.");
        }

        ClearInputField();

        return true;
    }

    void ClearInputField() {
        inputField.Select();
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    void SelectInputFieldAndSetText(string newContents) {
        inputField.Select();
        inputField.text = newContents;
        inputField.ActivateInputField();
    }

    public void WriteLine(string add) {
        commandOutputText.text += $"\n<color=#00E5FF>{username}@{machineName}</color> <color=yellow>$</color> {add}";
        NotifyListenersOnWriteToConsole(add);
    }
}