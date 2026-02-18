using Actors;
using Actors.PlayerSystem;
using Dialogue;
using UnityEngine;
using BlackboxSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public class RubielDialogueManager : DialogueManager
    {
        [Header("Rubiel Dialogue Manager")]
        [SerializeField] private Rubiel _rubiel;
        [SerializeField] private CharacterStateController _playerStateController;
        [Space]
        [SerializeField] private DialogueTitle _dialogueTitle = DialogueTitle.None;
        [SerializeField] private string _title = string.Empty;
        [SerializeField] private KeyCode _startDialogue = KeyCode.C;

        private string Title => _dialogueTitle != DialogueTitle.Undefined
            ? _dialogueTitle.ToString() : _title;

        private void Update()
        {
            if (Input.GetKeyDown(_startDialogue))
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"{nameof(Title)}이(가) 유효하지 않기 떄문에 대화를 재생할 수 없습니다."),
                        this);
                    return;
                }

                Play(Title);
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

#if UNITY_EDITOR
#if UNITY_EDITOR
        [CustomEditor(typeof(RubielDialogueManager)), CanEditMultipleObjects]
        protected class RubielDialogueManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var target = (RubielDialogueManager)base.target;

                if (target._dialogueTitle != DialogueTitle.Undefined)
                    DrawPropertiesExcluding(serializedObject, nameof(_title));
                else
                    DrawDefaultInspector();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
#endif
    }
}
