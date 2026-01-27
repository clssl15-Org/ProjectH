using System;
using BlackboxSystem;
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
        IEnablable,
        IInputController
    {
        [SerializeField] private Image _portraitUI;
        [SerializeField] private TextMeshProUGUI _nametagUI;
        [SerializeField] private TextMeshProUGUI _dialogueUI;

        public event Action Disabling;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            if (_inputHub == null)
            {
                Debug.LogError(BlackboxHandle.Of(this).Write(
                    $"(OnEnabling) '{nameof(_inputHub)}'이(가) 유효하지 않기 때문에 Input 설정을 변경할 수 없습니다."),
                    this);
                return;
            }

            BlackboxHandle.Of(this).Exert(_inputHub, "(OnEnabling) Block All Inputs");
            _inputHub.BlockAll();
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            if (_inputHub == null)
            {
                Debug.LogError(BlackboxHandle.Of(this).Write(
                    $"(OnEnabling) '{nameof(_inputHub)}'이(가) 유효하지 않기 때문에 Input 설정을 변경할 수 없습니다."),
                    this);
                return;
            }

            BlackboxHandle.Of(this).Exert(_inputHub, "(OnDisabling) Unblock All Inputs");
            _inputHub.UnblockAll();

            Disabling?.Invoke();
        };
        Action IEnablable.OnDisabled => null;
        #endregion

        private GameAssetLibrary _gameAssetLibrary;
        private IInputHub _inputHub;
        private EnableWithAnimation _enabler;
        private bool _isAwaked = false;


        private void Awake()
        {
            if (_isAwaked) return;
            _isAwaked = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _enabler = new EnableWithAnimation(GetComponent<Animation>())
                .InitializeWithIEnablable(this);
        }
        private void Start() => SetToDisabled();

        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary)
        {
            _gameAssetLibrary = gameAssetLibrary;
        }
        void IInputController.Initialize(IInputHub inputHub)
        {
            _inputHub = inputHub;
        }

        public void SetContent(DialogueData dialogueData)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Content: {dialogueData.Character}");
            GetData(dialogueData, out var name, out var portrait, out var dialogue);

            _nametagUI.text = name;
            _portraitUI.sprite = portrait;
            _dialogueUI.text = dialogue;
        }

        private void GetData(
            DialogueData dialogueData,
            out string name,
            out Sprite portrait,
            out string dialogue)
        {
#pragma warning disable IDE0029
            if (!_gameAssetLibrary.TryGetCharacterInfo(dialogueData.Character, out var characterInfo))
            {
                throw new ArgumentException(BlackboxHandle.Of(this).CrashExport(
                    $"{nameof(dialogueData)}의 캐릭터 타입 '{dialogueData.Character}'에 해당하는 {nameof(CharacterInfoSO)}을(를) 찾지 못하였습니다."),
                    nameof(dialogueData));
            }

            name = !string.IsNullOrEmpty(dialogueData.NameOverride) ? dialogueData.NameOverride : characterInfo.Name;
            portrait = dialogueData.PortraitOverride != null ? dialogueData.PortraitOverride : characterInfo.Portrait;
            dialogue = dialogueData.Dialogue;
#pragma warning restore
        }


        public void Enable()
        {
            Awake();
            _enabler.Enable();
        }
        public void Disable()
        {
            Awake();
            _enabler.Disable();
        }
        public void SetToEnabled()
        {
            Awake();
            _enabler.SetToEnabled();
        }
        public void SetToDisabled()
        {
            Awake();
            _enabler.SetToDisabled();
        }
    }
}
