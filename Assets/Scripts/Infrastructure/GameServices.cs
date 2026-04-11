using System;
using UnityEngine;

namespace Infrastructure
{
    public abstract class GameServices : MonoBehaviour
    {
        public abstract event Action<int> BgmChanged;
        public abstract event Action<int> SfxChanged;
        public abstract int BgmVolume { get; }
        public abstract int SfxVolume { get; }
        public abstract void SetBgmVolume(int volume, object context = null);
        public abstract void SetSfxVolume(int volume, object context = null);

        public abstract bool PlayerHasDied { get; }
        public abstract bool IsStage3Reached { get; set; }
        public abstract bool IsGameCleared { get; set; }
        public abstract void SetPlayerName(string playerName, object context = null);

        public abstract void ToFirstScene(object context = null);
        public abstract void ChangeScene(string sceneName, object context = null);
        public abstract void Quit(object context = null);
    }
}
