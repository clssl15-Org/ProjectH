using System;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class RelicAcquisitionUI : MonoBehaviour,
        IStandaloneInitializable,
        IEnablable,
        IView,
        IInputLayerController,
        IInjectable<DarkscreenUI>
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
        [SerializeField] private GameObject _coinThrowMessage;
        [SerializeField] private TextMeshProUGUI _coinDescripton;
        [SerializeField] private GameObject _coinImage;
        [SerializeField] private GameObject _coinAnimation;
        [SerializeField] private GameObject _effectAnimation;
        [SerializeField] private VideoPlayer _coinRawVideoPlayer;
        [SerializeField] private VideoPlayer _coinMaskVideoPlayer;
        [SerializeField] private VideoPlayer _coinEffectVideoPlayer;
        [SerializeField] private VideoPlayer _coinEffectMaskVideoPlayer;
        [SerializeField, Range(MinCoinAnimationPlaySpeed, MaxCoinAnimationPlaySpeed)] private float _coinAnimationPlaySpeed = 1f;
        [SerializeField] private float _effectPlayTiming = 1f;
        [SerializeField, Max(0f)] private float _dropEventNotifyTiming = -2.6f;

        public event Action CoinThrown;
        public event Action CoinDropped;
        public event Action Disabling;
        public event Action Destroying;

        public enum VideoType
        {
            CoinFront,
            CoinBack,
            CoinEffect,
        }

        [Serializable]
        public struct VideoData
        {
            public VideoType VideoType;
            public VideoClip Video;
            public VideoClip AlphaMask;
        }
        [SerializeField] private VideoData[] _videoClips;

        private const float MinCoinAnimationPlaySpeed = 0.1f;
        private const float MaxCoinAnimationPlaySpeed = 10f;

        private IInputHub _inputHub;
        private DarkscreenUI _darkscreenUI;
        private IDisposable _updater, _coinTimer, _coinDropTimer, _effectTimer;
        private RelicDataSO _relic;
        private bool _forceSuccess;
        private EnableWithAnimation _enabler;

        private bool _isInitialized;
        private bool _isOperating;
        private bool _isOperated;
        private bool _isDestroyed;

        private readonly bool UseCoinReadyImage;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");
            Time.timeScale = 0f;

            _relicPage.SetActive(true);
            _coinPage.SetActive(false);

            _toThrowCoinBtn.gameObject.SetActive(true);
            _closeBtn.gameObject.SetActive(false);

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

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "Unblock All");
                _inputHub.Unblock(this);
            }

            Disabling?.Invoke();
        };
        Action IEnablable.OnDisabled => null;
        #endregion

        private float CoinAnimationPlaySpeed =>
            Mathf.Clamp(_coinAnimationPlaySpeed, MinCoinAnimationPlaySpeed, MaxCoinAnimationPlaySpeed);

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

            _coinAnimation.SetActive(false);
        }

        void IInputLayerController.Initialize(IInputHub inputHub) => _inputHub = inputHub;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;

        private void OnRelicAcquiring(RelicDataSO relicInfo, string description, bool forceSuccess = false)
        {
            if (!this)
            {
                // 정상적으로 구독 해제되지 않은 UI 처리
                Destroy();
                return;
            }

            using var _ = BlackboxHandle.Of(this).WriteScope($"Relic Acquiring: {relicInfo.name}");
            if (_darkscreenUI) _darkscreenUI.EnableFor(this, () => { if (_isOperated) Close(); });

            if (_isOperating)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    $"{nameof(RelicAcquisitionUI)} 이미 선행 작업이 진행 중이므로 새로운 렐릭을 얻을 수 없습니다."),
                    this);
                return;
            }

            _isOperating = true;
            _isOperated = false;
            _relic = relicInfo;
            _forceSuccess = forceSuccess;

            Enable();

            _relicIcon.sprite = relicInfo.Icon;
            _relicNametag.text = relicInfo.RelicName;
            _relicDescrption.text = description;

            _coinRawVideoPlayer.clip = null;
            _coinMaskVideoPlayer.clip = null;
            _coinEffectVideoPlayer.clip = null;
            _coinEffectMaskVideoPlayer.clip = null;
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

            _coinThrowMessage.SetActive(true);
            _effectAnimation.SetActive(false);


            _coinDescripton.text = $"<align=center><size=120%>강화 성공 시 능력치 {_relic.BaseValue} → {_relic.CoinFlipValue}</size></align>";
            var reinforced = _forceSuccess || RelicManager.Instance.StartCoinRandom(_relic.RelicNumber);

            var coinClip = _videoClips.FirstOrDefault(v => v.VideoType
                == (reinforced ? VideoType.CoinFront : VideoType.CoinBack));

            _coinRawVideoPlayer.playbackSpeed = 0f;
            _coinMaskVideoPlayer.playbackSpeed = 0f;

            _coinRawVideoPlayer.clip = coinClip.Video;
            _coinMaskVideoPlayer.clip = coinClip.AlphaMask;

            _coinRawVideoPlayer.frame = 0;
            _coinMaskVideoPlayer.frame = 0;

            if (reinforced)
            {
                var effectClip = _videoClips.FirstOrDefault(v => v.VideoType == VideoType.CoinEffect);

                _coinEffectVideoPlayer.playbackSpeed = 0f;
                _coinEffectMaskVideoPlayer.playbackSpeed = 0f;

                _coinEffectVideoPlayer.clip = effectClip.Video;
                _coinEffectMaskVideoPlayer.clip = effectClip.AlphaMask;

                _coinEffectVideoPlayer.frame = 0;
                _coinEffectMaskVideoPlayer.frame = 0;
            }

            _updater?.Dispose();
            _updater = Loco.Subscribe(() =>
            {
                if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) <= 0.001f)
                    return;

                _coinThrowMessage.SetActive(false);
                ThrowCoin();

                _updater.Dispose();
            });

            void ThrowCoin()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Throw Coin");
                var coinAnimationPlaySpeed = CoinAnimationPlaySpeed;

                if (UseCoinReadyImage)
                {
                    _coinImage.SetActive(false);
                    _coinAnimation.SetActive(true);
                }

                _coinRawVideoPlayer.playbackSpeed = coinAnimationPlaySpeed;
                _coinMaskVideoPlayer.playbackSpeed = coinAnimationPlaySpeed;

                CoinThrown?.Invoke();

                _effectTimer?.Dispose();
                _effectTimer = reinforced ?
                    new Timer(_effectPlayTiming / coinAnimationPlaySpeed, succeeded =>
                    {
                        BlackboxHandle.Of(this).Write($"Effect Ended, succeeded: {succeeded}");

                        _effectAnimation.SetActive(true);
                        _coinEffectVideoPlayer.playbackSpeed = 1f;
                        _coinEffectMaskVideoPlayer.playbackSpeed = 1f;
                    },
                    useAbsoluteTime: true)
                    : null;

                _coinTimer?.Dispose();
                _coinTimer = new Timer((float)coinClip.Video.length / coinAnimationPlaySpeed, succeeded =>
                {
                    using var _ = BlackboxHandle.Of(this).WriteScope($"Play Ended, succeeded: {succeeded}");
                    _isOperated = true;

                    if (succeeded)
                    {
                        BlackboxHandle.Of(this).Exert(RelicManager.Instance, "Add Relic");
                        RelicManager.Instance.AddRelic(
                            _relic.RelicNumber,
                            out var description,
                            reinforced);

                        if (reinforced)
                            _relicDescrption.text = $"<size=120%><color=#76ffff>강화 성공</color></size>\n\n{description}";
                        else
                            _relicDescrption.text = $"<size=120%><color=#FF0000>강화 실패</color></size>\n\n{description}";

                        _toThrowCoinBtn.gameObject.SetActive(false);
                        _closeBtn.gameObject.SetActive(true);

                        _coinPage.SetActive(false);
                        _relicPage.SetActive(true);
                    }
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).ExertMessage(
                            RelicManager.Instance,
                            "Play Failed"));
                },
                useAbsoluteTime: true);

                _coinDropTimer?.Dispose();

                var dropNotifyTimimg = ((float)coinClip.Video.length + _dropEventNotifyTiming) / coinAnimationPlaySpeed;
                _coinDropTimer = dropNotifyTimimg > 0
                    ? new Timer(dropNotifyTimimg, succeeded =>
                    {
                        if (succeeded)
                            CoinDropped?.Invoke();
                    },
                    useAbsoluteTime: true)
                    : null;
            }
        }

        private void Close()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Close");
            if (_darkscreenUI) _darkscreenUI.Disable();

            _updater?.Dispose();
            _coinTimer?.Dispose();
            _coinDropTimer?.Dispose();
            _effectTimer?.Dispose();

            _updater = null;
            _coinTimer = null;
            _coinDropTimer = null;
            _effectTimer = null;

            Disable();
            _isOperating = false;
            _isOperated = false;
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
            print("Destroy, " + _isDestroyed);

            if (_isDestroyed) return;
            _isDestroyed = true;

            _enabler?.Dispose();
            Destroying?.Invoke();

            _updater?.Dispose();
            _coinTimer?.Dispose();
            _coinDropTimer?.Dispose();
            _effectTimer?.Dispose();

            RelicManager.Instance.RelicAcquiring -= OnRelicAcquiring;
        }
    }
}
