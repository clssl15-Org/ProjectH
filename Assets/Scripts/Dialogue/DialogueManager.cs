using System;
using System.Linq;
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
        private string _currentScriptTitle = string.Empty;
        private IDisposable _updateHandle;

        public void Initialize(
            DialogueUI dialogueUI,
            BubbleDialogueUI bubbleDialogueUI,
            RectTransform canvasTrasnform,
            Func<Character, Func<Vector2>> getTransform)
        {

            _dialogueUI = dialogueUI;
            _bubbleDialogueUI = bubbleDialogueUI;
            _canvasTransform = canvasTrasnform;
            _getTransform = getTransform;
        }


        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary)
        {
            _gameAssetLibrary = gameAssetLibrary;
        }
        void IInjectable<DialogueScriptLibrary>.Inject(DialogueScriptLibrary dialogueScriptLibrary)
        {
            _dialogueScriptLibrary = dialogueScriptLibrary;
        }

        public void Play(string title, bool openDialogue = true, bool closeDialogue = true, Action callback = null)
        {

            if (!string.IsNullOrEmpty(_currentScriptTitle))
            {
                Debug.LogWarning($"�̹� ��Ʈ��Ʈ '{_currentScriptTitle}'��(��) ��� ���̱� ������ ���ο� ��ũ��Ʈ '{title}'��(��) ����� �� �����ϴ�.",
                    this);
                return;
            }

            if (!_dialogueScriptLibrary.TryGetDialogueScript(title, out var script))
            {
                var currentScriptList = string.Join(", ", _dialogueScriptLibrary.AllScriptTitles);

                Debug.LogError($"'{title}'��(��) �������� ������ ��ȭ�� {nameof(DialogueScriptLibrary)}���� ã�� �� �����߽��ϴ�.\n" +
                    $"��ü ��ȭ ���: {currentScriptList}",
                    this);
                return;
            }

            _currentScriptTitle = title;
            OnPlayStarting();

            if (script.TargetStyle == DialogueStyle.ChatBubble)
            {
                CalculateAndSetBubbleSize(script);

                _bubbleDialogueUI.transform.SetAsLastSibling();

                _currentUI = _bubbleDialogueUI;
            }
            else
            {
                _dialogueUI.transform.SetAsLastSibling();
                if (openDialogue) _dialogueUI.Enable();

                _currentUI = _dialogueUI;
            }

            int currentIdx = -1;
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

                    if (script.ShowOneRandomLine || currentIdx >= script.Count - 1)
                    {

                        Stop(closeDialogue);
                        callback?.Invoke();
                        return;
                    }

                    PlayDialogue();
                }
            });

            void PlayDialogue()
            {
                if (script.ShowOneRandomLine && script.Count > 0)
                    currentIdx = UnityEngine.Random.Range(0, script.Count);
                else
                    currentIdx++;


                if (script.TargetStyle == DialogueStyle.ChatBubble)
                {
                    var line = script[currentIdx];
                    var dialogueText = line.Dialogue;

                    if (_gameAssetLibrary.TryGetCharacterInfo(Character.Player, out var playerInfo))
                        dialogueText = dialogueText.Replace("{player}", playerInfo.Name, StringComparison.OrdinalIgnoreCase);
                    else
                        Debug.LogWarning("Player ������ �������� ���߱� ������ {player} ���ڿ��� ġȯ���� ���߽��ϴ�.");

                    var characterPosition = _getTransform(line.Character);

                    _bubbleDialogueUI
                        .Show(new BubbleContainer(_canvasTransform)
                        .With(dialogueText, characterPosition, BubbleOffset));
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

        public void Stop(bool closeDialogue = true)
        {

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
        }

        private void OnDestroy()
        {

            Stop();
            Destroying?.Invoke();
        }
    }
}