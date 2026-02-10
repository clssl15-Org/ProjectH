using System;
using BlackboxSystem;
using UnityEngine;

namespace Game.Management
{
    internal class SoundManager
    {
        public event Action<float> BgmChanged;
        public event Action<float> SfxChanged;

        public int BgmVolume { get; private set; } = 100;
        public int SfxVolume { get; private set; } = 100;

        public void SetBgmVolume(int volume)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Bgm Volume: {volume}");

            BgmVolume = Mathf.Clamp(volume, 0, 100);
            BgmChanged?.Invoke(BgmVolume);
        }

        public void SetSfxVolume(int volume)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Sfx Volume: {volume}");

            SfxVolume = Mathf.Clamp(volume, 0, 100);
            SfxChanged?.Invoke(SfxVolume);
        }
    }
}
