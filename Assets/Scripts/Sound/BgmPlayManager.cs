using System;
using System.Collections;
using System.Linq;
using BlackboxSystem;
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
        private static float _targetVolume = 1f;

        private static BgmPlayManager _instance;
        private static bool _isStarted;


        void IStandaloneInitializable.StandaloneInitialize() => Awake();

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            if (_gameServices) return;
            _gameServices = gameServices;
        }

        private void Start()
        {
            if (_isStarted) return;
            _isStarted = true;

            SetVolume(_gameServices.BgmVolume);
            _gameServices.BgmChanged += SetVolume;
        }

        private void OnDestroy()
        {
            if (_instance != this) return;

            _gameServices.BgmChanged -= SetVolume;
            _instance = null;
        }

        public static void Play(BgmName name)
        {
            using var _ = BlackboxHandle.Of(_instance).WriteScope($"Play BGM: {name}");

            var clipData = _instance._bgms.FirstOrDefault(a => a.Name == name);
            if (clipData.Name == BgmName.None || clipData.AudioClip == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(_instance).WriteError(
                    $"'{name}' BGM 파일을 가져오는 데 실패했습니다."),
                    _instance);
                return;
            }

            if (_fadeCoroutine != null)
                _instance.StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = _instance.StartCoroutine(CrossFadeBgm(clipData.AudioClip));
        }

        public static void Stop()
        {
            using var _ = BlackboxHandle.Of(_instance).WriteScope($"Stop BGM, validGO: {_instance != null}");
            if (!_instance) return;

            if (_fadeCoroutine != null) _instance.StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = _instance.StartCoroutine(FadeOutAndStop());

            IEnumerator FadeOutAndStop()
            {
                if (_audioSource.isPlaying)
                {
                    float startVolume = _audioSource.volume;
                    for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
                    {
                        _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _instance._fadeDuration);
                        yield return null;
                    }
                    _audioSource.volume = 0f;
                    _audioSource.Stop();
                }
                _fadeCoroutine = null;
            }
        }

        public static void SetVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            _targetVolume = volume / 100f;

            using var _ = BlackboxHandle.Of(_instance).WriteScope($"Set BGM Volume to {volume}");

            if (_fadeCoroutine == null)
                _audioSource.volume = _targetVolume;
        }

        private static IEnumerator CrossFadeBgm(AudioClip newClip)
        {
            if (_audioSource.isPlaying)
            {
                float startVolume = _audioSource.volume;
                for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
                {
                    _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _instance._fadeDuration);
                    yield return null;
                }
                _audioSource.volume = 0f;
                _audioSource.Stop();
            }

            _audioSource.clip = newClip;
            _audioSource.Play();

            for (float t = 0; t < _instance._fadeDuration; t += Time.deltaTime)
            {
                _audioSource.volume = Mathf.Lerp(0f, _targetVolume, t / _instance._fadeDuration);
                yield return null;
            }

            _audioSource.volume = _targetVolume;
            _fadeCoroutine = null;
        }
    }
}
