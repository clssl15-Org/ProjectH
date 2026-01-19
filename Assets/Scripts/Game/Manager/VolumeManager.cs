using UnityEngine;
using BlackboxSystem;

namespace Game.Management
{
    internal class VolumeManager
    {
        public int BgmVolume { get; private set; } = 100;
        public int SfxVolume { get; private set; } = 100;

        public void SetBgmVolume(int volume)
        {
            BlackboxHandle.Of(this).Write($"Set Bgm Volume: {volume}");
            BgmVolume = Mathf.Clamp(volume, 0, 100);
        }

        public void SetSfxVolume(int volume)
        {
            BlackboxHandle.Of(this).Write($"Set Sfx Volume: {volume}");
            SfxVolume = Mathf.Clamp(volume, 0, 100);
        }
    }
}
