using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Dialogue_Superclass;

public class JJConsole : MonoBehaviour
{
    private static JJConsole _i;
    private int currentLine = 0;

    private void Awake()
    {
        _i = this;
        DontDestroyOnLoad(gameObject);
        commands.Add(new CommandList());
        commands.Add(new GetTime());
        UpdateVisuals();
    }

    public static JJConsole i
    {
        get
        {
            if (_i == null)
            {
                JJConsole x = Resources.Load<JJConsole>("JJConsole");

                _i = Instantiate(x);
            }

            return _i;
        }
    }

    public static char commandPrefix = '/';

    [HideInInspector] public List<HamCommand> commands = new List<HamCommand>();

    public TextMeshProUGUI commandOutputText;
    public TMP_InputField inputField;

    public CanvasGroup group;
    public RectTransform scrollViewport;

    [HideInInspector]
    public bool visible = false;

    private void Update()
    {
        ConsoleFunctionality();
    }

    private void ConsoleFunctionality()
    {
        Vector2 textSize = commandOutputText.GetRenderedValues(false);

        scrollViewport.sizeDelta = textSize;

        if (SlashKey() && !visible)
        {
            visible = true;
            UpdateVisuals();
        }
        else if (TabKey())
        {
            visible = !visible;
            UpdateVisuals();
        }

        Time.timeScale = visible ? 0 : 1;

        if (visible && ReturnKey())
        {
            TryCommand();
        }
    }

    public void UpdateVisuals()
    {
        group.alpha = visible ? 1 : 0;
        group.interactable = visible;
        group.blocksRaycasts = visible;

        ClearInputField();
    }

    public static bool Visible()
    {
        return i.visible;
    }

    private static bool SlashKey()
    {
        return Input.GetKeyDown(KeyCode.Slash);
    }

    private static bool TabKey()
    {
        return Input.GetKeyDown(KeyCode.Tab);
    }

    private static bool ReturnKey()
    {
        return Input.GetKeyDown(KeyCode.Return);
    }

    public void TryCommand()
    {
        string input = inputField.text;

        if(string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        if (input[0] == commandPrefix)
        {
            string[] split = input.Split(' ');
            HamCommand selectedCommand = Array.Find(commands.ToArray(), c => commandPrefix + c.Keyword() == split[0]);

            if(selectedCommand == null)
            {
                WriteLine("Command not recognized.");
                ClearInputField();
                return;
            }

            WriteLine($"<color=yellow>{selectedCommand.CommandFunction(split)}</color>");
        }
        else
        {
            WriteLine("Command not recognized.");
        }

        ClearInputField();
    }

    private void ClearInputField()
    {
        inputField.Select();
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void WriteLine(string add)
    {
        //commandOutputText.text += $"\n<color=yellow>[{currentLine}]</color>>{add}";
        commandOutputText.text += $"\n>{add}";
        currentLine++;
    }

    public DialogueManager GetDialogueManager()
    {
        return FindObjectOfType<DialogueManager>();
    }
}

public interface HamCommand
{
    string Keyword();
    string CommandFunction(params string[] parameters);
}

public class ConsoleUtility
{
    public static string ParameterParse(string[] parameters)
    {
        string result = "";

        for (int i = 1; i < parameters.Length; i++)
        {
            if (parameters.Length < 2)
            {
                result += parameters[i];
            }
            else if (i < parameters.Length - 1)
            {
                result += parameters[i] + " ";
            }
            else
            {
                result += parameters[i];
            }
        }

        return result;
    }
}

public class CommandList : HamCommand
{
    public string CommandFunction(params string[] parameters)
    {
        foreach (var command in JJConsole.i.commands)
        {
            JJConsole.i.WriteLine($"{command.Keyword()}");
        }

        JJConsole.i.WriteLine($"Some commands have a 'list' and 'all' function.");

        return $"Listed {JJConsole.i.commands.Count} commands.";
    }

    public string Keyword()
    {
        return "help";
    }
}

public class GetTime : HamCommand
{
    public string CommandFunction(params string[] parameters)
    {
        return DateTime.Today.ToLongDateString();
    }

    public string Keyword()
    {
        return "time";
    }
}

public class TestDialogue : HamCommand
{
    public string CommandFunction(params string[] parameters)
    {
        JJConsole.i.GetDialogueManager().StartDialogue(Dialogue());
        JJConsole.i.visible = false;
        JJConsole.i.UpdateVisuals();

        return "Testing dialogue...";
    }

    public string Keyword()
    {
        return "test_dialogue";
    }

    public DialogueSection Dialogue()
    {
        string localName = "Local Name";
        string sound = "dialogue";

        Monologue delay = new Monologue(localName, "This, well, this is supposed to test the delay. Really? I think it worked!", sound);
        Monologue color = new Monologue(localName, "This is testing whether the [color,yellow]colorful text[!color] works.", sound, delay);
        Monologue shake = new Monologue(localName, "This is testing whether the [shake,7]shaky text works[!shake].", sound, color);
        Monologue wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        Monologue basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        Choices loopQuestion = new Choices(localName, "Now, try again?", sound, ChoiceList(Choice("Yes", basic), Choice("No", null)));
        delay.next = loopQuestion;

        return basic;
    }
}