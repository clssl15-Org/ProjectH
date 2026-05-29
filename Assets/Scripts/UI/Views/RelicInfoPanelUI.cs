using System;
using System.Linq;
using BlackboxSystem;
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");

            Time.timeScale = 0f;
            transform.SetAsLastSibling();
            _cursorVisibilityController?.RequestVisible(this);

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, "Enable");
                _darkscreenUI.EnableFor(this, ((IEnablable)this).Disable);
            }

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "Block All");
                _inputHub.Block(this);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Disabling");
            Time.timeScale = 1f;
            _cursorVisibilityController?.ReleaseVisible(this);

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, "Disable");
                _darkscreenUI.Disable();
            }

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "Unblock All");
                _inputHub.Unblock(this);
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion

        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;
        private CursorVisibilityController _cursorVisibilityController;
        private EmptyInputSubject _openerSubject;
        private DarkscreenUI _darkscreenUI;
        private bool _isInitialized = false;


        // Content
        private void Awake() => Initialize();
        public void Initialize()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize, wasInitialized: {_isInitialized}");

            if (_isInitialized) return;
            _isInitialized = true;

            if (!_closeBtn)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    Ctx($"{nameof(_closeBtn)} ????????? ??????? ??????.")));

            if (!_relicListParent)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    Ctx($"{nameof(_relicListParent)} ????????? ??????? ??????.")));

            if (!_relicInfoPrefab)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    Ctx($"{nameof(_relicInfoPrefab)} ????????? ??????? ??????.")));

            if (!_animation)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    Ctx($"{nameof(_animation)} ????????? ??????? ??????.")));


            _relicInfoPrefab.gameObject.SetActive(false);

            _openerSubject = new EmptyInputSubject(nameof(RelicInfoPanelUI), true);

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            _closeBtn.onClick.AddListener(Close);

            BlackboxHandle.Of(this).Write("Set To Disable");
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
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;

        public void Open()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Open");

            if (RelicManager.Instance)
            {
                var childCount = _relicListParent.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                    Destroy(_relicListParent.GetChild(i).gameObject);

                foreach (var relicId in RelicManager.Instance.OwnedRelics.Keys)
                {
                    if (!RelicManager.Instance.TryGetRelicData(relicId, out var relicData))
                    {
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                            $"{nameof(RelicManager.Instance)}???? {nameof(relicId)} '{relicId}'??(??) ?????? ?????? ??? ????????. " +
                            $"??? ?????? ???? ?????? ??????.")),
                            this);

                        continue;
                    }

                    if (relicData.HiddenFromRelicUI)
                        continue;

                    var relicInfo = Instantiate(_relicInfoPrefab);
                    BlackboxHandle.Of(this).Write($"Add: {relicData.RelicName}");

                    // ????? ???????? ????????
                    var description = RelicManager.RelicDescriptionRegistry.TryGetValue(relicId, out var desc)
                        ? desc
                        : relicData.Description;

                    relicInfo.Initialize(relicData.Icon, relicData.RelicName, description);

                    relicInfo.GetComponent<RectTransform>().SetParent(_relicListParent, false);
                    relicInfo.name = _relicInfoPrefab.name + $" {relicData.RelicName}";
                    relicInfo.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    Ctx($"{nameof(RelicManager.Instance)}??(??) ??????? ??????. ?????? ???? ???? ????? ???? ?? ??????.")), this);
            }

            ((IEnablable)this).Enable();
        }
        public void Close()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Close");
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
    }
}
