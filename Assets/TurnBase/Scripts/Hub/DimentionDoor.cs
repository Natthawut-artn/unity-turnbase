using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimentionDoor : MonoBehaviour
{
    public Theme theme;
    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartGamePopupController.Show(theme);    
        }
    }
    void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartGamePopupController.Hide();    
        }
    }
}
