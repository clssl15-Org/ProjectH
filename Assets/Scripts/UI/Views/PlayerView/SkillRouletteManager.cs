using System;
using Infrastructure;
using Sound;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI.PlayerView
{
    [RequireComponent(typeof(SfxAudioController))]
    public class SkillRouletteManager : MonoBehaviour, IEnablable
    {
        [Header("Default")]
        [SerializeField] private Animation _skillRouletteBackground;
        [SerializeField] private VideoPlayer _skillRouletteVideoPlayer;

        [Header("Effects")]
        [SerializeField, Min(0)] private float _effectPlayTiming = 1f;
        [SerializeField, Min(0)] private float _effectFadeInTime = 1f;
        [SerializeField, Range(0, 1)] private float _effectTargetAlpha = 1f;
        [SerializeField] private VideoPlayer _effectVideoPlayer;
        [SerializeField] private VideoPlayer _effectMaskVideoPlayer;
        [SerializeField] private RawImage _effectRawImage;

        [Header("Resources")]
        [SerializeField, Min(0)] private float _skillRouletteVideoPlaytime = 5f;
        [SerializeField] private VideoClip[] _skillRouletteVideos;

        [Serializable]
        public struct EffetctVideoData
        {
            public VideoClip Video;
            public VideoClip AlphaMask;
        }
        [SerializeField] private EffetctVideoData[] _effectVideos;

        private SfxAudioController _sfxAudioController;
        private EnableWithAnimation _skillRouletteEnabler;
        private IDisposable _deactivateTimer, _effectTimer, _effectFadeTimer;

        #region Interfaces
        Action IEnablable.OnEnabling =>
            () => _skillRouletteBackground.gameObject.SetActive(true);
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => null;
        Action IEnablable.OnDisabled =>
            () => _skillRouletteBackground.gameObject.SetActive(false);
        #endregion


        private void Awake()
        {
            _skillRouletteEnabler = new EnableWithAnimation(_skillRouletteBackground, false)
                .InitializeWithIEnablable(this, false);
            _skillRouletteEnabler.SetToDisabled();

            _sfxAudioController = GetComponent<SfxAudioController>();

            _skillRouletteVideoPlayer.clip = null;
            _effectVideoPlayer.gameObject.SetActive(false);
        }

        public void Enable(int index, Action callback)
        {
            DisableInternal();

            _skillRouletteEnabler.Enable();
            _sfxAudioController.Play("PlayerRoulette_Spin", AudioSourceController.PlayOption.Independently);

            var effectIdx = index - 3;
            var isGoodBouns = effectIdx >= 0;

            _deactivateTimer = new Timer(
                _skillRouletteVideoPlaytime,
                succeeded =>
                {
                    if (succeeded)
                    {
                        _sfxAudioController.Play(
                            !isGoodBouns ? "PlayerRoulette_A" : "PlayerRoulette_B",
                            AudioSourceController.PlayOption.Independently);

                        Disable();
                        callback?.Invoke();
                    }
                });

            _skillRouletteVideoPlayer.clip = _skillRouletteVideos[index];
            _skillRouletteVideoPlayer.Play();

            if (isGoodBouns)
                _effectTimer = new Timer(_effectPlayTiming, succeeded =>
                {
                    if (!succeeded) return;

                    _effectVideoPlayer.gameObject.SetActive(true);

                    _effectVideoPlayer.clip = _effectVideos[effectIdx].Video;
                    _effectMaskVideoPlayer.clip = _effectVideos[effectIdx].AlphaMask;

                    _effectVideoPlayer.Play();
                    _effectMaskVideoPlayer.Play();

                    float elapsedTime = 0f;
                    _effectFadeTimer = new Timer(_effectFadeInTime, updated: () =>
                    {
                        elapsedTime += Time.unscaledDeltaTime;
                        _effectRawImage.color = new Color(1, 1, 1, _effectTargetAlpha * elapsedTime / _effectFadeInTime);
                    });
                });
        }

        public void Disable()
        {
            _skillRouletteEnabler.Disable();
            DisableInternal();
        }
        private void DisableInternal()
        {
            _skillRouletteVideoPlayer.Stop();
            _skillRouletteVideoPlayer.clip = null;

            _effectVideoPlayer.gameObject.SetActive(false);
            _effectRawImage.color = new Color(1, 1, 1, 0);
            _effectVideoPlayer.Stop();
            _effectMaskVideoPlayer.Stop();

            _effectVideoPlayer.clip = null;
            _effectMaskVideoPlayer.clip = null;

            _deactivateTimer?.Dispose();
            _effectTimer?.Dispose();
            _effectFadeTimer?.Dispose();
        }


        void IEnablable.Enable() =>
            throw new NotImplementedException();
        void IEnablable.Disable() =>
            throw new NotImplementedException();
        void IEnablable.SetToEnabled() =>
            throw new NotImplementedException();
        void IEnablable.SetToDisabled() =>
            throw new NotImplementedException();
    }
}
