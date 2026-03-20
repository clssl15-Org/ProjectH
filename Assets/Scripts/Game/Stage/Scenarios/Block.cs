using System;
using Dialogue;
using Infrastructure.StateMachines.Fsm;

namespace Game.Stage
{
    internal class Block : Work<ScenarioManager.ScenarioMachine>
    {
        public bool ToNextToken { get; set; } = false;
        public Block(object name) : base(name.ToString()) { }
    }

    internal class DialogueBlock : Block
    {
        public bool OpenDialogue { get; init; } = true;
        public bool CloseDialogue { get; init; } = true;

        private DialogueManager DialogueManager => Parent.StageManager.DialogueManager;
        private string _dialogueTitle;
        private Action _onDialogueEnd;

        public DialogueBlock(object name, object dialogueTitle, Action onDialogueEnd) : base(name)
        {
            _dialogueTitle = dialogueTitle.ToString();
            _onDialogueEnd = onDialogueEnd;
        }

        protected override void OnEnter(params object[] _)
        {
            DialogueManager.Play(_dialogueTitle, OpenDialogue, CloseDialogue, _onDialogueEnd);
        }

        protected override void OnExit()
        {
            if (DialogueManager)
                DialogueManager.Stop(CloseDialogue);
        }
    }
}
 