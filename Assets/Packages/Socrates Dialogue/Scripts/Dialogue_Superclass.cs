using System;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_Superclass : MonoBehaviour {
    public static Tuple<string, DialogueSection> Choice(string choice, DialogueSection next) {
        return new Tuple<string, DialogueSection>(choice, next);
    }

    public static List<Tuple<string, DialogueSection>> ChoiceList(params Tuple<string, DialogueSection>[] entries) {
        var result = new List<Tuple<string, DialogueSection>>();

        foreach (var tuple in entries) result.Add(tuple);

        return result;
    }

    public interface DialogueSection {
        string GetTitle();
        string GetSpeakerName();
        string GetDialogueSound();
        Action GetAction();
        bool IsMonotone();
        void TriggerAction(bool isStart);
        DialogueSection GetNextSection();
        float CharDelay();
        bool HasExecutedAction();
        void SetActionExecution(bool set);
    }

    public class Monologue : DialogueSection {
        public Action action;
        public float charDelay;
        public string content;

        public string dialogueSound;
        public bool hasExecutedAction;
        public bool isMonotone;

        // If there is no next, terminate
        public DialogueSection next;
        public string speakerName;
        public bool triggerActionWhenStart;

        public Monologue(
            string speakerName = "DefualtName",
            string content = "Default dialogue content.",
            string sound = "dialogue",
            DialogueSection next = null,
            Action action = null,
            bool triggerActionWhenStart = false,
            bool isMonotone = false,
            float charDelay = 0.02F) {
            this.speakerName = speakerName;
            this.content = content;
            this.next = null;
            dialogueSound = sound;
            this.next = next;
            this.action = action;
            this.triggerActionWhenStart = triggerActionWhenStart;
            this.isMonotone = isMonotone;
            this.charDelay = charDelay;
        }

        public DialogueSection GetNextSection() {
            return next;
        }

        public string GetSpeakerName() {
            return speakerName;
        }

        public string GetTitle() {
            return content;
        }

        public string GetDialogueSound() {
            return dialogueSound;
        }

        public Action GetAction() {
            return action;
        }

        public void TriggerAction(bool isStart) {
            if (isStart == triggerActionWhenStart)
                //Debug.Log($"Triggered action, isStart: {isStart}, triggerActionWhenStart: {triggerActionWhenStart}");
                action.Invoke();
        }

        public bool IsMonotone() {
            return isMonotone;
        }

        public float CharDelay() {
            return charDelay;
        }

        public bool HasExecutedAction() {
            return hasExecutedAction;
        }

        public void SetActionExecution(bool set) {
            hasExecutedAction = set;
        }
    }

    public class Choices : DialogueSection {
        public float charDelay;
        public List<Tuple<string, DialogueSection>> choices;
        public string dialogueSound;
        public bool isMonotone;
        public string speakerName;
        public string title;

        public Choices(
            string speakerName, string title, string sound,
            List<Tuple<string, DialogueSection>> choices, bool isMonotone = false, float charDelay = 0.02F) {
            this.speakerName = speakerName;
            this.title = title;
            this.choices = choices;
            dialogueSound = sound;
            this.isMonotone = isMonotone;
            this.charDelay = charDelay;
        }

        public DialogueSection GetNextSection() {
            return null;
        }

        public string GetSpeakerName() {
            return speakerName;
        }

        public string GetTitle() {
            return title;
        }

        public string GetDialogueSound() {
            return dialogueSound;
        }

        public Action GetAction() {
            return null;
        }

        public void TriggerAction(bool isStart) {
        }

        public bool IsMonotone() {
            return isMonotone;
        }

        public float CharDelay() {
            return charDelay;
        }

        public bool HasExecutedAction() {
            return true;
        }

        public void SetActionExecution(bool set) {
        }
    }
}