using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NewSocratesDialogue;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
// using static DialogueSuperclass;

public class JConsole : MonoBehaviour {
    static JConsole _i;

    public static char commandPrefix = '/';

    public TextMeshProUGUI commandOutputText;
    public TMP_InputField inputField;
    public TextMeshProUGUI autocompleteText;

    public TextMeshProUGUI autocompleteOptionsText;

    // RectTransform autocompleteOptionsRect;
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

    public void RegisterListener(JConsoleLogListener newListener) {
        logListeners.Add(newListener);
        newListener.RecieveBacklog(logs);
    }

    public void NotifyListeners(string message) {
        foreach (var listener in logListeners) {
            listener.OnSystemMessageLogged(message);
        }
    }
    
    public void LogSystemMessage(string message, string nonTruncatedMessage = "") {
        string prefix = message[0] == '[' ? " " : " [System] ";
        string finalMessage = string.IsNullOrEmpty(nonTruncatedMessage) ? message : nonTruncatedMessage;
        string formattedMessage = $"({DateTime.Now}){prefix}{finalMessage}";
        
        WriteLine(formattedMessage);
        
        logs.Add(formattedMessage);
        NotifyListeners(formattedMessage);

        var rect = Instantiate(ResourceLoader.LoadObject("SystemMessage"), Vector2.zero, Quaternion.identity)
            .GetComponent<RectTransform>();
        rect.SetParent(parentMessagesTo);

        rect.localPosition = Vector2.zero;
        rect.localScale = Vector2.one;

        rect.gameObject.GetComponent<SystemMessage>().SetText(message);

        UpdateCurrentMessages(1, rect.sizeDelta.y);
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
        commands.Add(new HcClearConsole());
        commands.Add(new HcLoadScene());
        commands.Add(new HcTestDialogue());
        commands.Add(new HcSuppressMessages());
        commands.Add(new HcCloseConsole());
        commands.Add(new HcForceQuit());

        foreach (var command in commands) autocompleteCommands.Add(command.Keyword());

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
        } else {
            autocompleteBackgroundRect.sizeDelta = Vector2.zero;
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            inputField.text = finalAutocomplete;
            inputField.MoveToEndOfLine(false, true);

            selectedAutocompleteOption = -1;
            autocompleteOptions.Clear();
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
                inputField.text = "";
    }

    void ScrollHistory() {
        if (visible)
            if (history.Count > 0 && selectedAutocompleteOption < 0)
                if (Keyboard.current.anyKey.wasPressedThisFrame) {
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                        ScrollHistoryBy(1);
                    else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
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
        if (visible) {
            var textSize = commandOutputText.GetRenderedValues(false);
            scrollViewport.sizeDelta = textSize;
        }

        if (SlashKey() && !visible)
            OpenConsole();
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.tabKey.wasPressedThisFrame)
            CloseConsole();
        else if (EscapeKey()) CloseConsole();

        // Time.timeScale = visible ? 0 : 1;

        if (visible && ReturnKey()) TryCommand();
    }

    void OpenConsole() {
        visible = true;
        UpdateVisuals();
        
    }

    public void CloseConsole() {
        visible = false;
        UpdateVisuals();
        SelectInputFieldAndSetText("/");
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
        } else {
            WriteLine("Command not recognized.");
        }

        ClearInputField();
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
        commandOutputText.text += $"\n> {add}";
    }
}