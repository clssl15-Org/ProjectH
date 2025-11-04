using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXSoundType
{
    Attack1, Attack2, Attack3, Eskill, Ultimate, Dash, Hit,
}
public enum BGMSoundType
{
    BGM_Stage1, BGM_Stage2,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource[] sfxSources;
    [SerializeField] private AudioSource uiSFXSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.5f;

    private int sfxChannelIndex = 0;
    private Dictionary<BGMSoundType, AudioClip> bgmAudioClipDict;
    private Dictionary<SFXSoundType, AudioClip> sfxAudioClipDict;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSoundManager();
    }

    #region UI SFX Methods
    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, pos);
    }
    public void PlaySound3D(string soundName, Vector3 pos)
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        PlaySound3D(clip, pos);
    }
    public void PlaySound2D(string soundName)
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);

        if (clip == null) return;

        uiSFXSource.PlayOneShot(clip);
    }
    #endregion

    private void InitializeSoundManager()
    {
        // 오디오 클립 딕셔너리 초기화
        bgmAudioClipDict = new Dictionary<BGMSoundType, AudioClip>();
        sfxAudioClipDict = new Dictionary<SFXSoundType, AudioClip>();

        // BGM 오디오 소스 설정
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Player");
            bgmObj.transform.parent = transform;
            bgmSource = bgmObj.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        // SFX 오디오 소스 설정
        if (sfxSources == null || sfxSources.Length == 0)
        {
            GameObject sfxObj = new GameObject("SFX_Player");
            sfxObj.transform.parent = transform;

            sfxSources = new AudioSource[5]; // 기본 5개 채널
            for (int i = 0; i < sfxSources.Length; i++)
            {
                sfxSources[i] = sfxObj.AddComponent<AudioSource>();
                sfxSources[i].playOnAwake = false;
                sfxSources[i].loop = false;
                sfxSources[i].volume = sfxVolume;
            }
        }

        // BGM 오디오 클립 딕셔너리에 등록
        foreach (AudioClip clip in bgmClips)
        {
            if (clip == null)
            {
                continue;
            }

            if (System.Enum.TryParse<BGMSoundType>(clip.name, out BGMSoundType bgmType))
            {
                bgmAudioClipDict.Add(bgmType, clip);
            }
            else
            {
                Debug.LogWarning($"BGM Clip '{clip.name}' does not match any BGMSoundType enum.");
            }
        }

        // SFX 오디오 클립 딕셔너리에 등록
        foreach (AudioClip clip in sfxClips)
        {
            if (clip == null)
            {
                continue;
            }

            if (System.Enum.TryParse<SFXSoundType>(clip.name, out SFXSoundType sfxType))
            {
                sfxAudioClipDict.Add(sfxType, clip);
            }
            else
            {
                Debug.LogWarning($"SFX Clip '{clip.name}' does not match any SFXSoundType enum.");
            }
        }
    }

    // BGM 재생
    public void PlayBGM(BGMSoundType bgmType)
    {
        if (!bgmAudioClipDict.ContainsKey(bgmType))
            return;

        bgmSource.clip = bgmAudioClipDict[bgmType];
        bgmSource.Play();
    }

    // BGM 중지
    public void StopBGM()
    {
        bgmSource.Stop();
    }
    // SFX 재생
    public void PlaySFX(SFXSoundType sfxType)
    {
        if (!sfxAudioClipDict.ContainsKey(sfxType))
            return;

        AudioSource selectedSource = null;

        // 재생 중이지 않은 오디오 소스를 찾음
        for (int i = 0; i < sfxSources.Length; i++)
        {
            int index = (i + sfxChannelIndex) % sfxSources.Length;

            if (!sfxSources[index].isPlaying)
            {
                selectedSource = sfxSources[index];
                sfxChannelIndex = (index + 1) % sfxSources.Length;
                break;
            }
        }

        // 모든 소스가 사용 중이면 가장 오래된 것을 사용
        if (selectedSource == null)
        {
            selectedSource = sfxSources[sfxChannelIndex];
            sfxChannelIndex = (sfxChannelIndex + 1) % sfxSources.Length;
        }

        selectedSource.clip = sfxAudioClipDict[sfxType];

        selectedSource.Play();
    }

    // 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        foreach (AudioSource source in sfxSources)
        {
            source.volume = volume;
        }
    }
}
