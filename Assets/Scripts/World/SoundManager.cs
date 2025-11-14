using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;

    [SerializeField]
    private AudioSource sfx2DSource;
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
    }
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

        sfx2DSource.PlayOneShot(clip);
    }
}
