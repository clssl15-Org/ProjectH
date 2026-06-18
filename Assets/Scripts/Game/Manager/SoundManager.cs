using System;
using System.Linq;
using BlackThunder.BlackboxSystem;
using UnityEngine;

namespace Game.Management
{
    public class SoundManager : MonoBehaviour
    {
        public event Action<int> BgmChanged;
        public event Action<int> SfxChanged;

        public int BgmVolume { get; private set; } = 70;
        public int SfxVolume { get; private set; } = 70;

        [SerializeField] private bool _setListenerIfPossible = true;
        private BlackboxHandle _blackbox;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("사운드 매니저 초기화를 시작합니다.", out _blackbox);
        }

        public void SetBgmVolume(int volume)
        {
            using var _ = _blackbox.Scope($"BGM 볼륨을 설정합니다. volume: {volume}");

            volume = Mathf.Clamp(volume, 0, 100);

            BgmVolume = volume;
            BgmChanged?.Invoke(volume);
        }

        public void SetSfxVolume(int volume)
        {
            using var _ = _blackbox.Scope($"SFX 볼륨을 설정합니다. volume: {volume}");

            volume = Mathf.Clamp(volume, 0, 100);

            SfxVolume = volume;
            SfxChanged?.Invoke(volume);
        }

        public void SetListenerIfPossible()
        {
            using var _ = _blackbox.Scope("Audio Listener 보정을 확인합니다.");

            if (!_setListenerIfPossible) return;

            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners.Any(l => l.gameObject.activeInHierarchy && l.enabled)) return;

            if (listeners.Length > 0)
            {
                var togglable = listeners.FirstOrDefault(l => l.gameObject.activeInHierarchy);
                if (togglable)
                {
                    Debug.Log($"[SoundManager] '{togglable.name}'�� Listener�� Ȱ��ȭ�մϴ�.",
                        this);

                    togglable.enabled = true;
                    return;
                }

                Debug.LogWarning("[SoundManager] ��Ȱ��ȭ�� ��ü�� Listener�� �����մϴ�. �Ҹ��� ���������� ��µ��� ���� �� �ֽ��ϴ�.",
                    this);
                return;
            }

            Debug.Log("[SoundManager] ��ȿ�� Listener�� �����Ƿ� �� Audio Listener ��ü�� �����մϴ�.",
                this);

            new GameObject("Audio Listener", typeof(AudioListener));
        }
    }
}
