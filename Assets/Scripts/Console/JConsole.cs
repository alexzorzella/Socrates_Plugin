
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Dialogue_Superclass;

public class JConsole : MonoBehaviour
{
    private static JConsole _i;
    private int currentLine = 0;

    private List<string> history = new List<string>();
    private int historyIndex = -1;

    private void Awake()
    {
        if (_i != null)
		{
			if(_i != this)
			{
				Destroy(gameObject);
			}
		}

        _i = this;
        DontDestroyOnLoad(gameObject);

        commands.Add(new HCCommandList());
        commands.Add(new HCTestDialogue());
        commands.Add(new HCLoadScene());
        commands.Add(new HCForceQuit());

		foreach(var command in commands)
		{
			autocompleteCommands.Add(command.Keyword());
		}

		autocompleteOptionsRect = autocompleteOptionsText.GetComponent<RectTransform>();

		canvasRect = GetComponent<RectTransform>();

        UpdateVisuals();
    }

    public static JConsole i
    {
        get
        {
            if (_i == null)
            {
                JConsole x = Resources.Load<JConsole>("JConsole");

                _i = Instantiate(x);
            }

            return _i;
        }
    }

    public static char commandPrefix = '/';

    [HideInInspector] public List<HCommand> commands = new List<HCommand>();

    public TextMeshProUGUI commandOutputText;
    public TMP_InputField inputField;

	RectTransform canvasRect;

	List<string> autocompleteCommands = new List<string>();
    public TextMeshProUGUI autocompleteText;
	public TextMeshProUGUI autocompleteOptionsText;
	RectTransform autocompleteOptionsRect;
	public RectTransform autocompleteBackgroundRect;

	public int selectedAutocompleteOption = 0;

    public CanvasGroup group;
    public RectTransform scrollViewport;

    [HideInInspector]
    public bool visible = false;


    private void Update()
    {
		if (_i != null)
		{
			if(_i != this)
			{
				Destroy(gameObject);
			}
		}

		Autocomplete();

        ConsoleFunctionality();
        ScrollHistory();
    }

	private void ScrollAutocomplete(int wrapAt)
	{
		int scrollAmount = 0;

		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			scrollAmount = -1;
		} else if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			scrollAmount = 1;
		}

		if(scrollAmount != 0)
		{
			selectedAutocompleteOption = IncrementWithOverflow.Run(selectedAutocompleteOption, wrapAt, scrollAmount);
			inputField.MoveToEndOfLine(false, true);
		}
	}

	private string GetLastWord(string input)
	{
		if(string.IsNullOrWhiteSpace(input))
		{
			return "";
		}

		string result = "";

		string[] words = input.Split(' ');

		result = words[words.Length - 1];

		result = Regex.Replace(result, "[^A-Za-z0-9_'+.?!]", "");

		return result;
	}

	private void Autocomplete()
	{
		if(string.IsNullOrWhiteSpace(inputField.text))
		{
			autocompleteOptionsText.text = "";
			autocompleteText.text = "/";

			autocompleteBackgroundRect.sizeDelta = Vector2.zero;

			selectedAutocompleteOption = -1;
			return;
		}

		string lastWord = GetLastWord(inputField.text);

		List<string> sourceAutocompleteFrom = new List<string>();

		int lastWordIndex = inputField.text.Split(' ').Length;

		if(lastWordIndex == 1)
		{
			sourceAutocompleteFrom = autocompleteCommands;
		} else if(lastWordIndex == 2)
		{
			if(GetCurrentCommand() != null)
			{
				sourceAutocompleteFrom = GetCurrentCommand().AutocompleteOptions();
			}
		}

		List<string> autocompleteOptions = new List<string>();

		foreach(var option in sourceAutocompleteFrom)
		{
			if(option.StartsWith(lastWord))
			{
				autocompleteOptions.Add(option);
			}
		}

		if(autocompleteOptions.Count <= 0)
		{
			autocompleteOptionsText.text = "";
			autocompleteText.text = "/";

			autocompleteBackgroundRect.sizeDelta = Vector2.zero;

			selectedAutocompleteOption = -1;

			return;
		}

		if(selectedAutocompleteOption < 0)
		{
			selectedAutocompleteOption = 0;
		}

		ScrollAutocomplete(autocompleteOptions.Count);

		string finalAutocomplete = "";

		string selectedOption = autocompleteOptions[selectedAutocompleteOption];

		if(!string.IsNullOrWhiteSpace(selectedOption))
		{
			finalAutocomplete = inputField.text;
			finalAutocomplete += selectedOption.Substring(lastWord.Length, selectedOption.Length - lastWord.Length);
		}

		string optionsPrompt = "";

		if(autocompleteOptions.Count > 1)
		{
			for(int i = 0; i < autocompleteOptions.Count; i++)
			{
				if(i == selectedAutocompleteOption)
				{
					optionsPrompt += "<color=yellow>";
				}

				optionsPrompt += autocompleteOptions[i];

				if(i == selectedAutocompleteOption)
				{
					optionsPrompt += "</color>";
				}

				if(i < autocompleteOptions.Count - 1)
				{
					optionsPrompt += "\n";
				}
			}
		}

		autocompleteOptionsText.text = optionsPrompt;

		autocompleteText.text = finalAutocomplete;

		autocompleteText.ForceMeshUpdate();
		autocompleteOptionsText.ForceMeshUpdate();

		if(!string.IsNullOrWhiteSpace(autocompleteText.text))
		{
			TMP_TextInfo textInfo = autocompleteText.textInfo;

			int indexOfLastWord = inputField.text.Length - lastWord.Length;

			Vector2 promptPosition = textInfo.characterInfo[indexOfLastWord].bottomLeft;

			Vector3 worldBottomLeft = autocompleteText.transform.TransformPoint(promptPosition);

			autocompleteBackgroundRect.anchoredPosition = new Vector2(worldBottomLeft.x / canvasRect.localScale.x, 145F);

			Vector2 textSize = autocompleteOptionsText.GetRenderedValues(false);
			Vector2 paddingSize = new Vector2(8, 8);

			autocompleteBackgroundRect.sizeDelta = textSize + paddingSize;
		} else
		{
			autocompleteBackgroundRect.sizeDelta = Vector2.zero;
		}

		if(Input.GetKeyDown(KeyCode.Tab))
		{
			inputField.text = finalAutocomplete;
			inputField.MoveToEndOfLine(false, true);
			
			selectedAutocompleteOption = -1;
			autocompleteOptions.Clear();
		}

		if(Input.GetKey(KeyCode.LeftShift))
		{
			if(Input.GetKeyDown(KeyCode.Backspace))
			{
				inputField.text = "";
			}
		}
	}

	private void ScrollHistory()
    {
        if (visible)
        {
            if (history.Count > 0 && selectedAutocompleteOption < 0)
            {
                if (Input.anyKeyDown)
                {
                    if (Input.GetKey(KeyCode.UpArrow))
                    {
                        ScrollHistoryBy(1);
                    }
                    else if (Input.GetKey(KeyCode.DownArrow))
                    {
                        ScrollHistoryBy(-1);
                    }
                    else
                    {
                        historyIndex = -1;
                    }
                }
            }
        }
    }

    private void ScrollHistoryBy(int amount)
    {
        historyIndex = IncrementWithOverflow.Run(historyIndex, history.Count, amount);
        inputField.text = history[historyIndex];
        inputField.caretPosition = history[historyIndex].Length;
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
        else if (EscapeKey())
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

    public static bool Open()
    {
        return i.visible;
    }

    private static bool SlashKey()
    {
        return Input.GetKeyDown(KeyCode.Slash);
    }

    private static bool EscapeKey()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private static bool ReturnKey()
    {
        return Input.GetKeyDown(KeyCode.Return);
    }

	public HCommand GetCurrentCommand()
	{
        string input = inputField.text;

		string[] split = input.Split(' ');
        HCommand selectedCommand = Array.Find(commands.ToArray(), c => commandPrefix + c.Keyword() == split[0]);

		return selectedCommand;
	}

    public void TryCommand()
    {
        string input = inputField.text;

        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        if (input[0] == commandPrefix)
        {
            HCommand selectedCommand = GetCurrentCommand();

            history.Add(input);

            if (selectedCommand == null)
            {
                WriteLine("Command not recognized.");
                ClearInputField();
                return;
            }

            WriteLine($"<color=yellow>{selectedCommand.CommandFunction(input.Split(' '))}</color>");
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
        commandOutputText.text += $"\n> {add}";
        currentLine++;
    }

    public DialogueManager GetDialogueManager()
    {
        return FindObjectOfType<DialogueManager>();
    }
}

public interface HCommand
{
    string Keyword();
    string CommandHelp();
	List<string> AutocompleteOptions();
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

public class HCCommandList : HCommand
{
	public string CommandFunction(params string[] parameters)
    {
        foreach (var command in JConsole.i.commands)
        {
            JConsole.i.WriteLine($"{command.Keyword()} {command.CommandHelp()}");
        }

        JConsole.i.WriteLine($"Some commands have a 'list' and 'all' function.");

        return $"Listed {JConsole.i.commands.Count} commands.";
    }

    public string CommandHelp()
    {
        return "";
    }

    public string Keyword()
    {
        return "help";
    }

	List<string> options = new List<string>() { };

	public List<string> AutocompleteOptions()
	{
		return options;
	}
}

public class HCTestDialogue : HCommand
{
    public string CommandFunction(params string[] parameters)
    {
        JConsole.i.GetDialogueManager().StartDialogue(Dialogue());
        JConsole.i.visible = false;
        JConsole.i.UpdateVisuals();

        return "Testing dialogue...";
    }

    public string Keyword()
    {
        return "testDialogue";
    }

    public DialogueSection Dialogue()
    {
        string localName = "Alex";
        string sound = "dialogue";

        Monologue l = new Monologue(localName, "This [color,yellow]dialogue system[!color] and [color,yellow]text interpreter[!color] took [wave]years[!wave] of updating to get here, thanks to my dad.", sound);
        Monologue delay = new Monologue(localName, "This, well, this is supposed to test the delay. Really? I think it worked!", sound, l);
        Monologue color = new Monologue(localName, "This is testing whether the [color,yellow]colorful text[!color] works.", sound, delay);
        Monologue shake = new Monologue(localName, "This is testing whether the [shake,7]shaky text works[!shake].", sound, color);
        Monologue wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        Monologue basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        Choices loopQuestion = new Choices(localName, "Now, try again?", sound, ChoiceList(Choice("Yes", basic), Choice("No", null)));
        l.next = loopQuestion;

        return basic;
    }

    public string CommandHelp()
    {
        return "";
    }

	List<string> options = new List<string>() { };

	public List<string> AutocompleteOptions()
	{
		return options;
	}
}

public class HCLoadScene : HCommand
{
    public string CommandFunction(params string[] parameters)
    {
        bool sceneExists = SceneManager.GetSceneByName(parameters[1]) != null;

        if (sceneExists)
            SceneManager.LoadScene(parameters[1]);

        return sceneExists ? $"Loading {parameters[1]}..." : $"{parameters[1]} doesn't exist.";
    }

    public string CommandHelp()
    {
        return "(string sceneName)";
    }

    public string Keyword()
    {
        return "loadScene";
    }

	List<string> options = new List<string>() { };

	public List<string> AutocompleteOptions()
	{
		if(options.Count <= 0)
		{
			int sceneCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; i++)
            {
                options.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            }
		}

		return options;
	}
}

public class HCForceQuit : HCommand
{
    public string CommandFunction(params string[] parameters)
    {
        Application.Quit();

        return $"Quitting...";
    }

    public string CommandHelp()
    {
        return "";
    }

    public string Keyword()
    {
        return "quit";
    }

	List<string> options = new List<string>() { };

	public List<string> AutocompleteOptions()
	{
		return options;
	}
}