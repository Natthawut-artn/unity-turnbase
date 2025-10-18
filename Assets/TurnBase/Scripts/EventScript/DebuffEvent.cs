using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffEvent : MonoBehaviour
{
    private GameObject player;
    private bool getDeBuff;
    private bool alreadyBuffed = false;
    private GameObject panelToClose; // Stores the panel reference passed from the UI

    private void Start()
    {
        // Find player once in Start()
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // Apply debuff when triggered
        if (getDeBuff)
        {
            DeBuffPlayer();

            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }

            getDeBuff = false;
        }
    }

    // This function is called by the UI Button, passing the panel reference
    public void ChangeDeBuff(GameObject panelRef)
    {
        getDeBuff = true;
        panelToClose = panelRef;
    }

    private void DeBuffPlayer()
    {
        if (alreadyBuffed) return;

        // ⭐ THE FIX: Check if the player object still exists before accessing its components.
        if (player == null)
        {
            // Attempt to find the player again. This handles scene transitions or respawns.
            player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                // If we still can't find the player, log the warning and exit.
                Debug.LogWarning("Cannot apply buff: Player object is missing or has been destroyed.");
                return;
            }
        }

        var health = player.GetComponent<PlayerData>();
        if (health != null)
        {
            health.currentHP = 50;
            Debug.Log("Debuff Applied Player Health = " + health.currentHP);
            // Assuming HPBarsHUDController.Update() is a static method
            HPBarsHUDController.Update(); 
            alreadyBuffed = true;
        }
        else
        {
            Debug.LogWarning("PlayerData component not found on Player!");
        }
    }
}