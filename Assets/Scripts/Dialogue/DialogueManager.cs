using System;
using BlackboxSystem;
using Infrastructure;
using UI;
using UnityEngine;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour, IInjectable<DialogueScriptLibrary>
    {
        [SerializeField] private DialogueUI _dialogueUI;

        private DialogueScriptLibrary _dialogueScriptLibrary;
        private string _currentScriptTitle = string.Empty;
        private IDisposable _updateHandle;

        public void Initialize(DialogueUI dialogueUI)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize: {dialogueUI}");
            _dialogueUI = dialogueUI;
        }

        void IInjectable<DialogueScriptLibrary>.Inject(DialogueScriptLibrary dialogueScriptLibrary)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("DialogueScriptLibrary Injected");
            _dialogueScriptLibrary = dialogueScriptLibrary;
        }

        public void Play(string title, Action callback = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play: {title}");

            if (!string.IsNullOrEmpty(_currentScriptTitle))
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    $"이미 스트립트 '{_currentScriptTitle}'이(가) 재생 중이기 때문에 새로운 스크립트 '{title}'을(를) 재생할 수 없습니다."),
                    this);
                return;
            }

            if (!_dialogueScriptLibrary.TryGetDialogueScript(title, out var script))
            {
                var currentScriptsList = string.Join(", ", _dialogueScriptLibrary.AllScriptTitles);

                Debug.LogError(BlackboxHandle.Of(this).WriteError(
                    $"'{title}'을(를) 제목으로 가지는 대화를 {nameof(_dialogueScriptLibrary)}에서 찾는 데 실패했습니다.\n" +
                    $"전체 대화 목록: {currentScriptsList}"),
                    this);
                return;
            }

            _currentScriptTitle = title;
            OnPlayStarting();

            BlackboxHandle.Of(this).Exert(_dialogueUI, "Enable");

            _dialogueUI.transform.SetAsLastSibling();
            _dialogueUI.Enable();

            int currentIdx = -1;
            PlayDialogue();

            _updateHandle = Loco.Subscribe(() =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (currentIdx >= script.Count - 1)
                    {
                        using var _ = BlackboxHandle.Of(this).WriteScope($"Stopping: {currentIdx}");

                        Stop();
                        callback?.Invoke();
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
        protected virtual void OnPlayStopping() { }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop");

            _updateHandle?.Dispose();
            _updateHandle = null;

            if (!string.IsNullOrEmpty(_currentScriptTitle))
                OnPlayStopping();

            if (_dialogueUI) _dialogueUI.Disable();
            _currentScriptTitle = string.Empty;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");
            Stop();
        }
    }
}
