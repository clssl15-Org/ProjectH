using System;
using BlackboxSystem;
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
        IInjectable<DarkscreenUI>
    {
        [SerializeField] private bool _multiDisplayMode = false;
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");

            Time.timeScale = 0f;
            transform.SetAsLastSibling();

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
        private DarkscreenUI _darkscreenUI;
        private bool _isInitialized = false;


        // Content
        private void Awake() => Initialize();
        public void Initialize()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize, wasInitialized: {_isInitialized}");

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

            BlackboxHandle.Of(this).Write("Set To Disable");
            ((IEnablable)this).SetToDisabled();
        }

        void IInputLayerController.Initialize(IInputHub inputHub) => _inputHub = inputHub;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;


        public void Open(bool multiDisplayMode)
        {
            _multiDisplayMode = multiDisplayMode;
            Open();
        }
        public void Open()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Open, mode: {_multiDisplayMode}");

            if (!_multiDisplayMode)
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Close");
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
#endif
        }
    }
}
