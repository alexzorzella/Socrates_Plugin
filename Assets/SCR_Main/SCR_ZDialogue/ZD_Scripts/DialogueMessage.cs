using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueMessage
{
    public string message;
    public List<Choice> choices;


    public DialogueMessage(string message, List<Choice> choices)
    {
        this.message = message;
        this.choices = choices;
    }

    public DialogueMessage(string message, params string[] choices)
    {
        this.message = message;
        this.choices = ChoiceList(choices);
    }

    private static List<Choice> ChoiceList(params string[] choices)
    {
        List<Choice> result = new List<Choice>();
        foreach(string choice in choices)
        {
            result.Add(new Choice(choice));
        }
        return result;
    }

    public void Link(params DialogueMessage[] destinations)
    {
        for (int i = 0; i < destinations.Length; i++)
        {
            this.choices[i].nextMessage = destinations[i];
        }
    }
}
