using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffEvent : MonoBehaviour
{
    private GameObject player;
    private bool getBuff;
    private bool alreadyBuffed = false;
    private GameObject panelToClose; // Stores the panel reference passed from the UI

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");  
    }

    private void Update()
    {
        // Apply buff when triggered
        if (getBuff)
        {
            BuffPlayer();

            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }

            getBuff = false;
        }
    }

    // This function is called by the UI Button, passing the panel reference
    public void ChangeBuff(GameObject panelRef)
    {
        getBuff = true;
        panelToClose = panelRef;
    }

    private void BuffPlayer()
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

        var attack = player.GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.attackDamage += 20;
            Debug.Log("Buff applied! Player damage = " + attack.attackDamage);
            alreadyBuffed = true;
        }
        else
        {
            Debug.LogWarning("PlayerAttack component not found on Player!");
        }
    }
}