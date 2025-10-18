using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMoving : MonoBehaviour
{
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Close Enough!!");
            
            TurnManager.Instance.currentPhase = Phase.Player;
            Debug.Log("Player Turn");
        }
    }

}

