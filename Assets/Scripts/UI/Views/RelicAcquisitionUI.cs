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
        ICursorVisibilityControllerUser,
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
        [SerializeField, Tooltip("동전 영상 끝 기준 낙하음 알림 시간입니다. 음수면 영상 끝 이전에 알립니다.")]
        private float _dropEventNotifyTiming = -2.6f;
        [SerializeField, UnityEngine.Min(0f)] private float _coinCompletionGraceTime = 0.75f;

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
        private CursorVisibilityController _cursorVisibilityController;
        private DarkscreenUI _darkscreenUI;
        private IDisposable _updater, _coinTimer, _coinDropTimer, _effectTimer, _coinCompletionGuardTimer;
        private RelicDataSO _relic;
        private RelicManager.RelicAcquisitionDto _relicAcquisition;
        private bool _forceSuccess;
        private EnableWithAnimation _enabler;

        private bool _isInitialized;
        private bool _isOperating;
        private bool _isOperated;
        private bool _isDestroyed;
        private bool _isCoinCompletionHandled;
        private bool _isCoinVideoEventSubscribed;
        private bool _currentCoinReinforced;

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
            _cursorVisibilityController?.RequestVisible(this);

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
        void ICursorVisibilityControllerUser.Initialize(CursorVisibilityController cursorVisibilityController) =>
            _cursorVisibilityController = cursorVisibilityController;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;

        private void OnRelicAcquiring(RelicManager.RelicAcquisitionDto relicAcquisition)
        {
            var relicInfo = relicAcquisition.Data;

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

            ClearCoinPlaybackGuards();
            _isOperating = true;
            _isOperated = false;
            _isCoinCompletionHandled = false;
            _relic = relicInfo;
            _relicAcquisition = relicAcquisition;
            _forceSuccess = relicAcquisition.ForceSuccess;

            Enable();

            _relicIcon.sprite = relicInfo.Icon;
            _relicNametag.text = relicInfo.RelicName;
            _relicDescrption.text = relicAcquisition.NormalDescription;

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

            string normalNextValue = FormatValue(_relicAcquisition.NormalNextValue);
            string reinforcedNextValue = FormatValue(_relicAcquisition.ReinforcedNextValue);
            _coinDescripton.text = $"<align=center><size=120%>강화 성공 시 능력치 {normalNextValue} → {reinforcedNextValue}</size></align>";
            var reinforced = _forceSuccess || RelicManager.Instance.StartCoinRandom(_relic.RelicNumber);
            _currentCoinReinforced = reinforced;
            _isCoinCompletionHandled = false;

            var coinClip = _videoClips.FirstOrDefault(v => v.VideoType
                == (reinforced ? VideoType.CoinFront : VideoType.CoinBack));

            _updater?.Dispose();
            _updater = null;
            ClearCoinPlaybackGuards();

            if (!_coinRawVideoPlayer || !_coinMaskVideoPlayer)
            {
                Debug.LogError("동전 영상 재생기가 할당되지 않아 결과 화면으로 복구합니다.", this);
                CompleteCoinThrow(reinforced, "동전 영상 재생기 누락");
                return;
            }

            if (!TryGetClipPlayDuration(coinClip.Video, CoinAnimationPlaySpeed, out var coinPlayDuration))
            {
                Debug.LogWarning("동전 영상 클립이 없거나 길이가 비정상이라 결과 화면으로 복구합니다.", this);
                CompleteCoinThrow(reinforced, "동전 클립 누락");
                return;
            }

            _coinRawVideoPlayer.playbackSpeed = 0f;
            _coinMaskVideoPlayer.playbackSpeed = 0f;

            _coinRawVideoPlayer.clip = coinClip.Video;
            _coinMaskVideoPlayer.clip = coinClip.AlphaMask;

            _coinRawVideoPlayer.frame = 0;
            _coinMaskVideoPlayer.frame = 0;

            var canPlayEffect = false;
            if (reinforced)
            {
                var effectClip = _videoClips.FirstOrDefault(v => v.VideoType == VideoType.CoinEffect);

                if (_coinEffectVideoPlayer && _coinEffectMaskVideoPlayer && effectClip.Video && effectClip.AlphaMask)
                {
                    _coinEffectVideoPlayer.playbackSpeed = 0f;
                    _coinEffectMaskVideoPlayer.playbackSpeed = 0f;

                    _coinEffectVideoPlayer.clip = effectClip.Video;
                    _coinEffectMaskVideoPlayer.clip = effectClip.AlphaMask;

                    _coinEffectVideoPlayer.frame = 0;
                    _coinEffectMaskVideoPlayer.frame = 0;
                    canPlayEffect = true;
                }
                else
                {
                    Debug.LogWarning("동전 성공 효과 영상 구성이 없어 효과 재생만 건너뜁니다.", this);
                }
            }

            SubscribeCoinVideoEvents();

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

                if (_isCoinCompletionHandled)
                    return;

                _coinRawVideoPlayer.playbackSpeed = coinAnimationPlaySpeed;
                _coinMaskVideoPlayer.playbackSpeed = coinAnimationPlaySpeed;

                CoinThrown?.Invoke();

                _effectTimer?.Dispose();
                _effectTimer = canPlayEffect ?
                    new Timer(_effectPlayTiming / coinAnimationPlaySpeed, succeeded =>
                    {
<<<<<<< Updated upstream
                        BlackboxHandle.Of(this).Write($"Effect Ended, succeeded: {succeeded}");
=======
                        if (!succeeded || _isCoinCompletionHandled)
                            return;
>>>>>>> Stashed changes

                        _effectAnimation.SetActive(true);
                        _coinEffectVideoPlayer.playbackSpeed = 1f;
                        _coinEffectMaskVideoPlayer.playbackSpeed = 1f;
                    },
                    useAbsoluteTime: true)
                    : null;

                _coinTimer?.Dispose();
                _coinTimer = new Timer(coinPlayDuration, succeeded =>
                {
<<<<<<< Updated upstream
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
=======
                    if (succeeded)
                        CompleteCoinThrow(reinforced, "재생 시간 완료");
                },
                useAbsoluteTime: true);

                _coinCompletionGuardTimer?.Dispose();
                _coinCompletionGuardTimer = new Timer(coinPlayDuration + _coinCompletionGraceTime, succeeded =>
                {
                    if (!succeeded || _isCoinCompletionHandled)
                        return;

                    Debug.LogWarning("동전 완료 신호가 누락되어 안전장치로 결과 화면을 표시합니다.", this);
                    CompleteCoinThrow(reinforced, "완료 안전장치");
>>>>>>> Stashed changes
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
            ClearCoinPlaybackGuards();

            _updater = null;

            Disable();
            _isOperating = false;
            _isOperated = false;
            _isCoinCompletionHandled = false;
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

            _cursorVisibilityController?.ReleaseVisible(this);
            _enabler?.Dispose();
            Destroying?.Invoke();

            _updater?.Dispose();
            ClearCoinPlaybackGuards();

            if (RelicManager.Instance != null)
                RelicManager.Instance.RelicAcquiring -= OnRelicAcquiring;
        }

        private void CompleteCoinThrow(bool reinforced, string reason)
        {
            if (_isCoinCompletionHandled)
                return;

            _isCoinCompletionHandled = true;
            _isOperated = true;
            ClearCoinPlaybackGuards();

            string description = string.Empty;
            try
            {
                if (RelicManager.Instance == null)
                    throw new InvalidOperationException($"{nameof(RelicManager)}.Instance가 없습니다.");

                RelicManager.Instance.AddRelic(
                    _relic.RelicNumber,
                    out description,
                    reinforced);

                SetCoinResultDescription(reinforced, description);
            }
            catch (Exception ex)
            {
                Debug.LogError($"동전 결과 처리 중 오류가 발생했습니다. 원인: {reason}\n{ex}", this);
                string fallbackDescription = string.IsNullOrEmpty(description)
                    ? "유물 획득 처리 중 오류가 발생했습니다. 창을 닫은 뒤 진행 상태를 확인해 주세요."
                    : description;

                SetRelicDescriptionSafe(
                    $"<size=120%>{UiRichTextFormatter.RedHighlightOpenTag}유물 획득 처리 오류{UiRichTextFormatter.RedHighlightCloseTag}</size>\n\n{fallbackDescription}");
            }
            finally
            {
                ShowCoinResultPage();
            }
        }

        private void SetCoinResultDescription(bool reinforced, string description)
        {
            if (reinforced)
            {
                var infoMessage = UITextLibrary.GetText(UITextResource.Coin_Success, LanguageManager.Language);

                SetRelicDescriptionSafe(
                    $"<size=120%>{RelicManager.BlueHighlightOpenTag}{infoMessage}{RelicManager.BlueHighlightCloseTag}</size>\n\n{description}");
            }
            else
            {
                var infoMessage = UITextLibrary.GetText(UITextResource.Coin_Failed, LanguageManager.Language);

                SetRelicDescriptionSafe(
                    $"<size=120%>{UiRichTextFormatter.RedHighlightOpenTag}{infoMessage}{UiRichTextFormatter.RedHighlightCloseTag}</size>\n\n{description}");
            }
        }

        private void ShowCoinResultPage()
        {
            if (!this)
                return;

            _toThrowCoinBtn.gameObject.SetActive(false);
            _closeBtn.gameObject.SetActive(true);

            _coinPage.SetActive(false);
            _relicPage.SetActive(true);
        }

        private void SetRelicDescriptionSafe(string text)
        {
            try
            {
                _relicDescrption.text = FormatUiText(text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"유물 설명 표시 중 오류가 발생했습니다.\n{ex}", this);
                _relicDescrption.text = text;
            }
        }

        private void SubscribeCoinVideoEvents()
        {
            if (!_coinRawVideoPlayer || _isCoinVideoEventSubscribed)
                return;

            _coinRawVideoPlayer.loopPointReached += OnCoinRawVideoLoopPointReached;
            _coinRawVideoPlayer.errorReceived += OnCoinRawVideoErrorReceived;
            _isCoinVideoEventSubscribed = true;
        }

        private void UnsubscribeCoinVideoEvents()
        {
            if (!_coinRawVideoPlayer || !_isCoinVideoEventSubscribed)
                return;

            _coinRawVideoPlayer.loopPointReached -= OnCoinRawVideoLoopPointReached;
            _coinRawVideoPlayer.errorReceived -= OnCoinRawVideoErrorReceived;
            _isCoinVideoEventSubscribed = false;
        }

        private void OnCoinRawVideoLoopPointReached(VideoPlayer source) =>
            CompleteCoinThrow(_currentCoinReinforced, "영상 종료 이벤트");

        private void OnCoinRawVideoErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogWarning($"동전 영상 재생 오류가 발생하여 결과 화면으로 복구합니다. 오류: {message}", this);
            CompleteCoinThrow(_currentCoinReinforced, "영상 오류");
        }

        private void ClearCoinPlaybackGuards()
        {
            UnsubscribeCoinVideoEvents();

            _coinTimer?.Dispose();
            _coinDropTimer?.Dispose();
            _effectTimer?.Dispose();
            _coinCompletionGuardTimer?.Dispose();

            _coinTimer = null;
            _coinDropTimer = null;
            _effectTimer = null;
            _coinCompletionGuardTimer = null;
        }

        private static bool TryGetClipPlayDuration(VideoClip clip, float speed, out float playDuration)
        {
            playDuration = 0f;
            if (clip == null)
                return false;

            if (speed <= 0f)
                return false;

            double length = clip.length;
            if (double.IsNaN(length) || double.IsInfinity(length) || length <= 0d)
                return false;

            playDuration = (float)(length / speed);
            return playDuration > 0f && !float.IsNaN(playDuration) && !float.IsInfinity(playDuration);
        }

        private static string FormatValue(float value)
        {
            float rounded = Mathf.Round(value);
            if (Mathf.Approximately(value, rounded))
                return Mathf.RoundToInt(value).ToString();

            return value.ToString("0.##");
        }
    }
}
