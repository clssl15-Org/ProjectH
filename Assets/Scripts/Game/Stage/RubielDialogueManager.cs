using Actors;
using Actors.PlayerSystem;
using Dialogue;
using UnityEngine;
using BlackboxSystem;

namespace Game
{
    public class RubielDialogueManager : DialogueManager
    {
        [Header("Rubiel Dialogue Manager")]
        [SerializeField] private Rubiel _rubiel;
        [SerializeField] private CharacterStateController _playerStateController;
        [Space]
        [SerializeField] private string _dialogueTitle = string.Empty;
        [SerializeField] private KeyCode _startDialogue = KeyCode.C;

        private string DialogueTitle => _dialogueTitle;

        private void Update()
        {
            if (Input.GetKeyDown(_startDialogue))
            {
                if (string.IsNullOrWhiteSpace(DialogueTitle))
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"{nameof(DialogueTitle)}이(가) 유효하지 않기 떄문에 대화를 재생할 수 없습니다."),
                        this);
                    return;
                }

                Play(DialogueTitle);
            }
        }

        protected override void OnPlayStarting()
        {
            _playerStateController.EnqueueTransition<NoInputState>();
            _rubiel.ToBig();
        }

        protected override void OnPlayStopping()
        {
            _rubiel.ToSmall();
            _playerStateController.EnqueueTransition<NormalMovement>();
        }
    }
}
