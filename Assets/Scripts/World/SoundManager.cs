using System;
using System.Collections;
using UnityEngine;

public enum PlayerAction
{
    Run,
    Dash,
    Jump,
    Drop,
    Attack1,
    Attack2,
    Attack3,
    AttackHit1,
    AttackHit2,
    AttackHit3,
    Hit,
    RangedAttack,
    Skill1,
    Skill2,
    Skill3,
    UltimateCharge,
    UltimateRelease,
    PotionUse,
}

[System.Serializable]
public struct ActionSound
{
    public PlayerAction action;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
}

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource loopSfxSource;

    [Header("Sound Clips")]
    public ActionSound[] actionSounds;

    private Coroutine fadeOutCoroutine;
    private Game.Management.SoundManager _volumeSettings;
    private bool _sfxChangedSubscribed;
    private float _loopClipVolume = 1f;

    private void Start()
    {
        EnsureVolumeSettings();
        ApplySfxVolume();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSfxChanged();
    }

    private void EnsureVolumeSettings()
    {
        if (_volumeSettings) return;

        _volumeSettings = FindAnyObjectByType<Game.Management.SoundManager>(FindObjectsInactive.Exclude);
        SubscribeToSfxChanged();
    }

    private void SubscribeToSfxChanged()
    {
        if (_sfxChangedSubscribed || _volumeSettings == null) return;

        _volumeSettings.SfxChanged += OnSfxVolumeChanged;
        _sfxChangedSubscribed = true;
    }

    private void UnsubscribeFromSfxChanged()
    {
        if (!_sfxChangedSubscribed || _volumeSettings == null) return;

        _volumeSettings.SfxChanged -= OnSfxVolumeChanged;
        _sfxChangedSubscribed = false;
    }

    private void OnSfxVolumeChanged(int _) => ApplySfxVolume();

    private float SfxVolumeScale => (_volumeSettings?.SfxVolume ?? 100) / 100f;

    private void ApplySfxVolume()
    {
        sfxSource.volume = SfxVolumeScale;

        if (loopSfxSource.isPlaying)
            loopSfxSource.volume = _loopClipVolume * SfxVolumeScale;
    }

    private void Update()
    {
        if (Time.timeScale != 0f) return;

        if (loopSfxSource.isPlaying)
            ResetLoopSound();
    }

    public void PlayActionSound(PlayerAction action)
    {
        foreach (var sound in actionSounds)
        {
            if (sound.action == action && sound.clip != null)
            {
                float volume = sound.volume > 0 ? sound.volume : 1f;
                sfxSource.PlayOneShot(sound.clip, volume);
                return;
            }
        }
    }
    public void PlayLoopSound(PlayerAction action)
    {
        if (Time.timeScale == 0f) return;

        foreach (var sound in actionSounds)
        {
            if (sound.action == action && sound.clip != null)
            {
                if (loopSfxSource.clip == sound.clip && loopSfxSource.isPlaying) return;

                loopSfxSource.clip = sound.clip;
                _loopClipVolume = sound.volume > 0 ? sound.volume : 1f;
                loopSfxSource.volume = _loopClipVolume * SfxVolumeScale;
                loopSfxSource.loop = true;
                loopSfxSource.Play();
                return;
            }
        }
    }
    public void ResetLoopSound()
    {
        if (loopSfxSource.isPlaying)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            loopSfxSource.Stop();
            loopSfxSource.clip = null;
        }
    }
    public void StopLoopSound()
    {
        if (loopSfxSource.isPlaying)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            fadeOutCoroutine = StartCoroutine(FadeOutRoutine(0.1f));
        }
    }
    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = loopSfxSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            loopSfxSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);

            yield return null;
        }

        loopSfxSource.Stop();
        loopSfxSource.clip = null;

        loopSfxSource.volume = startVolume;
        fadeOutCoroutine = null;
    }
}