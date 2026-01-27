using System;
using BlackboxSystem;
using Infrastructure;
using UI;
using UnityEngine;
using World;

namespace Game
{
    public class DialogueManager : MonoBehaviour, IInjectable<GameAssetLibrary>
    {
        [SerializeField] private DialogueUI _dialogueUI;
        [Space]
        [SerializeField] private string _scriptTitle = string.Empty;
        [SerializeField] private KeyCode _startDialogue = KeyCode.C;

        private DialogueScriptLibrary _dialogueScriptLibrary;
        private string _currentScriptTitle = string.Empty;
        private IDisposable _updateHandle;


        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary)
        {
            _dialogueScriptLibrary = gameAssetLibrary.DialogueScriptLibrary;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_startDialogue))
            {
                if (string.IsNullOrWhiteSpace(_scriptTitle))
                {
                    Debug.LogWarning(
                        $"{nameof(_scriptTitle)}이(가) 유효하지 않기 떄문에 대화를 재생할 수 없습니다.",
                        this);
                    return;
                }

                Play(_scriptTitle);
            }
        }

        public void Play(string title)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play: {title}");

            if (!string.IsNullOrEmpty(_currentScriptTitle))
            {
                Debug.LogWarning(BlackboxHandle.Of(this).Write(
                    $"이미 스트립트 '{_currentScriptTitle}'이(가) 재생 중이기 때문에 새로운 스크립트 '{title}'을(를) 재생할 수 없습니다."),
                    this);
                return;
            }

            if (!_dialogueScriptLibrary.TryGetDialogueScript(title, out var script))
            {
                var currentScriptsList = string.Join(", ", _dialogueScriptLibrary.AllScriptTitles);

                Debug.LogError(BlackboxHandle.Of(this).CrashExport(
                    $"'{title}'을(를) 제목으로 가지는 대화를 {_dialogueScriptLibrary}에서 찾는 데 실패했습니다.\n" +
                    $"전체 대화 목록: {currentScriptsList}"),
                    this);

                return;
            }

            _currentScriptTitle = title;

            OnPlayStarting();
            _dialogueUI.Enable();

            int currentIdx = -1;
            PlayDialogue();

            _updateHandle = Loco.Subscribe(() =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (currentIdx >= script.Count - 1)
                    {
                        using var _ = BlackboxHandle.Of(this).WriteScope($"Play Dialogue (Completing): {currentIdx}");

                        Stop();
                        return;
                    }

                    PlayDialogue();
                }
            });

            void PlayDialogue()
            {
                currentIdx++;
                using var _ = BlackboxHandle.Of(this).WriteScope($"Play Dialogue: {currentIdx}");

                _dialogueUI.SetContent(script[currentIdx]);
            }
        }
        protected virtual void OnPlayStarting() { }
        protected virtual void OnPlayCompleting() { }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Stop");

            _updateHandle?.Dispose();
            _updateHandle = null;

            if (!string.IsNullOrEmpty(_currentScriptTitle))
                OnPlayCompleting();

            if (_dialogueUI)
                _dialogueUI.Disable();

            _currentScriptTitle = string.Empty;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");
            Stop();
        }
    }
}
