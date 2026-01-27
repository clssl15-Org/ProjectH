using Actors;
using Actors.PlayerSystem;
using UnityEngine;

namespace Game
{
    public class RubielDialogueManager : DialogueManager
    {
        [Header("Rubiel Dialogue Manager")]
        [SerializeField] private Rubiel _rubiel;
        [SerializeField] private CharacterStateController _playerStateController;


        protected override void OnPlayStarting()
        {
            _playerStateController.EnqueueTransition<NoInputState>();
            _rubiel.ToBig();
        }

        protected override void OnPlayCompleting()
        {
            _rubiel.ToSmall();
            _playerStateController.EnqueueTransition<NormalMovement>();

        }
    }
}
