using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEvent : MonoBehaviour
{
    public DebuffEvent debuffEventScript;
    public BuffEvent buffEventScript;
    public GameObject panelToClose;
    private bool eventTriggered = false;
    public void RandomEvents()
    {
        if(eventTriggered) return;
        int randomEvent = Random.Range(0, 2);
        if (randomEvent == 0)
        {
            
            debuffEventScript.getDeBuff = true;
            eventTriggered = true;
        }
        else
        {
            buffEventScript.getBuff = true;
            eventTriggered = true;
        }
    }
}
