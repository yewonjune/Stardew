using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource bgmSource;

    public AudioSource sfxSource;
    public List<AudioClip> sfxClips;

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
        }
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();

    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySFX(string clipName, float volume = 0.5f)
    {
        if (sfxSource == null) return;

        AudioClip clip = sfxClips.Find(c => c != null && c.name == clipName);
        if (clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }
}
