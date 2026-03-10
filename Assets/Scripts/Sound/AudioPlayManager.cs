using System;
using System.Linq;
using BlackboxSystem;
using UnityEngine;
using Infrastructure;

namespace Sound
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class AudioPlayManager<TAudioName> : MonoBehaviour,
        IStandaloneInitializable
        where TAudioName : Enum
    {
        [Serializable]
        public struct AudioData
        {
            public TAudioName Name;
            public AudioClip AudioClip;
        }
        [SerializeField] private AudioData[] _audios;

        private AudioSource _audioSource;
        private bool _isAwake = false;


        void IStandaloneInitializable.StandaloneInitialize() => Awake();
        private void Awake()
        {
            if (_isAwake) return;
            _isAwake = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(TAudioName name)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Play: {name}");

            var clip = _audios.FirstOrDefault(a => a.Name.Equals(name));
            if (clip.Name == null || clip.AudioClip == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteError(
                    $"'{name}' 오디오 파일을 가져오는 데 실패했습니다."),
                    this);
                return;
            }

            if (_audioSource)
                _audioSource.PlayOneShot(clip.AudioClip);
        }

        public void Stop()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Stop");
            _audioSource.Stop();
        }

        public void SetVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Volume to {volume}");

            _audioSource.volume = volume / 100f;
        }
    }
}
