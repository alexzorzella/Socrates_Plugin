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

    public CanvasGroup messagesCanvasGroup;
    public RectTransform parentSystemMessagesTo;

    public RectTransform scrollViewport;

    readonly List<string> autocompleteCommands = new();

    RectTransform canvasRect;

    bool suppressSystemMessages;
    bool visible;
    
    readonly List<HCommand> commands = new();
    public List<HCommand> GetCommands() { return commands; }

    int currentMessages;

    readonly List<string> history = new();
    int historyIndex = -1;

    readonly List<string> logs = new();
    readonly List<JConsoleLogListener> logListeners = new();

    string machineName = "vnix";
    string username = "june";

    static readonly bool pauseTimeWhenActive = false;
    
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
            if (_i != this) { Destroy(gameObject); }
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

        foreach (var command in commands) {
            autocompleteCommands.Add(command.Keyword());
        }
        
        Print($"Successfully loaded {commands.Count} commands");

        canvasRect = GetComponent<RectTransform>();

        UpdateVisuals();
        
        machineName = Environment.MachineName;
        username = Environment.UserName;
        
        ClearConsole();
        
        Print("<color=#00E5FF>/help</color> for command list");
        Print("<color=yellow>Ctrl + Tab</color> to close console"); 
    }

    /// <summary>
    /// Sets the visibility of the console to the passed visibility.
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisible(bool visible) {
        this.visible = visible;
        UpdateVisuals();
    }

    public bool IsVisible() { return visible; }

    /// <summary>
    /// Toggles whether system messages are suppressed and returns the new value.
    /// </summary>
    /// <returns></returns>
    public bool ToggleSuppressSystemMessages() {
        suppressSystemMessages = !suppressSystemMessages;
        return suppressSystemMessages;
    }
   
    /// <summary>
    /// Registers a JConsoleLogListener to the console.
    /// </summary>
    /// <param name="newListener"></param>
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

    /// <summary>
    /// Displays the passed system message and prints the non-truncated message to the console.
    /// If the no non-truncated message is passed, the original message is printed to the console.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="nonTruncatedMessage"></param>
    public void DisplaySystemMessage(string message, string nonTruncatedMessage = "") {
        string prefix = message[0] == '[' ? " " : " [System] ";
        string finalMessage = string.IsNullOrEmpty(nonTruncatedMessage) ? message : nonTruncatedMessage;
        string formattedMessage = $"({DateTime.Now}){prefix}{finalMessage}";
        
        Print(formattedMessage);
        
        logs.Add(formattedMessage);
        NotifyListenersOfSystemMessage(formattedMessage);
        
        if (!suppressSystemMessages) {
            var rect = Instantiate(ResourceLoader.LoadObject("SystemMessage"), Vector2.zero, Quaternion.identity)
                .GetComponent<RectTransform>();
            rect.SetParent(parentSystemMessagesTo);

            rect.localPosition = Vector2.zero;
            rect.localScale = Vector2.one;

            rect.gameObject.GetComponent<SystemMessage>().SetText(message);

            UpdateSystemMessageCount(1, rect.sizeDelta.y);
        }
    }
   
    /// <summary>
    /// Clears the console.
    /// </summary>
    public void ClearConsole() {
        commandOutputText.text = "";
    }

    /// <summary>
    /// Updates the system message count and parent size
    /// </summary>
    /// <param name="alterBy"></param>
    /// <param name="sizeY"></param>
    public void UpdateSystemMessageCount(int alterBy, float sizeY) {
        currentMessages += alterBy;
        parentSystemMessagesTo.sizeDelta = new Vector2(parentSystemMessagesTo.sizeDelta.x, sizeY * currentMessages);
    }

    /// <summary>
    /// Checks if the up or down arrow keys have been pressed, and then scrolls the currently selected autocomplete option
    /// up or down
    /// </summary>
    /// <param name="autocompleteOptions"></param>
    string ScrollAutocomplete(List<string> autocompleteOptions) {
        var scrollAmount = 0;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
            scrollAmount = -1;
        } else if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
            scrollAmount = 1;
        }

        if (autocompleteOptions.Count > 0) {
            if (scrollAmount != 0) {
                int wrapAt = autocompleteOptions.Count;
                IncrementWithOverflow.Run(selectedAutocompleteOption, wrapAt, scrollAmount, out selectedAutocompleteOption);
                inputField.MoveToEndOfLine(false, true);
            }

            if (selectedAutocompleteOption < 0 || selectedAutocompleteOption > autocompleteOptions.Count - 1) {
                selectedAutocompleteOption = 0;
            }

            return autocompleteOptions[selectedAutocompleteOption];
        }

        if (selectedAutocompleteOption > autocompleteOptions.Count - 1) {
            selectedAutocompleteOption = 0;
        }

        return "";
    }

    string GetLastWord(string input) {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var result = "";

        var words = input.Split(' ');

        result = words[^1];

        result = Regex.Replace(result, $"[{commandPrefix}]", "");
        
        return result;
    }

    void ClearAutocompleteOptions() {
        autocompleteOptionsText.text = "";
        autocompleteText.text = "";
            
        autocompleteBackgroundRect.sizeDelta = Vector2.zero;
        selectedAutocompleteOption = -1;
    }
    
    void Update() {
        Autocomplete();
        ConsoleFunctionality();
        ScrollHistory();
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

        List<string> autocompleteOptions = new();

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
        
        string selectedOption = ScrollAutocomplete(autocompleteOptions);

        if (selectedOption == "") {
            return;
        }
        
        string finalAutocomplete = "";
        
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
        if (visible && history.Count > 0 && selectedAutocompleteOption < 0 && Keyboard.current.anyKey.wasPressedThisFrame) {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) {
                ScrollHistoryBy(1);
            } else if (Keyboard.current.downArrowKey.wasPressedThisFrame) {
                ScrollHistoryBy(-1);
            } else {
                historyIndex = -1;
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
    
    /// <summary>
    /// Opens the console.
    /// </summary>
    public void OpenConsole() {
        SetVisible(true);
        if (pauseTimeWhenActive) { Time.timeScale = visible ? 0 : 1; }
    }

    /// <summary>
    /// Closes the console.
    /// </summary>
    public void CloseConsole() {
        SetVisible(false);
        SelectInputFieldAndSetText("/");
        if (pauseTimeWhenActive) { Time.timeScale = visible ? 0 : 1; }
    }

    /// <summary>
    /// Updates the canvas groups to be opaque, be interactable, and to block raycasts
    /// according to whether the console is visible or not.
    /// </summary>
    public void UpdateVisuals() {
        terminalCanvasGroup.alpha = visible ? 1 : 0;
        terminalCanvasGroup.interactable = visible;
        terminalCanvasGroup.blocksRaycasts = visible;

        messagesCanvasGroup.alpha = !visible ? 1 : 0;
        messagesCanvasGroup.interactable = !visible;
        messagesCanvasGroup.blocksRaycasts = !visible;

        ClearInputField();
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

    bool TryCommand(string overrideCommand = "") {
        string commandInput = !string.IsNullOrWhiteSpace(overrideCommand) ? overrideCommand : inputField.text;

        if (string.IsNullOrWhiteSpace(commandInput)) {
            return false;
        }

        if (commandInput[0] == commandPrefix) {
            List<HCommand> commands = GetCurrentCommands(commandInput);

            history.Add(commandInput);

            if (commands.Count <= 0) {
                Print("Command(s) not recognized.");
                ClearInputField();
                return false;
            }

            string[] commandInputs = commandInput.Split("&&");
            
            for (int i = 0; i < commands.Count; i++) {
                string output = commands[i].CommandFunction(commandInputs[i].Trim().Split(' '));
                Print($"<color=yellow>{output}</color>");
            }
        } else {
            Print("Command not recognized.");
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

    /// <summary>
    /// Prints a line to the console.
    /// </summary>
    /// <param name="content"></param>
    public void Print(string content) {
        commandOutputText.text += $"\n<color=#00E5FF>{username}@{machineName}</color> <color=yellow>$</color> {content}";
        NotifyListenersOnWriteToConsole(content);
    }
}