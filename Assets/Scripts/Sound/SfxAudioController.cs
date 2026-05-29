using Infrastructure;
using UnityEngine;

namespace Sound
{
    public class SfxAudioController : AudioSourceController,
        IInjectable<GameServices>
    {
        [field: SerializeField, Min(0)] public float VolumeRate { get; set; } = 1;
        private GameServices _gameServices;
        private bool _sfxChangedSubscribed;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            UnsubscribeFromSfxChanged();
            _gameServices = gameServices;
            SubscribeToSfxChanged();
            ApplySettings();
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureGameServices();
        }

        private void OnDestroy()
        {
            UnsubscribeFromSfxChanged();
        }

        private void EnsureGameServices()
        {
            if (_gameServices) return;

            _gameServices = FindAnyObjectByType<GameServices>(FindObjectsInactive.Include);
            SubscribeToSfxChanged();
            ApplySettings();
        }

        private void SubscribeToSfxChanged()
        {
            if (_sfxChangedSubscribed || _gameServices == null) return;

            _gameServices.SfxChanged += OnSfxVolumeChanged;
            _sfxChangedSubscribed = true;
        }

        private void UnsubscribeFromSfxChanged()
        {
            if (!_sfxChangedSubscribed || _gameServices == null) return;

            _gameServices.SfxChanged -= OnSfxVolumeChanged;
            _sfxChangedSubscribed = false;
        }

        private void OnSfxVolumeChanged(int _) => ApplySettings();

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
