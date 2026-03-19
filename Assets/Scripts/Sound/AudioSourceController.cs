using System;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;

namespace Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceController : MonoBehaviour
    {
        [Serializable]
        public struct AudioData
        {
            public string Name;
            public AudioClip AudioClip;
            public PlayOption PlayOption;
        }
        [SerializeField] private AudioData[] _audios;

        public int Volume
        {
            get
            {
                EnsureInitialization();
                return Mathf.RoundToInt(_audioSource.volume * 100);
            }
            set
            {
                EnsureInitialization();
                _audioSource.volume = Mathf.Clamp(value, 0, 100) / 100f;
            }
        }

        public float SpatialBlend
        {
            get
            {
                EnsureInitialization();
                return _audioSource.spatialBlend;
            }
            set
            {
                EnsureInitialization();
                _audioSource.spatialBlend = value;
            }
        }

        private AudioSource _audioSource;
        private bool _isInitialized = false;

        private const bool PrintSound = true;


        protected virtual void Awake() => EnsureInitialization();
        private void EnsureInitialization()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Initialize");
            _audioSource = GetComponent<AudioSource>();
        }

        public enum PlayOption
        {
            None,
            OneShot,
            Loop,
            Independently,
        }
        public void Play(string name, PlayOption playOption = PlayOption.None, float? independentPlayTime = null)
        {
            if (!TryPlay(name, playOption, independentPlayTime))
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"'{name}' 오디오를 재생하는 데 실패했습니다. " +
                    $"오디오 목록: {(_audios?.Length > 0 ? ("\n" + string.Join(", ", _audios.Select(a => a.Name))) : "None")}"));
        }
        public bool TryPlay(string name, PlayOption playOption = PlayOption.None, float? independentPlayTime = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play {name}, playOption: {playOption}");
            EnsureInitialization();

            if (!TryGetAudioData(name, out var clip))
                return false;

            _audioSource.loop = false;

            if (playOption == PlayOption.None)
                playOption = clip.PlayOption != PlayOption.None
                    ? clip.PlayOption
                    : throw new ArgumentException(
                        $"[{gameObject.name}] {nameof(playOption)}은(는) {PlayOption.None}일 수 없습니다. name: {name}");

            switch (playOption)
            {
                case PlayOption.Loop:
                    _audioSource.clip = clip.AudioClip;
                    _audioSource.loop = true;
                    _audioSource.Play();
                    break;

                case PlayOption.Independently:
                    var go = new GameObject($"[{name}] OneShot: {clip.AudioClip.name}");
                    go.transform.position = transform.position;

                    var source = go.AddComponent<AudioSource>();
                    source.clip = clip.AudioClip;
                    source.volume = _audioSource.volume;
                    source.pitch = _audioSource.pitch;
                    source.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
                    source.spatialBlend = _audioSource.spatialBlend;
                    source.minDistance = _audioSource.minDistance;
                    source.maxDistance = _audioSource.maxDistance;
                    source.rolloffMode = _audioSource.rolloffMode;
                    source.priority = _audioSource.priority;
                    source.panStereo = _audioSource.panStereo;
                    source.loop = true;

                    source.Play();
                    Destroy(go, independentPlayTime ?? clip.AudioClip.length);
                    break;

                default:
                    _audioSource.PlayOneShot(clip.AudioClip);
                    break;
            }

            if (PrintSound.Resolve(false)) print($"<<[♪] {gameObject.name}: {name}>>");
            return true;
        }

        protected bool TryGetAudioData(string name, out AudioData audioData)
        {
            audioData = _audios.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            return audioData.Name != null && audioData.AudioClip;
        }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop");
            EnsureInitialization();

            _audioSource.Stop();
            _audioSource.loop = false;
            _audioSource.clip = null;
        }
    }
}
