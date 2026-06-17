using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    public class GuideAndWorldRecordsUI : MonoBehaviour,
        IEnablable,
        IInputLayerController,
        ICursorVisibilityControllerUser,
        IInjectable<GameServices>,
        IInjectable<DarkscreenUI>
    {
        [SerializeField] private bool _overrideMultiDisplayMode;
        [SerializeField] private bool _multiDisplayMode;
        [Space]
        [SerializeField] private Button _backBtn;
        [SerializeField] private Animation _animation;
        [Space]
        [SerializeField] private GameObject _singleGuide;
        [SerializeField] private GameObject _multiGuide;
        [SerializeField] private Button _ToMultiRecordBtn;
        [SerializeField] private GameObject _multiRecords;
        [SerializeField] private Button _ToMultiGuideBtn;

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

        private bool MultiDisplayMode => !_overrideMultiDisplayMode
            ? _gameServices.IsStage3Reached : _multiDisplayMode;

        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;
        private CursorVisibilityController _cursorVisibilityController;
        private GameServices _gameServices;
        private DarkscreenUI _darkscreenUI;
        private bool _isInitialized = false;


        // Content
        private void Awake() => Initialize();
        public void Initialize()
        {

            if (_isInitialized) return;
            _isInitialized = true;

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            _backBtn.onClick.AddListener(Close);

            _ToMultiRecordBtn.onClick.AddListener(() =>
            {
                _multiGuide.SetActive(false);
                _multiRecords.SetActive(true);
            });

            _ToMultiGuideBtn.onClick.AddListener(() =>
            {
                _multiGuide.SetActive(true);
                _multiRecords.SetActive(false);
            });

            ((IEnablable)this).SetToDisabled();
        }

        void IInputLayerController.Initialize(IInputHub inputHub) => _inputHub = inputHub;
        void ICursorVisibilityControllerUser.Initialize(CursorVisibilityController cursorVisibilityController) =>
            _cursorVisibilityController = cursorVisibilityController;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;
        void IInjectable<GameServices>.Inject(GameServices gameServices) => _gameServices = gameServices;


        public void Open()
        {

            if (!MultiDisplayMode)
            {
                _singleGuide.SetActive(true);
                _multiGuide.SetActive(false);
                _multiRecords.SetActive(false);
            }
            else
            {
                _singleGuide.SetActive(false);
                _multiGuide.SetActive(true);
                _multiRecords.SetActive(false);
            }

            ((IEnablable)this).Enable();
        }

        public void Close()
        {
            ((IEnablable)this).Disable();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
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
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(GuideAndWorldRecordsUI)}] {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(GuideAndWorldRecordsUI))]
        private class GuideAndWorldRecordsUIEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space(8);

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Open"))
                        ((GuideAndWorldRecordsUI)target).Open();
                }
                else
                    GUILayout.Label("Enter play mode to open UI", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
