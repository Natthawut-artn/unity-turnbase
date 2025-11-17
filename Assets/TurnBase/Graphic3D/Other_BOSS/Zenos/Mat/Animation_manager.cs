using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_manager: MonoBehaviour
{
    public GameObject VFX;

    public void Open()
    {

        VFX.SetActive(true);

    }

    public void close()
    {

        VFX.SetActive(false);

    }


}
