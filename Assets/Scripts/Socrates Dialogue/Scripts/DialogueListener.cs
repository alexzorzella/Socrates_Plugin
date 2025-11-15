namespace NewSocratesDialogue {
    public interface DialogueListener {
        void OnDialogueBegun();
        void OnSectionChanged(NewDialogueSection newSection);
        void OnDialogueEnded();
    }
}