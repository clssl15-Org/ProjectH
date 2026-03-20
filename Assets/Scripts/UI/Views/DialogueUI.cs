using System;
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
        IDialogueUI,
        IInjectable<GameAssetLibrary>,
        IEnablable
    {
        [Header("UI Components")]
        [SerializeField] private Image _portraitUI;
        [SerializeField] private TextMeshProUGUI _nametagUI;
        [SerializeField] private TMPTypeHandler _typeHandler;
        [SerializeField] private TMP_InputField _inputField;

        public bool IsInputMode
        {
            get => _isInputMode;
            set
            {
                if (value == _isInputMode) return;
                _isInputMode = value;

                if (_inputField)
                    _inputField.text = string.Empty;

                _typeHandler.gameObject.SetActive(!value);
                _inputField.gameObject.SetActive(value);
            }
        }
        public string InputText => _inputField.text;

        public bool IsTotallyTyped => _typeHandler.IsTotallyTyped;

        public event Action TextTyped;
        public event Action Disabling;

        #region Interfaces
        Action IEnablable.OnEnabling => null;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () => Disabling?.Invoke();
        Action IEnablable.OnDisabled => null;
        #endregion

        private GameAssetLibrary _gameAssetLibrary;
        private EnableWithAnimation _enabler;
        private bool _isInputMode;
        private bool _isAwake;

        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Awake, wasAwake: {_isAwake}");

            if (_isAwake) return;
            _isAwake = true;

            _enabler = new EnableWithAnimation(GetComponent<Animation>(), gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            _typeHandler.TextTyped += () => TextTyped?.Invoke();

            BlackboxHandle.Of(this).Write("Set To Disabled");
            SetToDisabled();
        }

        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary) => _gameAssetLibrary = gameAssetLibrary;

        public void SetContent(DialogueLine dialogueData)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Content: {dialogueData.Character}");
            GetData(dialogueData, out var name, out var portrait, out var dialogueText);

            _nametagUI.text = name;
            _portraitUI.sprite = portrait;

            if (_gameAssetLibrary.TryGetCharacterInfo(Character.Player, out var playerInfo))
                dialogueText = dialogueText.Replace("{player}", playerInfo.Name, StringComparison.OrdinalIgnoreCase);
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "Player 정보를 가져오지 못했기 때문에 {player} 문자열을 치환하지 못했습니다."));

            _typeHandler.TypeDialogue(dialogueText);
        }

        public void SkipTyping() => _typeHandler.SkipTyping();

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

            _typeHandler.SkipTyping();
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
