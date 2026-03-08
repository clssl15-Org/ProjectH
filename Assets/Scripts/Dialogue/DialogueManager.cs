using System;
using BlackboxSystem;
using Infrastructure;
using UI;
using UnityEngine;
using World;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour,
        IInjectable<DialogueScriptLibrary>,
        IInputLayerSubject
    {
        [field: Header("Bubble Settings")]
        [field: SerializeField] public Vector2 BubbleOffset { get; set; } = new(0, 100);
        [field: SerializeField, Min(100)] private int MaxBubbleWidth { get; set; } = 500;
        [field: SerializeField] private Vector2 BubblePadding { get; set; } = new(100, 100);

        public bool AllowInput { get; set; } = true;
        bool IInputLayerSubject.IsTrigger { get; } = false;

        public event Action Destroying;

        [Header("Bindings")]
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private BubbleDialogueUI _bubbleDialogueUI;
        [SerializeField] private RectTransform _canvasTransform;
        private Func<Character, Transform> _getTransform;

        private DialogueScriptLibrary _dialogueScriptLibrary;
        private string _currentScriptTitle = string.Empty;
        private IDisposable _updateHandle;

        public void Initialize(
            DialogueUI dialogueUI,
            BubbleDialogueUI bubbleDialogueUI,
            RectTransform canvasTrasnform,
            Func<Character, Transform> getTransform)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize: {dialogueUI}, {bubbleDialogueUI}");

            _dialogueUI = dialogueUI;
            _bubbleDialogueUI = bubbleDialogueUI;
            _canvasTransform = canvasTrasnform;
            _getTransform = getTransform;
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

            if (script.TargetDialogueStyle == DialogueStyle.ChatBubble)
            {
                CalculateAndSetBubbleSize(script);

                BlackboxHandle.Of(this).Exert(_bubbleDialogueUI, "Enable");
                _bubbleDialogueUI.transform.SetAsLastSibling();
            }
            else
            {
                BlackboxHandle.Of(this).Exert(_dialogueUI, "Enable");
                _dialogueUI.transform.SetAsLastSibling();
                _dialogueUI.Enable();
            }

            int currentIdx = -1;
            PlayDialogue();

            _updateHandle = Loco.Subscribe(() =>
            {
                if (AllowInput && Input.GetMouseButtonDown(0))
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

                if (script.TargetDialogueStyle == DialogueStyle.ChatBubble)
                {
                    var line = script[currentIdx];
                    var characterTransform = _getTransform(line.Character);

                    _bubbleDialogueUI
                        .Show(new BubbleContainer(_canvasTransform)
                        .With(line.Dialogue, characterTransform, BubbleOffset));
                }
                else
                {
                    _dialogueUI.SetContent(script[currentIdx]);
                }
            }
        }

        private void CalculateAndSetBubbleSize(DialogueScriptSO script)
        {
            float targetWidth = 0;
            float targetHeight = 0;

            float maxTextWidth = MaxBubbleWidth - BubblePadding.x;

            for (int i = 0; i < script.Count; i++)
            {
                var text = script[i].Dialogue;
                var textSize = _bubbleDialogueUI.GetPreferredValues(text, maxTextWidth);

                if (textSize.x > targetWidth) targetWidth = textSize.x;
                if (textSize.y > targetHeight) targetHeight = textSize.y;
            }

            _bubbleDialogueUI.SetPanelSize(new()
            {
                x = Mathf.Min(targetWidth + BubblePadding.x, MaxBubbleWidth),
                y = targetHeight + BubblePadding.y
            });
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
            if (_bubbleDialogueUI) _bubbleDialogueUI.Hide();

            _currentScriptTitle = string.Empty;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            Stop();
            Destroying?.Invoke();
        }
    }
}