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
        IEnablable
    {
        [SerializeField] private Image _portraitUI;
        [SerializeField] private TextMeshProUGUI _nametagUI;
        [SerializeField] private TextMeshProUGUI _dialogueUI;

        public event Action Disabling;

        #region Interfaces
        Action IEnablable.OnEnabling => null;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () => Disabling?.Invoke();
        Action IEnablable.OnDisabled => null;
        #endregion

        private GameAssetLibrary _gameAssetLibrary;
        private EnableWithAnimation _enabler;
        private bool _isAwake = false;


        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Awake, was: {_isAwake}");

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
            using var _ = BlackboxHandle.Of(this).WriteScope("Enable");
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
