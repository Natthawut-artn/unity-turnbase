using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStopMoving : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the stop zone.");
            // Implement logic to stop player movement
            // For example, you might disable the player's movement script here
            // other.GetComponent<PlayerMovement>().enabled = false;
            PlayerController.Instance.playerData.playerMoveState = PlayerMoveState.Stand;
            TurnManager.Instance.EndTurn();
        }
    }
}
