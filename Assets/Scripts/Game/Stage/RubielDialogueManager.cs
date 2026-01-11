using System;
using Actors;
using Infrastructure;
using UI;
using UnityEngine;
using Actors.PlayerSystem;

namespace Game
{
    public class RubielDialogueManager : MonoBehaviour
    {
        [SerializeField] private Rubiel _rubiel;
        [SerializeField] private CharacterStateController playerStateController;
        [SerializeField] private DialogueUI _dialogueUI;
        [Space]
        [SerializeField] private DialogueData[] _dialogues;
        [SerializeField] private bool _commence_C = false;

        private bool _running = false;
        private IDisposable _updateHandle;


        private void Update()
        {
            if (_commence_C && Input.GetKeyDown(KeyCode.C))
                Commence();
        }

        private void Commence()
        {
            if (_running) return;
            _running = true;

            playerStateController.EnqueueTransition<NoInputState>();

            _dialogueUI.Enable();
            _rubiel.ToBig();

            int currentIdx = 0;

            _dialogueUI.SetContent(_dialogues[currentIdx]);
            currentIdx++;

            _updateHandle = Loco.Subscribe(() =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (currentIdx >= _dialogues.Length)
                    {
                        _dialogueUI.Disable();
                        _rubiel.ToSmall();

                        _updateHandle?.Dispose();
                        _updateHandle = null;

                        playerStateController.EnqueueTransition<NormalMovement>();
                        _running = false;
                        return;
                    }

                    _dialogueUI.SetContent(_dialogues[currentIdx]);
                    currentIdx++;
                }
            });
        }

        private void OnDestroy()
        {
            _updateHandle?.Dispose();
            _updateHandle = null;
        }
    }
}
