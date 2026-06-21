using System;
using BlackThunder.BlackboxSystem;
using Game.Management;
using Infrastructure;
using UI;
using UnityEngine;
using World;

namespace Dialogue
{
    public class DialogueManager : MonoBehaviour,
        IInjectable<GameAssetLibrary>,
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

        private Func<Character, Func<Vector2>> _getTransform;
        private GameAssetLibrary _gameAssetLibrary;
        private DialogueScriptLibrary _dialogueScriptLibrary;

        private IDialogueUI _currentUI;
        private DialogueScriptSO _currentScript;
        private string _currentScriptTitle = string.Empty;
        private int _currentLineIndex = -1;
        private bool _currentCloseDialogue = true;
        private Action _currentCallback;
        private IDisposable _updateHandle;
        private BlackboxHandle _blackbox;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("대화 매니저 초기화를 시작합니다.", out _blackbox);
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        public void Initialize(
            DialogueUI dialogueUI,
            BubbleDialogueUI bubbleDialogueUI,
            RectTransform canvasTrasnform,
            Func<Character, Func<Vector2>> getTransform)
        {
            using var _ = _blackbox.Scope("대화 UI 참조를 초기화합니다.").With(dialogueUI, bubbleDialogueUI, canvasTrasnform);

            _dialogueUI = dialogueUI;
            _bubbleDialogueUI = bubbleDialogueUI;
            _canvasTransform = canvasTrasnform;
            _getTransform = getTransform;
        }


        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary)
        {
            using var _ = _blackbox.Scope("게임 에셋 라이브러리를 주입받습니다.").With(gameAssetLibrary);

            _gameAssetLibrary = gameAssetLibrary;
        }
        void IInjectable<DialogueScriptLibrary>.Inject(DialogueScriptLibrary dialogueScriptLibrary)
        {
            using var _ = _blackbox.Scope("대화 스크립트 라이브러리를 주입받습니다.").With(dialogueScriptLibrary);

            _dialogueScriptLibrary = dialogueScriptLibrary;
        }

        public void Play(string title, bool openDialogue = true, bool closeDialogue = true, Action callback = null)
        {
            using var _ = _blackbox.Scope($"대화 재생을 시작합니다. title: {title}, openDialogue: {openDialogue}, closeDialogue: {closeDialogue}");

            if (!string.IsNullOrEmpty(_currentScriptTitle))
            {
                Debug.LogWarning($"이미 스크립트 '{_currentScriptTitle}'을(를) 재생 중이기 때문에 새로운 스크립트 '{title}'을(를) 재생할 수 없습니다.",
                    this);
                return;
            }

            var script = GetRequiredDialogueScript(title, LanguageManager.Language);

            _currentScriptTitle = title;
            _currentScript = script;
            _currentLineIndex = -1;
            _currentCloseDialogue = closeDialogue;
            _currentCallback = callback;
            OnPlayStarting();

            if (_currentScript.TargetStyle == DialogueStyle.ChatBubble)
            {
                CalculateAndSetBubbleSize(_currentScript);

                _bubbleDialogueUI.transform.SetAsLastSibling();

                _currentUI = _bubbleDialogueUI;
            }
            else
            {
                _dialogueUI.transform.SetAsLastSibling();
                if (openDialogue) _dialogueUI.Enable();

                _currentUI = _dialogueUI;
            }

            PlayDialogue();

            _updateHandle = Loco.Subscribe(() =>
            {
                var isPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F);
                if (AllowInput && isPressed)
                {
                    if (!_currentUI.IsTotallyTyped)
                    {
                        _currentUI.SkipTyping();
                        return;
                    }

                    if (_currentScript.ShowOneRandomLine || _currentLineIndex >= _currentScript.Count - 1)
                    {

                        var callbackOnStop = _currentCallback;
                        Stop(_currentCloseDialogue);
                        callbackOnStop?.Invoke();
                        return;
                    }

                    PlayDialogue();
                }
            });

            void PlayDialogue()
            {
                if (_currentScript.ShowOneRandomLine)
                    _currentLineIndex = UnityEngine.Random.Range(0, _currentScript.Count);
                else
                    _currentLineIndex++;


                RenderCurrentLine(showImmediately: false);
            }
        }

        private void OnLanguageChanged(Language language)
        {
            if (string.IsNullOrEmpty(_currentScriptTitle))
                return;

            var showImmediately = _currentUI?.IsTotallyTyped ?? true;
            _currentScript = GetRequiredDialogueScript(_currentScriptTitle, language);
            ClampCurrentLineIndex();

            RenderCurrentLine(showImmediately);
        }

        private DialogueScriptSO GetRequiredDialogueScript(string title, Language language)
        {
            if (_dialogueScriptLibrary == null)
                throw new InvalidOperationException($"{nameof(DialogueScriptLibrary)}가 주입되지 않았습니다.");

            if (_dialogueScriptLibrary.TryGetDialogueScript(title, language, out var script))
            {
                ValidateDialogueScript(script);
                return script;
            }

            var currentScriptList = string.Join(", ", _dialogueScriptLibrary.AllScriptTitles);
            throw new InvalidOperationException(
                $"'{title}' 대화 스크립트({language})를 {nameof(DialogueScriptLibrary)}에서 찾지 못했습니다.\n" +
                $"전체 대화 목록: {currentScriptList}");
        }

        private static void ValidateDialogueScript(DialogueScriptSO script)
        {
            if (script == null)
                throw new InvalidOperationException("대화 스크립트가 비어 있습니다.");

            if (script.Count <= 0)
                throw new InvalidOperationException(
                    $"'{script.Title}' 대화 스크립트({script.Language})에 대사가 없습니다.");
        }

        private void ClampCurrentLineIndex()
        {
            ValidateDialogueScript(_currentScript);
            _currentLineIndex = Mathf.Clamp(_currentLineIndex, 0, _currentScript.Count - 1);
        }

        private void RenderCurrentLine(bool showImmediately)
        {
            ClampCurrentLineIndex();

            if (_currentScript.TargetStyle == DialogueStyle.ChatBubble)
            {
                CalculateAndSetBubbleSize(_currentScript);

                var line = _currentScript[_currentLineIndex];
                var dialogueText = line.Dialogue;

                if (_gameAssetLibrary.TryGetCharacterInfo(Character.Player, out var playerInfo))
                    dialogueText = dialogueText.Replace(
                        "{player}",
                        playerInfo.GetName(LanguageManager.Language),
                        StringComparison.OrdinalIgnoreCase);
                else
                    Debug.LogWarning("Player 정보를 가져오지 못했기 때문에 {player} 문자열을 치환하지 못했습니다.");

                var characterPosition = _getTransform(line.Character);

                _bubbleDialogueUI
                    .Show(new BubbleContainer(_canvasTransform)
                    .With(dialogueText, characterPosition, BubbleOffset),
                    showImmediately);
            }
            else
            {
                _dialogueUI.SetContent(_currentScript[_currentLineIndex], showImmediately);
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

        public void Stop(bool closeDialogue = true)
        {
            using var _ = _blackbox.Scope($"대화 재생을 종료합니다. title: {_currentScriptTitle}, closeDialogue: {closeDialogue}");

            _updateHandle?.Dispose();
            _updateHandle = null;

            if (!string.IsNullOrEmpty(_currentScriptTitle))
                OnPlayStopping();

            if (closeDialogue)
            {
                if (_dialogueUI) _dialogueUI.Disable();
                if (_bubbleDialogueUI) _bubbleDialogueUI.Hide();
            }

            _currentUI = null;
            _currentScriptTitle = string.Empty;
            _currentScript = null;
            _currentLineIndex = -1;
            _currentCloseDialogue = true;
            _currentCallback = null;
        }

        private void OnDestroy()
        {
            using var _ = _blackbox.Scope("대화 매니저를 정리합니다.");

            LanguageManager.LanguageChanged -= OnLanguageChanged;
            Stop();
            Destroying?.Invoke();
        }
    }
}
