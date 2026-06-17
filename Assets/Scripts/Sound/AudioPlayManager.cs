using System;
using System.Linq;
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

            _audioSource = GetComponent<AudioSource>();
        }

        public void Play(TAudioName name)
        {

            var clip = _audios.FirstOrDefault(a => a.Name.Equals(name));
            if (clip.Name == null || clip.AudioClip == null)
            {
                Debug.LogWarning($"'{name}' ����� ������ �������� �� �����߽��ϴ�.",
                    this);
                return;
            }

            if (_audioSource)
                _audioSource.PlayOneShot(clip.AudioClip);
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        public void SetVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);

            _audioSource.volume = volume / 100f;
        }
    }
}
