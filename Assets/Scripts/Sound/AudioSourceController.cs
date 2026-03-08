using System;
using System.Linq;
using BlackboxSystem;
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
        }
        [SerializeField] private AudioData[] _audios;

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


        protected virtual void Awake() => EnsureInitialization();
        private void EnsureInitialization()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Initialize");
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(string name, bool playIndependently = false)
        {
            if (!TryPlay(name, playIndependently))
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"'{name}' 오디오를 재생하는 데 실패했습니다."));
        }
        public bool TryPlay(string name, bool playIndependently = false)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play {name}, independent: {playIndependently}");
            EnsureInitialization();

            var clip = _audios.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.Ordinal));
            if (clip.Name == null || !clip.AudioClip) return false;

            if (playIndependently)
                AudioSource.PlayClipAtPoint(clip.AudioClip, transform.position, _audioSource.volume);
            else
                _audioSource.PlayOneShot(clip.AudioClip);

            return true;
        }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop");
            EnsureInitialization();

            _audioSource.Stop();
        }

        public void SetVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Volume to {volume}");

            EnsureInitialization();
            _audioSource.volume = volume / 100f;
        }
    }
}
