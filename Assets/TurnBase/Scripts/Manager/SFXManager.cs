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

    public bool userSetting = false;
    public float usermusicvolume = 0f;
    public float usersfxvolume = 0f;

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
        if (userSetting)
        {
            BGMusic.volume = usermusicvolume * MusicVolump[(int)music];
        }
        BGMusic.Play();
    }

    public void PlayOneShot(SFX sfx)
    {
        float volume = 1f;
        if (userSetting)
        {
            volume = usersfxvolume * SFXVolump[(int)sfx];
        }
        SFX.PlayOneShot( SFXList[(int)sfx], volume);
    }

    public void Stop()
    {
        BGMusic.Stop();
    }

    public void usersetting()
    {
        AudioClip currentClip = BGMusic.clip;
        int index = GetClipIndex(currentClip);
        BGMusic.volume = usermusicvolume * MusicVolump[index];
    }

    int GetClipIndex(AudioClip clip)
    {
        for (int i = 0; i < MusicList.Count; i++)
        {
            if (MusicList[i] == clip)  
                return i;
        }
        return 1;
    }
}
