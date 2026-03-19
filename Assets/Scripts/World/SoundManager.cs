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
    [Tooltip("효과음을 재생할 오디오 소스")]
    public AudioSource sfxSource;
    public AudioSource loopSfxSource;

    [Header("Sound Clips")]
    public ActionSound[] actionSounds;

    private Coroutine fadeOutCoroutine;

    // 외부에서 사운드를 재생할 때 호출하는 함수
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
        Debug.LogWarning($"[SoundManager] {action}에 해당하는 사운드 클립이 없습니다!");
    }
    // 반복 소리 재생 시작
    public void PlayLoopSound(PlayerAction action)
    {
        foreach (var sound in actionSounds)
        {
            if (sound.action == action && sound.clip != null)
            {
                // 이미 같은 소리가 재생 중이면 무시
                if (loopSfxSource.clip == sound.clip && loopSfxSource.isPlaying) return;

                loopSfxSource.clip = sound.clip;
                loopSfxSource.volume = sound.volume > 0 ? sound.volume : 1f;
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
            // 부드럽게 종료하기
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