using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoomTriggerEvent : MonoBehaviour
{
    public GameObject EventPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventPanel.SetActive(true);
        }
    }

    public void CloseEventPanel()
    {
        EventPanel.SetActive(false);
    }
}
