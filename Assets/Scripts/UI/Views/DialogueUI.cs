using System;
using System.Collections;
using BlackboxSystem;
using Dialogue;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI
{
    [RequireComponent(typeof(Animation))]
    public class DialogueUI : MonoBehaviour,
        IInjectable<GameAssetLibrary>,
        IEnablable
    {
        [Header("UI Components")]
        [SerializeField] private Image _portraitUI;
        [SerializeField] private TextMeshProUGUI _nametagUI;
        [SerializeField] private TextMeshProUGUI _dialogueUI;

        [Header("Settings")]
        [SerializeField, Min(0)] private float _typingSpeed = 0.1f;

        public bool IsTotallyTyped => _typingCoroutine == null;

        public event Action OnTextTyped;
        public event Action Disabling;

        #region Interfaces
        Action IEnablable.OnEnabling => null;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () => Disabling?.Invoke();
        Action IEnablable.OnDisabled => null;
        #endregion

        private GameAssetLibrary _gameAssetLibrary;
        private EnableWithAnimation _enabler;
        private Coroutine _typingCoroutine;
        private bool _isAwake = false;

        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Awake, wasAwake: {_isAwake}");

            if (_isAwake) return;
            _isAwake = true;

            _enabler = new EnableWithAnimation(GetComponent<Animation>(), gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            BlackboxHandle.Of(this).Write("Set To Disabled");
            SetToDisabled();
        }

        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary) => _gameAssetLibrary = gameAssetLibrary;

        public void SetContent(DialogueLine dialogueData)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Content: {dialogueData.Character}");
            GetData(dialogueData, out var name, out var portrait, out var dialogue);

            _nametagUI.text = name;
            _portraitUI.sprite = portrait;

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeDialogue(dialogue));

            IEnumerator TypeDialogue(string textContent)
            {
                _dialogueUI.text = textContent;
                _dialogueUI.ForceMeshUpdate();

                var textInfo = _dialogueUI.textInfo;
                var totalVisibleCharacterCount = textInfo.characterCount;

                if (totalVisibleCharacterCount == 0)
                {
                    _typingCoroutine = null;
                    yield break;
                }

                int counter = 1;

                _dialogueUI.maxVisibleCharacters = counter;
                if (IsVisibleCharacter(0)) OnTextTyped?.Invoke();

                while (counter < totalVisibleCharacterCount)
                {
                    yield return new WaitForSeconds(_typingSpeed);

                    counter++;
                    _dialogueUI.maxVisibleCharacters = counter;

                    if (IsVisibleCharacter(counter - 1))
                        OnTextTyped?.Invoke();
                }

                _typingCoroutine = null;


                bool IsVisibleCharacter(int index)
                {
                    if (index >= textInfo.characterInfo.Length)
                        return false;

                    var info = textInfo.characterInfo[index];
                    return char.IsLetterOrDigit(info.character);
                }
            }
        }

        public void SkipTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _dialogueUI.maxVisibleCharacters = int.MaxValue;
        }

        private void GetData(
            DialogueLine dialogueData,
            out string name,
            out Sprite portrait,
            out string dialogue)
        {
            if (!_gameAssetLibrary.TryGetCharacterInfo(dialogueData.Character, out var characterInfo))
            {
                throw new ArgumentException(BlackboxHandle.Of(this).WriteError(
                    $"{nameof(dialogueData)}의 캐릭터 타입 '{dialogueData.Character}'에 해당하는 {nameof(CharacterInfoSO)}을(를) 찾지 못하였습니다."),
                    nameof(dialogueData));
            }

            name = !string.IsNullOrEmpty(dialogueData.NameOverride) ? dialogueData.NameOverride : characterInfo.Name;
            portrait = dialogueData.PortraitOverride != null ? dialogueData.PortraitOverride : characterInfo.Portrait;
            dialogue = dialogueData.Dialogue;
        }

        public void Enable()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Enable");
            Start();

            BlackboxHandle.Of(this).Exert(_enabler, "Enable");
            _enabler.Enable();
        }
        public void Disable()
        {
            Start();
            _enabler.Disable();

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
        }
        public void SetToEnabled()
        {
            Start();
            _enabler.SetToEnabled();
        }
        public void SetToDisabled()
        {
            Start();
            _enabler.SetToDisabled();
        }
    }
}
