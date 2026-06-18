using System;
using System.Linq;
using BlackThunder.BlackboxSystem;
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
        private BlackboxHandle _blackbox;


        void IStandaloneInitializable.StandaloneInitialize() => Awake();
        private void Awake()
        {
            if (_isAwake) return;
            using var _ = BlackboxHandle.Of(this).Construct("오디오 재생 매니저 초기화를 시작합니다.", out _blackbox);

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
            using var _ = _blackbox.Scope("오디오 재생을 정지합니다.");

            _audioSource.Stop();
        }

        public void SetVolume(int volume)
        {
            using var _ = _blackbox.Scope($"오디오 볼륨을 설정합니다. volume: {volume}");

            volume = Mathf.Clamp(volume, 0, 100);

            _audioSource.volume = volume / 100f;
        }
    }
}
