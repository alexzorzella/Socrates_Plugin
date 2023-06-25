using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Console : MonoBehaviour
{
    private static Console _i;
    private int currentLine = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        commands.Add(new CommandList());
        commands.Add(new GetTime());
        UpdateVisuals();
    }

    public static Console i
    {
        get
        {
            if (_i == null)
            {
                Console x = Resources.Load<Console>("Console");

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
    bool visible = false;

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

    private void UpdateVisuals()
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
        commandOutputText.text += $"\n<color=yellow>[{currentLine}]</color>>{add}";
        currentLine++;
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
        foreach (var command in Console.i.commands)
        {
            Console.i.WriteLine($"{command.Keyword()}");
        }

        Console.i.WriteLine($"Some commands have a 'list' and 'all' function.");

        return $"Listed {Console.i.commands.Count} commands.";
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