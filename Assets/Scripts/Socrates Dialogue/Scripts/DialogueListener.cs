namespace NewSocratesDialogue {
    public interface DialogueListener {
        void OnDialogueBegun();
        void OnSectionChanged(DialogueSection section);
        void OnDialogueEnded();
    }
}