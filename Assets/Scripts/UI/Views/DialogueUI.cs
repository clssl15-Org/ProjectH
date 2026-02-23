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
                Debug.LogError(BlackboxHandle.Of(this).WriteMessage(
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
                Debug.LogError(BlackboxHandle.Of(this).WriteMessage(
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


        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Awake, was: {_isAwaked}");

            if (_isAwaked) return;
            _isAwaked = true;

            _enabler = new EnableWithAnimation(GetComponent<Animation>(), gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            BlackboxHandle.Of(this).Write("Set To Disabled");
            SetToDisabled();
        }

        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary) => _gameAssetLibrary = gameAssetLibrary;
        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

        public void SetContent(DialogueLine dialogueData)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Content: {dialogueData.Character}");
            GetData(dialogueData, out var name, out var portrait, out var dialogue);

            _nametagUI.text = name;
            _portraitUI.sprite = portrait;
            _dialogueUI.text = dialogue;
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabled");
            Start();

            BlackboxHandle.Of(this).Exert(_enabler, "Enable");
            _enabler.Enable();
        }
        public void Disable()
        {
            Start();
            _enabler.Disable();
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
