using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    void Awake()
    {
        Instance = this;
    }

    public AudioSource BGMusic;
    public AudioSource SFX;

    [Header("Music")]
    public List<AudioClip> MusicList = new List<AudioClip>();
    public List<float> MusicVolump = new List<float>();

    [Header("SFX")]
    public List<AudioClip> SFXList = new List<AudioClip>();
    public List<float> SFXVolump = new List<float>();

    public void PlayMusic(Music music)
    {
        if (BGMusic == null) { Debug.LogError("Have no Speaker"); return; }
        BGMusic.Stop();
        BGMusic.clip = MusicList[(int)music];
        BGMusic.volume = MusicVolump[(int)music];
        BGMusic.Play();
    }

    public void PlayOneShot(SFX sfx)
    {
        SFX.PlayOneShot( SFXList[(int)sfx], SFXVolump[(int)sfx]);
    }

    public void Stop()
    {
        BGMusic.Stop();
    }
}
