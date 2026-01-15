using System;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RelicAcquisitionUI : MonoBehaviour,
        IView, IStandaloneInitializable, IEnablable, IInputController
    {
        [Header("Main")]
        [SerializeField] private Animation _animation;

        [Header("Relic")]
        [SerializeField] private Image _relicIcon;
        [SerializeField] private TextMeshProUGUI _relicNametag;
        [SerializeField] private TextMeshProUGUI _relicDescrption;
        [SerializeField] private Button _toThrowCoinBtn;
        [SerializeField] private Button _closeBtn;

        [Header("Throw Coin")]
        [SerializeField] private GameObject _throwCoin;
        [SerializeField] private TextMeshProUGUI _coinDescripton;
        [SerializeField] private VideoPlayer _coinRawVideoPlayer;
        [SerializeField] private VideoPlayer _coinMaskVideoPlayer;

#if UNITY_EDITOR
        [Header("Log")]
        [SerializeField] private bool _exportLog = false;
#endif

        public bool EnableInput { get; set; } = true;
        public event Action Destroyed;

        [Serializable]
        public struct VideoData
        {
            public bool IsFront;
            public VideoClip Video;
            public VideoClip AlphaMask;
        }
        [SerializeField] private VideoData[] _videoClips;

        private IInputHub _inputHub;
        private IDisposable _updater, _timer;
        private RelicDataSO _relic;
        private EnableWithAnimation _enabler;

        private bool _initialized = false;
        private bool _operating = false;
        private bool _isDestroyed = false;

        #region Interfaces
        Action IEnablable.OnEnabling => () => _inputHub?.BlockAll();
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () => _inputHub?.UnblockAll();
        Action IEnablable.OnDisabled => null;
        #endregion


        private void Awake() => ((IStandaloneInitializable)this).StandaloneInitialize();
        void IStandaloneInitializable.StandaloneInitialize()
        {
            if (_initialized) return;
            _initialized = true;

            _toThrowCoinBtn.onClick.AddListener(ToThrowCoin);
            _closeBtn.onClick.AddListener(Close);

            RelicManager.Instance.RelicAcquiring += OnRelicAcquiring;

            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);
            SetToDisabled();
        }

        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

        private void OnRelicAcquiring(RelicDataSO relicInfo)
        {
            if (_operating)
            {
                Debug.LogWarning(
                    $"{nameof(RelicAcquisitionUI)} 이미 선행 작업이 진행 중이므로 새로운 렐릭을 얻을 수 없습니다.",
                    this);

                return;
            }
            _operating = true;

            _relic = relicInfo;

            Enable();

            _toThrowCoinBtn.gameObject.SetActive(true);
            _closeBtn.gameObject.SetActive(false);

            _relicIcon.sprite = relicInfo.Icon;
            _relicNametag.text = relicInfo.RelicName;
            _relicDescrption.text = relicInfo.Description;

            _coinRawVideoPlayer.clip = null;
            _coinMaskVideoPlayer.clip = null;
        }

        private void ToThrowCoin()
        {
            _throwCoin.SetActive(true);
            _coinDescripton.text = $"강화 성공 시 능력치 {_relic.BaseValue} → {_relic.CoinFlipValue}";

            var reinforced = RelicManager.Instance.StartCoinRandom(_relic.RelicNumber);
            print($"강화 여부: {(reinforced ? "성공" : "실패")}");

            var clip = _videoClips.FirstOrDefault(v => v.IsFront == reinforced);
            if (!clip.Video || !clip.AlphaMask)
                throw new InvalidOperationException(
                    $"[{nameof(RelicAcquisitionUI)}] {nameof(clip)}이(가) 유효하지 않습니다.");

            _coinRawVideoPlayer.playbackSpeed = 0f;
            _coinMaskVideoPlayer.playbackSpeed = 0f;

            _coinRawVideoPlayer.clip = clip.Video;
            _coinMaskVideoPlayer.clip = clip.AlphaMask;


            _updater = Loco.Subscribe(() =>
            {
                if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) <= 0.0001f)
                    return;

                ThrowCoin();
                _updater.Dispose();
            });

            void ThrowCoin()
            {
                _coinRawVideoPlayer.playbackSpeed = 1f;
                _coinMaskVideoPlayer.playbackSpeed = 1f;

                _timer = new Timer((float)clip.Video.length, succeeded =>
                {
                    if (succeeded)
                    {
                        RelicManager.Instance.AddRelic(_relic.RelicNumber, reinforced);

                        _relicDescrption.text =
                            $"강화 {(reinforced ? "성공" : "실패")}\n\n" +
                            _relicDescrption.text;

                        _toThrowCoinBtn.gameObject.SetActive(false);
                        _closeBtn.gameObject.SetActive(true);

                        _throwCoin.SetActive(false);
                    }
                });
            }
        }

#if UNITY_EDITOR
        private bool _logExported = false;
        private void Update()
        {
            if (!_exportLog)
                return;

            if (!_logExported
                && Input.GetKey(KeyCode.LeftControl)
                && Input.GetKey(KeyCode.RightControl))
            {
                _logExported = true;
                BlackboxHandle.Of(this).Export(openLog: true);
            }
        }
#endif

        private void Close()
        {
            _updater?.Dispose();
            _updater = null;

            _timer?.Dispose();
            _timer = null;

            Disable();
            _operating = false;
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        public void SetParent(RectTransform parent) =>
            GetComponent<RectTransform>().SetParent(parent);

        private void OnDestroy() => Destroy();
        public void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            _enabler?.Dispose();
            Destroyed?.Invoke();

            _updater?.Dispose();
            _timer?.Dispose();

            RelicManager.Instance.RelicAcquiring -= OnRelicAcquiring;
        }
    }
}
