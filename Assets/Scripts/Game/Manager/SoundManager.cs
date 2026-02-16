using System;
using System.Linq;
using BlackboxSystem;
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
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Bgm Volume: {volume}");

            BgmVolume = volume;
            BgmChanged?.Invoke(volume);
        }

        public void SetSfxVolume(int volume)
        {
            volume = Mathf.Clamp(volume, 0, 100);
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Sfx Volume: {volume}");

            SfxVolume = volume;
            SfxChanged?.Invoke(volume);
        }

        public void SetListenerIfPossible()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Listener If Possible: {_setListenerIfPossible}");
            if (!_setListenerIfPossible) return;

            var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners.Any(l => l.gameObject.activeInHierarchy && l.enabled)) return;

            if (listeners.Length > 0)
            {
                var togglable = listeners.FirstOrDefault(l => l.gameObject.activeInHierarchy);
                if (togglable)
                {
                    Debug.Log(BlackboxHandle.Of(this).WriteMessage(
                        $"[SoundManager] '{togglable.name}'의 Listener를 활성화합니다."),
                        this);

                    togglable.enabled = true;
                    return;
                }

                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "[SoundManager] 비활성화된 객체에 Listener가 존재합니다. 소리가 정상적으로 출력되지 않을 수 있습니다."),
                    this);
                return;
            }

            Debug.Log(BlackboxHandle.Of(this).WriteMessage(
                "[SoundManager] 유효한 Listener가 없으므로 새 Audio Listener 객체를 생성합니다."),
                this);

            new GameObject("Audio Listener", typeof(AudioListener));
        }
    }
}
