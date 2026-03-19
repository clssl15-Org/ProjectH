using Infrastructure;
using UnityEngine;

namespace Sound
{
    public class SfxAudioController : AudioSourceController,
        IInjectable<GameServices>
    {
        [SerializeField, Min(0)] private float _volumeRate = 1;
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            _gameServices = gameServices;
            ApplySettings();
        }

#if UNITY_EDITOR
        private void Update() => ApplySettings();
#endif
        protected virtual void ApplySettings()
        {
            Volume = Mathf.RoundToInt(
                _volumeRate * (_gameServices?.SfxVolume ?? 100));
        }
    }
}
