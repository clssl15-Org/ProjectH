using System;
using System.Collections;
using System.Linq;
using BlackThunder.BlackboxSystem;
using Infrastructure;
using UnityEngine;

namespace Sound
{
    public enum BgmName
    {
        None,
        Title,
        Stage0,
        Stage1,
        Stage2,
        Stage2_Boss,
        Stage3,
        Stage3_Boss,
        Final_Boss,
    }

    [RequireComponent(typeof(AudioSource))]
    public class BgmPlayManager : MonoBehaviour,
        IStandaloneInitializable,
        IInjectable<GameServices>
    {
        [Serializable]
        public struct BgmData
        {
            public BgmName Name;
            public AudioClip AudioClip;
        }

        [SerializeField] private BgmData[] _bgms;
        [SerializeField] private float _fadeDuration = 1f;

        private static AudioSource _audioSource;
        private static GameServices _gameServices;

        private static Coroutine _fadeCoroutine;
        private static int _fadeVersion;
        private static float _targetVolume = 1f;

        private static BgmPlayManager _instance;
        private static bool _isStarted;
        private static bool _isVolumeSubscribed;
        private bool _isAwake;
        private BlackboxHandle _blackbox;


        void IStandaloneInitializable.StandaloneInitialize() => Awake();

        private void Awake()
        {
            if (_isAwake) return;
            using var _ = BlackboxHandle.Of(this).Construct("BGM 재생 매니저 초기화를 시작합니다.", out _blackbox);

            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _isAwake = true;
            _instance = this;

            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            using var _ = BlackboxHandle.Of(this).Scope("게임 서비스를 주입받습니다.").With(gameServices);

            if (_gameServices) return;
            _gameServices = gameServices;

            if (_isStarted)
                SubscribeVolume();
        }

        private void Start()
        {
            if (_isStarted) return;
            using var _ = _blackbox.Scope("BGM 재생 매니저 시작 설정을 적용합니다.");

            _isStarted = true;

            SubscribeVolume();
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            using var _ = _blackbox.Scope("BGM 재생 매니저를 정리합니다.");

            if (_gameServices != null && _isVolumeSubscribed)
            {
                _gameServices.BgmChanged -= SetVolume;
                _isVolumeSubscribed = false;
            }

            StopFadeCoroutine();
            _audioSource = null;
            _instance = null;
            _isStarted = false;
        }

        public static void Play(BgmName name)
        {
            if (!_instance || !_audioSource)
            {
                Debug.LogWarning($"'{name}' BGM을 재생할 수 없습니다. BGM 재생 매니저가 초기화되지 않았습니다.");
                return;
            }

            using var _ = _instance._blackbox.Scope($"BGM 재생을 시작합니다. name: {name}");

            var clipData = _instance._bgms.FirstOrDefault(a => a.Name == name);
            if (clipData.Name == BgmName.None || clipData.AudioClip == null)
            {
                Debug.LogWarning($"'{name}' BGM ������ �������� �� �����߽��ϴ�.",
                    _instance);
                return;
            }

            StopFadeCoroutine();

            var fadeVersion = ++_fadeVersion;
            _fadeCoroutine = _instance.StartCoroutine(CrossFadeBgm(clipData.AudioClip, fadeVersion));
        }

        public static void Stop()
        {
            if (!_instance || !_audioSource) return;
            using var _ = _instance._blackbox.Scope("BGM 재생을 정지합니다.");

            StopFadeCoroutine();

            var fadeVersion = ++_fadeVersion;
            _fadeCoroutine = _instance.StartCoroutine(FadeOutAndStop(fadeVersion));

            IEnumerator FadeOutAndStop(int version)
            {
                if (_audioSource.isPlaying)
                {
                    float startVolume = _audioSource.volume;
                    for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
                    {
                        if (version != _fadeVersion)
                            yield break;

                        _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _instance._fadeDuration);
                        yield return null;
                    }

                    if (version != _fadeVersion)
                        yield break;

                    _audioSource.volume = 0f;
                    _audioSource.Stop();
                }

                if (version == _fadeVersion)
                    _fadeCoroutine = null;
            }
        }

        public static void SetVolume(int volume)
        {
            if (_instance)
            {
                using var _ = _instance._blackbox.Scope($"BGM 볼륨을 반영합니다. volume: {volume}");
                ApplyVolume(volume);
                return;
            }

            ApplyVolume(volume);
        }

        private static void ApplyVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            _targetVolume = volume / 100f;


            if (_fadeCoroutine == null && _audioSource)
                _audioSource.volume = _targetVolume;
        }

        private static void SubscribeVolume()
        {
            if (_gameServices == null)
            {
                Debug.LogWarning("[BgmPlayManager] GameServices가 주입되지 않아 BGM 볼륨 동기화를 건너뜁니다.",
                    _instance);
                return;
            }

            if (_isVolumeSubscribed) return;

            SetVolume(_gameServices.BgmVolume);
            _gameServices.BgmChanged += SetVolume;
            _isVolumeSubscribed = true;
        }

        private static void StopFadeCoroutine()
        {
            if (_fadeCoroutine == null) return;

            if (_instance)
                _instance.StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = null;
        }

        private static IEnumerator CrossFadeBgm(AudioClip newClip, int fadeVersion)
        {
            if (_audioSource.isPlaying)
            {
                float startVolume = _audioSource.volume;
                for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
                {
                    if (fadeVersion != _fadeVersion)
                        yield break;

                    _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _instance._fadeDuration);
                    yield return null;
                }

                if (fadeVersion != _fadeVersion)
                    yield break;

                _audioSource.volume = 0f;
                _audioSource.Stop();
            }

            if (fadeVersion != _fadeVersion)
                yield break;

            _audioSource.clip = newClip;
            _audioSource.Play();

            for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
            {
                if (fadeVersion != _fadeVersion)
                    yield break;

                _audioSource.volume = Mathf.Lerp(0f, _targetVolume, t / _instance._fadeDuration);
                yield return null;
            }

            if (fadeVersion == _fadeVersion)
            {
                _audioSource.volume = _targetVolume;
                _fadeCoroutine = null;
            }
        }
    }
}
