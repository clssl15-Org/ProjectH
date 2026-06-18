using System;
using System.Linq;
using Infrastructure;
using UI.RelicInfoPanelView;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RelicInfoPanelUI : MonoBehaviour,
        IStandaloneUpdatable,
        IEnablable,
        IInputLayerController,
        ICursorVisibilityControllerUser,
        IInjectable<GameAssetLibrary>,
        IInjectable<DarkscreenUI>
    {
        [field: SerializeField] public KeyCode OpenKey { get; set; } = KeyCode.Tab;
        [field: SerializeField] public KeyCode[] CloseKeys { get; set; } = new[] { KeyCode.Escape, KeyCode.Tab };
        [Space]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private RectTransform _relicListParent;
        [SerializeField] private RelicInfoUI _relicInfoPrefab;
        [SerializeField] private Animation _animation;

        public event Action Destroying;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {

            Time.timeScale = 0f;
            transform.SetAsLastSibling();
            _cursorVisibilityController?.RequestVisible(this);

            if (_darkscreenUI)
            {
                _darkscreenUI.EnableFor(this, ((IEnablable)this).Disable);
            }

            if (_inputHub != null)
            {
                _inputHub.Block(this);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            Time.timeScale = 1f;
            _cursorVisibilityController?.ReleaseVisible(this);

            if (_darkscreenUI)
            {
                _darkscreenUI.Disable();
            }

            if (_inputHub != null)
            {
                _inputHub.Unblock(this);
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion

        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;
        private CursorVisibilityController _cursorVisibilityController;
        private EmptyInputSubject _openerSubject;
        private GameAssetLibrary _gameAssetLibrary = null!;
        private DarkscreenUI _darkscreenUI;
        private bool _isInitialized = false;


        // Content
        private void Awake() => Initialize();
        public void Initialize()
        {

            if (_isInitialized) return;
            _isInitialized = true;

            if (!_closeBtn)
                throw new InvalidOperationException(Ctx($"{nameof(_closeBtn)} 컴포넌트가 유효하지 않습니다."));

            if (!_relicListParent)
                throw new InvalidOperationException(Ctx($"{nameof(_relicListParent)} 컴포넌트가 유효하지 않습니다."));

            if (!_relicInfoPrefab)
                throw new InvalidOperationException(Ctx($"{nameof(_relicInfoPrefab)} 컴포넌트가 유효하지 않습니다."));

            if (!_animation)
                throw new InvalidOperationException(Ctx($"{nameof(_animation)} 컴포넌트가 유효하지 않습니다."));


            _relicInfoPrefab.gameObject.SetActive(false);

            _openerSubject = new EmptyInputSubject(nameof(RelicInfoPanelUI), true);

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            _closeBtn.onClick.AddListener(Close);

            ((IEnablable)this).SetToDisabled();
        }

        public EmptyInputSubject GetOpenerSubject()
        {
            Initialize();
            return _openerSubject;
        }

        void IInputLayerController.Initialize(IInputHub inputHub) => _inputHub = inputHub;
        void ICursorVisibilityControllerUser.Initialize(CursorVisibilityController cursorVisibilityController) =>
            _cursorVisibilityController = cursorVisibilityController;
        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary) => _gameAssetLibrary = gameAssetLibrary;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;

        public void Open()
        {

            if (RelicManager.Instance)
            {
                var childCount = _relicListParent.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                    Destroy(_relicListParent.GetChild(i).gameObject);

                foreach (var relicId in RelicManager.Instance.OwnedRelics.Keys)
                {
                    if (!RelicManager.Instance.TryGetRelicData(relicId, out var relicData))
                    {
                        Debug.LogWarning(Ctx(
                            $"{nameof(RelicManager.Instance)}에서 {nameof(relicId)} '{relicId}'을(를) 가지는 렐릭을 찾지 못했습니다. " +
                            $"해당 렐릭은 목록에 표시되지 않습니다."),
                            this);

                        continue;
                    }

                    if (relicData.HiddenFromRelicUI)
                        continue;

                    var relicInfo = Instantiate(_relicInfoPrefab);

                    // 런타임 설명으로 가져오기
                    var description = RelicManager.RelicDescriptionRegistry.TryGetValue(relicId, out var desc)
                        ? desc
                        : relicData.Description;

                    relicInfo.Initialize(relicData.Icon, relicData.RelicName, FormatUiText(description));

                    relicInfo.GetComponent<RectTransform>().SetParent(_relicListParent, false);
                    relicInfo.name = _relicInfoPrefab.name + $" {relicData.RelicName}";
                    relicInfo.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning(Ctx($"{nameof(RelicManager.Instance)}이(가) 유효하지 않습니다. 올바르지 않은 렐릭 목록이 표시될 수 있습니다."), this);
            }

            ((IEnablable)this).Enable();
        }
        public void Close()
        {
            ((IEnablable)this).Disable();
        }

        void IStandaloneUpdatable.StandaloneUpdate()
        {
            if (!_enabler.IsEnabled)
            {
                if (_openerSubject.AllowInput && Input.GetKeyDown(OpenKey))
                    Open();
            }
            else
            {
                if (CloseKeys.Any(Input.GetKeyDown))
                    Close();
            }
        }

        void IEnablable.Enable()
        {
            Initialize();
            _enabler.Enable();
        }
        void IEnablable.Disable()
        {
            Initialize();
            _enabler.Disable();
        }
        void IEnablable.SetToEnabled()
        {
            Initialize();
            _enabler.SetToEnabled();
        }
        void IEnablable.SetToDisabled()
        {
            Initialize();
            _enabler.SetToDisabled();
        }

        private void OnDestroy()
        {
            _cursorVisibilityController?.ReleaseVisible(this);
            Destroying?.Invoke();
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(RelicInfoPanelUI)}] {message}";
        private string FormatUiText(string text) => UiRichTextFormatter.ApplyHighlights(text, _gameAssetLibrary);
    }

    public static class UiRichTextFormatter
    {
        public const string RedHighlightOpenTag = "<ui-red>";
        public const string RedHighlightCloseTag = "</ui-red>";

        public static string ApplyHighlights(string text, GameAssetLibrary gameAssetLibrary)
        {
            var blueColor = gameAssetLibrary
                ? gameAssetLibrary.UiBlueHighlightColor
                : GameAssetLibrary.DefaultUiBlueHighlightColor;
            var redColor = gameAssetLibrary
                ? gameAssetLibrary.UiRedHighlightColor
                : GameAssetLibrary.DefaultUiRedHighlightColor;

            return ApplyHighlights(text, blueColor, redColor);
        }

        public static string ApplyHighlights(string text, Color blueColor, Color redColor)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace(RelicManager.BlueHighlightOpenTag, ToColorOpenTag(blueColor))
                .Replace(RelicManager.BlueHighlightCloseTag, "</color>")
                .Replace(RedHighlightOpenTag, ToColorOpenTag(redColor))
                .Replace(RedHighlightCloseTag, "</color>");
        }

        private static string ToColorOpenTag(Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>";
    }
}
