using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private AudioSource audioSource;
    public List<AudioClip> SFXList = new List<AudioClip>();
    public List<float> Volump = new List<float>();

    void Start()
    {
    }

    public void Play(SFX sfx)
    {
        if (audioSource == null) { Debug.LogError("Have no Speaker"); return; }
        StartCoroutine(Play((int)sfx));
    }

    IEnumerator Play(int index)
    {
        yield return null;
        // audioSource.Stop();
        audioSource.clip = SFXList[index];
        audioSource.volume = Volump[index];
        audioSource.Play();
    }

    public void PlayOneShot(SFX sfx)
    {
        audioSource.PlayOneShot( SFXList[(int)sfx], Volump[(int)sfx]);
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
