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

        private AudioSource _audioSource;
        private bool _isAwake = false;
        private GameServices _gameServices;

        private Coroutine _fadeCoroutine;
        private float _targetVolume = 1f;

        void IStandaloneInitializable.StandaloneInitialize() => Awake();

        private void Awake()
        {
            if (_isAwake) return;
            _isAwake = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices) =>
            _gameServices = gameServices;

        private void Start()
        {
            SetVolume(_gameServices.BgmVolume);
            _gameServices.BgmChanged += SetVolume;
        }

        private void OnDestroy()
        {
            _gameServices.BgmChanged -= SetVolume;
        }

        public void Play(BgmName name)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play BGM: {name}");

            var clipData = _bgms.FirstOrDefault(a => a.Name == name);
            if (clipData.Name == BgmName.None || clipData.AudioClip == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteError(
                    $"'{name}' BGM 파일을 가져오는 데 실패했습니다."),
                    this);
                return;
            }

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(CrossFadeBgm(clipData.AudioClip));
        }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop BGM");

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAndStop());
        }

        public void SetVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            _targetVolume = volume / 100f;

            using var _ = BlackboxHandle.Of(this).WriteScope($"Set BGM Volume to {volume}");

            if (_fadeCoroutine == null)
                _audioSource.volume = _targetVolume;
        }


        private IEnumerator CrossFadeBgm(AudioClip newClip)
        {
            if (_audioSource.isPlaying)
            {
                float startVolume = _audioSource.volume;
                for (float t = 0; t < _fadeDuration; t += Time.deltaTime)
                {
                    _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _fadeDuration);
                    yield return null;
                }
                _audioSource.volume = 0f;
                _audioSource.Stop();
            }

            _audioSource.clip = newClip;
            _audioSource.Play();

            for (float t = 0; t < _fadeDuration; t += Time.deltaTime)
            {
                _audioSource.volume = Mathf.Lerp(0f, _targetVolume, t / _fadeDuration);
                yield return null;
            }

            _audioSource.volume = _targetVolume;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutAndStop()
        {
            if (_audioSource.isPlaying)
            {
                float startVolume = _audioSource.volume;
                for (float t = 0; t < _fadeDuration; t += Time.deltaTime)
                {
                    _audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _fadeDuration);
                    yield return null;
                }
                _audioSource.volume = 0f;
                _audioSource.Stop();
            }
            _fadeCoroutine = null;
        }
    }
}
