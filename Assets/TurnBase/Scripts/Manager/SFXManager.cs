using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFX
{
    
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    void Awake()
    {
        Instance = this;
    }

    private AudioSource audioSource;
    public List<AudioClip> SFXList = new List<AudioClip>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(SFX sfx)
    {
        if (audioSource == null) { Debug.LogError("Have no Speaker"); return; }
        Play((int)sfx);
    }

    public void Play(int index)
    {
        audioSource.Stop();
        audioSource.clip = SFXList[index];
        audioSource.Play();
    }

    public void PlayOneShot(SFX sfx)
    {
        audioSource.PlayOneShot( SFXList[(int)sfx], 1);
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
