using Infrastructure;
using UnityEngine;

namespace Sound
{
    public class SfxAudioController : AudioSourceController,
        IInjectable<GameServices>
    {
        [field: SerializeField, Min(0)] public float VolumeRate { get; set; } = 1;
        private Game.Management.SoundManager _volumeSettings;
        private bool _sfxChangedSubscribed;

        void IInjectable<GameServices>.Inject(GameServices _) => EnsureVolumeSettings();

        private void Start() => EnsureVolumeSettings();

        private void OnDestroy()
        {
            UnsubscribeFromSfxChanged();
        }

        private void EnsureVolumeSettings()
        {
            if (_volumeSettings) return;

            _volumeSettings = FindAnyObjectByType<Game.Management.SoundManager>(FindObjectsInactive.Exclude);
            if (!_volumeSettings) return;

            SubscribeToSfxChanged();
            ApplySettings();
        }

        private void SubscribeToSfxChanged()
        {
            if (_sfxChangedSubscribed || !_volumeSettings) return;

            _volumeSettings.SfxChanged += OnSfxVolumeChanged;
            _sfxChangedSubscribed = true;
        }

        private void UnsubscribeFromSfxChanged()
        {
            if (!_sfxChangedSubscribed || !_volumeSettings) return;

            _volumeSettings.SfxChanged -= OnSfxVolumeChanged;
            _sfxChangedSubscribed = false;
        }

        private void OnSfxVolumeChanged(int _) => ApplySettings();

#if DEBUG_MODE
        private void Update() => ApplySettings();
#endif
        protected virtual void ApplySettings()
        {
            Volume = Mathf.RoundToInt(
                VolumeRate * (_volumeSettings?.SfxVolume ?? 100));
        }
    }
}
