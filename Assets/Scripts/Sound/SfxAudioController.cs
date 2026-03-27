using Infrastructure;
using UnityEngine;

namespace Sound
{
    public class SfxAudioController : AudioSourceController,
        IInjectable<GameServices>
    {
        [field: SerializeField, Min(0)] public float VolumeRate { get; set; } = 1;
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            _gameServices = gameServices;
            ApplySettings();
        }

#if DEBUG_MODE
        private void Update() => ApplySettings();
#endif
        protected virtual void ApplySettings()
        {
            Volume = Mathf.RoundToInt(
                VolumeRate * (_gameServices?.SfxVolume ?? 100));
        }
    }
}
