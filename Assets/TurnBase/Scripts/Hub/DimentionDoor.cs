using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimentionDoor : MonoBehaviour
{
    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartGamePopupController.Show();    
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
