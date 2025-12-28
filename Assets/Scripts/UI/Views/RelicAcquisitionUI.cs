using System;
using System.Linq;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    public class RelicAcquisitionUI : MonoBehaviour, IInitializable
    {
        [Header("Relic")]
        [SerializeField] private Image _relicIcon;
        [SerializeField] private TextMeshProUGUI _relicNametag;
        [SerializeField] private TextMeshProUGUI _relicDescrption;
        [SerializeField] private Button _toThrowCoin;
        [SerializeField] private Button _close;

        [Header("Throw Coin")]
        [SerializeField] private GameObject _throwCoin;
        [SerializeField] private TextMeshProUGUI _coinDescripton;
        [SerializeField] private VideoPlayer _coinRawVideoPlayer;
        [SerializeField] private VideoPlayer _coinMaskVideoPlayer;

        [Serializable]
        public struct VideoData
        {
            public bool IsFront;
            public VideoClip Video;
            public VideoClip AlphaMask;
        }
        [SerializeField] private VideoData[] _videoClips;

        private IDisposable _updater, _timer;
        private RelicDataSO _relic;

        private bool _initialized = false;
        private bool _operating = false;


        private void Awake() => Initialize();
        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _toThrowCoin.onClick.AddListener(ToThrowCoin);
            _close.onClick.AddListener(Close);

            RelicManager.Instance.RelicAcquiring += OnRelicAcquiring;
            gameObject.SetActive(false);
        }

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
            gameObject.SetActive(true);

            _toThrowCoin.gameObject.SetActive(true);
            _close.gameObject.SetActive(false);

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

            _updater = Loco.Subscribe(() =>
            {
                if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) <= 0.0001f)
                    return;

                ThrowCoin();
                _updater.Dispose();
            });

            void ThrowCoin()
            {
                var reinforced = RelicManager.Instance.StartCoinRandom(_relic.RelicNumber);
                print($"강화 여부: {(reinforced ? "성공" : "실패")}");

                var clip = _videoClips.FirstOrDefault(v => v.IsFront == reinforced);
                if (!clip.Video || !clip.AlphaMask)
                    throw new InvalidOperationException(
                        $"[{nameof(RelicAcquisitionUI)}] {nameof(clip)}이(가) 유효하지 않습니다.");

                _coinRawVideoPlayer.clip = clip.Video;
                _coinMaskVideoPlayer.clip = clip.AlphaMask;

                _timer = new Timer((float)clip.Video.length, succeeded =>
                {
                    if (succeeded)
                    {
                        RelicManager.Instance.AddRelic(_relic.RelicNumber, reinforced);

                        _relicDescrption.text =
                            $"강화 {(reinforced ? "성공" : "실패")}\n\n" +
                            _relicDescrption.text;

                        _toThrowCoin.gameObject.SetActive(false);
                        _close.gameObject.SetActive(true);

                        _throwCoin.SetActive(false);
                    }
                });
            }
        }

        private void Close()
        {
            _updater?.Dispose();
            _updater = null;

            _timer?.Dispose();
            _timer = null;

            gameObject.SetActive(false);
            _operating = false;
        }

        private void OnDestroy()
        {
            _updater?.Dispose();
            _timer?.Dispose();

            RelicManager.Instance.RelicAcquiring -= OnRelicAcquiring;
        }
    }
}
