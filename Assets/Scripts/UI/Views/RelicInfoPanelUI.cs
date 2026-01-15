using System;
using BlackboxSystem;
using Infrastructure;
using UI.RelicInfoPanelView;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    public class RelicInfoPanelUI : MonoBehaviour,
        IStandaloneUpdatable, IEnablable, IInputController
    {
        [field: SerializeField] public KeyCode OpenKey { get; set; } = KeyCode.Tab;
        [Space]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private RectTransform _relicListParent;
        [SerializeField] private RelicInfoUI _relicInfoPrefab;
        [SerializeField] private Animation _animation;

        public event Action Destroyed;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "BlockAll");
                _inputHub.BlockAll();
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "UnblockAll");
                _inputHub.UnblockAll();
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion

        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;
        private bool _initialized = false;


        private void Awake() => Initialize();
        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            if (!_closeBtn)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_closeBtn)} 컴포넌트가 유효하지 않습니다.")));

            if (!_relicListParent)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_relicListParent)} 컴포넌트가 유효하지 않습니다.")));

            if (!_relicInfoPrefab)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_relicInfoPrefab)} 컴포넌트가 유효하지 않습니다.")));

            if (!_animation)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_animation)} 컴포넌트가 유효하지 않습니다.")));


            _relicInfoPrefab.gameObject.SetActive(false);

            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);

            _closeBtn.onClick.AddListener(Close);
            ((IEnablable)this).SetToDisabled();
        }

        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

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
                        Debug.LogWarning(BlackboxHandle.Of(this).Write(Ctx(
                            $"{nameof(RelicManager.Instance)}에서 {nameof(relicId)} '{relicId}'을(를) 가지는 렐릭을 찾지 못했습니다. " +
                            $"해당 렐릭은 목록에 표시되지 않습니다.")),
                            this);

                        continue;
                    }

                    var relicInfo = Instantiate(_relicInfoPrefab);
                    BlackboxHandle.Of(this).Write($"Add: {relicData.RelicName}");

                    relicInfo.Initialize(relicData.Icon, relicData.RelicName, relicData.Description);
                    relicInfo.GetComponent<RectTransform>().SetParent(_relicListParent);
                    relicInfo.name = _relicInfoPrefab.name + $" {relicData.RelicName}";
                    relicInfo.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning(BlackboxHandle.Of(this).Write(
                    Ctx($"{nameof(RelicManager.Instance)}이(가) 유효하지 않습니다. 올바르지 않은 렐릭 목록이 표시될 수 있습니다.")), this);
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
            if (OpenKey != KeyCode.None && Input.GetKeyDown(OpenKey))
                Open();
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

        private string Ctx(string message) => $"[{nameof(RelicInfoPanelUI)}] {message}";

        private void OnDestroy()
        {
            Destroyed?.Invoke();
            _enabler?.Dispose();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(RelicInfoPanelUI))]
        private class RelicInfoPanelUIEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (Application.isPlaying && GUILayout.Button("Export Log"))
                {
                    GUILayout.Space(8);
                    BlackboxHandle.Of(target).Export(openLog: true);
                }
            }
        }
#endif
    }
}
