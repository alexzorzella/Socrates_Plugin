namespace SocratesDialogue {
    public interface DialogueListener {
        void OnDialogueBegun();
        void OnSectionChanged(DialogueSection newSection);
        void OnDialogueEnded();
    }
}