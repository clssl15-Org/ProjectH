using System;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UI.RelicInfoPanelView;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RelicAcquisitionUI : MonoBehaviour,
        IStandaloneInitializable, IView, IEnablable, IInputController
    {
        [Header("Main")]
        [SerializeField] private Animation _openAnimation;

        [Header("Relic")]
        [SerializeField] private GameObject _relicPage;
        [SerializeField] private Image _relicIcon;
        [SerializeField] private TextMeshProUGUI _relicNametag;
        [SerializeField] private TextMeshProUGUI _relicDescrption;
        [SerializeField] private Button _toThrowCoinBtn;
        [SerializeField] private Button _closeBtn;

        [Header("Throw Coin")]
        [SerializeField] private GameObject _coinPage;
        [SerializeField] private TextMeshProUGUI _coinDescripton;
        [SerializeField] private GameObject _coinImage;
        [SerializeField] private GameObject _coinAnimation;
        [SerializeField] private VideoPlayer _coinRawVideoPlayer;
        [SerializeField] private VideoPlayer _coinMaskVideoPlayer;

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

        private bool _isInitialized = false;
        private bool _isOperating = false;
        private bool _isDestroyed = false;

        private readonly bool UseCoinReadyImage = false;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            _relicPage.SetActive(true);
            _coinPage.SetActive(false);

            _toThrowCoinBtn.gameObject.SetActive(true);
            _closeBtn.gameObject.SetActive(false);

            _inputHub?.BlockAll();
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () => _inputHub?.UnblockAll();
        Action IEnablable.OnDisabled => null;
        #endregion



        void IStandaloneInitializable.StandaloneInitialize() => Start();
        private void Start()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _toThrowCoinBtn.onClick.AddListener(ToThrowCoin);
            _closeBtn.onClick.AddListener(Close);

            RelicManager.Instance.RelicAcquiring += OnRelicAcquiring;

            _enabler = new EnableWithAnimation(_openAnimation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);
            SetToDisabled();

            if (!UseCoinReadyImage)
            {
                _coinImage.SetActive(false);
                _coinAnimation.SetActive(true);
            }
        }

        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

        private void OnRelicAcquiring(RelicDataSO relicInfo)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Relic Acquiring: {relicInfo.name}");

            if (_isOperating)
            {
                Debug.LogWarning(
                    $"{nameof(RelicAcquisitionUI)} 이미 선행 작업이 진행 중이므로 새로운 렐릭을 얻을 수 없습니다.",
                    this);

                return;
            }

            _isOperating = true;
            _relic = relicInfo;

            Enable();

            _relicIcon.sprite = relicInfo.Icon;
            _relicNametag.text = relicInfo.RelicName;
            _relicDescrption.text = relicInfo.Description;

            _coinRawVideoPlayer.clip = null;
            _coinMaskVideoPlayer.clip = null;
        }

        private void ToThrowCoin()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Throw Coin");

            _relicPage.SetActive(false);
            _coinPage.SetActive(true);

            if (UseCoinReadyImage)
            {
                _coinImage.SetActive(true);
                _coinAnimation.SetActive(false);
            }
            else
                _coinAnimation.SetActive(true);


            _coinDescripton.text = $"강화 성공 시 능력치 {_relic.BaseValue} → {_relic.CoinFlipValue}";

            var reinforced = RelicManager.Instance.StartCoinRandom(_relic.RelicNumber);

            var clip = _videoClips.FirstOrDefault(v => v.IsFront == reinforced);
            if (!clip.Video || !clip.AlphaMask)
                throw new InvalidOperationException(
                    $"[{nameof(RelicAcquisitionUI)}] {nameof(clip)}이(가) 유효하지 않습니다.");

            _coinRawVideoPlayer.playbackSpeed = 0f;
            _coinMaskVideoPlayer.playbackSpeed = 0f;

            _coinRawVideoPlayer.clip = clip.Video;
            _coinMaskVideoPlayer.clip = clip.AlphaMask;

            _coinRawVideoPlayer.frame = 0;
            _coinMaskVideoPlayer.frame = 0;


            _updater = Loco.Subscribe(() =>
            {
                if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) <= 0.0001f)
                    return;

                ThrowCoin();
                _updater.Dispose();
            });

            void ThrowCoin()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Throw Coin");

                if (UseCoinReadyImage)
                {
                    _coinImage.SetActive(false);
                    _coinAnimation.SetActive(true);
                }

                _coinRawVideoPlayer.playbackSpeed = 1f;
                _coinMaskVideoPlayer.playbackSpeed = 1f;

                _timer = new Timer((float)clip.Video.length, succeeded =>
                {
                    using var _ = BlackboxHandle.Of(this).WriteScope("Play Ended");

                    if (succeeded)
                    {
                        BlackboxHandle.Of(this).Exert(RelicManager.Instance, "Add Relic");
                        RelicManager.Instance.AddRelic(_relic.RelicNumber, reinforced);

                        _relicDescrption.text =
                            $"강화 {(reinforced ? "성공" : "실패")}\n\n" +
                            _relicDescrption.text;

                        _toThrowCoinBtn.gameObject.SetActive(false);
                        _closeBtn.gameObject.SetActive(true);

                        _coinPage.SetActive(false);
                        _relicPage.SetActive(true);
                    }
                    else
                        BlackboxHandle.Of(this).Exert(RelicManager.Instance, "Play Failed");
                });
            }
        }

        private void Close()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Close");

            _updater?.Dispose();
            _updater = null;

            _timer?.Dispose();
            _timer = null;

            Disable();
            _isOperating = false;
        }

        public void Enable()
        {
            Start();
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


#if UNITY_EDITOR
        [CustomEditor(typeof(RelicAcquisitionUI))]
        private class RelicAcquisitionUIEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (Application.isPlaying)
                {
                    GUILayout.Space(8);
                    if (GUILayout.Button("Export Log"))
                        BlackboxHandle.Of(target).Export(openLogOption: OpenLogOption.Open);
                }
            }
        }
#endif
    }
}
