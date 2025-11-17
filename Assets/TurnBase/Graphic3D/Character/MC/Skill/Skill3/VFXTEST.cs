using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTEST : MonoBehaviour
{
    VisualEffect vfx;
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        StartCoroutine(HideAfter(3f)); 
    }

    IEnumerator HideAfter(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
