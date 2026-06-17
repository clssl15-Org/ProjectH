using System;
using System.Linq;
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

        public void SetBgmVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);

            BgmVolume = volume;
            BgmChanged?.Invoke(volume);
        }

        public void SetSfxVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);

            SfxVolume = volume;
            SfxChanged?.Invoke(volume);
        }

        public void SetListenerIfPossible()
        {
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
