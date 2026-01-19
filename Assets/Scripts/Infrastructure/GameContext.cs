using UnityEngine;

namespace Infrastructure
{
    public abstract class GameContext : MonoBehaviour
    {
        public abstract int BgmVolume { get; }
        public abstract int SfxVolume { get; }
        public abstract void SetBgmVolume(int volume, object context = null);
        public abstract void SetSfxVolume(int volume, object context = null);

        public abstract void Quit(object context = null);
    }
}
