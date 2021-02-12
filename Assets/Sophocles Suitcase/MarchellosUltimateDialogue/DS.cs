using System;
using System.Collections.Generic;
using UnityEngine;

public class DS : MonoBehaviour
{
    public interface DialogueSection {

        string GetTitle();
        string GetSpeakerName();
        string GetDialogueSound();
        Action GetAction();
        bool IsMonotone();
        void TriggerAction(bool isStart);
        DialogueSection GetNextSection();
        float CharDelay();
    }

    public class Monologue : DialogueSection
    {
        public string speakerName;
        public string content;
        public string dialogueSound;
        // If there is no next, terminate
        public DialogueSection next;
        public Action action;
        public bool triggerActionWhenStart;
        public bool isMonotone;
        public float charDelay;

        public Monologue(
            string speakerName = "DefualtName",
            string content = "Default dialogue content.",
            string sound = "dialogue",
            DialogueSection next = null,
            Action action = null,
            bool triggerActionWhenStart = false,
            bool isMonotone = false,
            float charDelay = 0.02F)
        {
            this.speakerName = speakerName;
            this.content = content;
            this.next = null;
            this.dialogueSound = sound;
            this.next = next;
            this.action = action;
            this.triggerActionWhenStart = triggerActionWhenStart;
            this.isMonotone = isMonotone;
            this.charDelay = charDelay;
        }

        public DialogueSection GetNextSection()
        {
            return next;
        }

        public string GetSpeakerName()
        {
            return speakerName;
        }

        public string GetTitle()
        {
            return content;
        }

        public string GetDialogueSound()
        {
            return this.dialogueSound;
        }

        public Action GetAction()
        {
            return action;
        }

        public void TriggerAction(bool isStart)
        {
            if(isStart == triggerActionWhenStart)
            {
                action.Invoke();
            }
        }

        public bool IsMonotone()
        {
            return isMonotone;
        }

        public float CharDelay()
        {
            return charDelay;
        }
    }

    public class Choices : DialogueSection
    {
        public string speakerName;
        public string title;
        public string dialogueSound;
        public List<Tuple<string, DialogueSection>> choices;
        public bool isMonotone;
        public float charDelay;

        public Choices(
            string speakerName, string title, string sound,
            List<Tuple<string, DialogueSection>> choices, bool isMonotone = false, float charDelay = 0.02F)
        {
            this.speakerName = speakerName;
            this.title = title;
            this.choices = choices;
            this.dialogueSound = sound;
            this.isMonotone = isMonotone;
            this.charDelay = charDelay;
        }

        public DialogueSection GetNextSection()
        {
            return null;
        }

        public string GetSpeakerName()
        {
            return speakerName;
        }

        public string GetTitle()
        {
            return title;
        }
        
        public string GetDialogueSound()
        {
            return this.dialogueSound;
        }

        public Action GetAction()
        {
            return null;
        }

        public void TriggerAction(bool isStart)
        {

        }

        public bool IsMonotone()
        {
            return isMonotone;
        }

        public float CharDelay()
        {
            return charDelay;
        }
    }

    public static Tuple<string, DialogueSection> Choice(string choice, DialogueSection next)
    {
        return new Tuple<string, DialogueSection>(choice, next);
    }

    public static List<Tuple<string, DialogueSection>> ChoiceList(params Tuple<string, DialogueSection>[] entries)
    {
        List<Tuple<string, DialogueSection>> result = new List<Tuple<string, DialogueSection>>();

        foreach (var tuple in entries)
        {
            result.Add(tuple);
        }

        return result;
    }
}