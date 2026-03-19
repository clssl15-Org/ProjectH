namespace UI
{
    public interface IDialogueUI
    {
        bool IsTotallyTyped { get; }
        void SkipTyping();
    }
}
