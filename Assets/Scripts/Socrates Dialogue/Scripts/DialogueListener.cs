namespace SocratesDialogue {
    public interface DialogueListener {
        void OnDialogueBegun();
        void OnSectionChanged(DialogueSection section);
        void OnDialogueEnded();
    }
}