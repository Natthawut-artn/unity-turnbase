using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffEvent : MonoBehaviour
{
    private GameObject EventPanel;
    private GameObject player;
    private bool getDeBuff;

    private void Update()
    {
        // If player not yet found, keep trying until found
        if (EventPanel == null)
        {
            EventPanel = GameObject.FindGameObjectWithTag("PanelEvent");
            if (EventPanel == null) return;
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
        }

        // Apply buff when triggered
        if (getDeBuff)
        {
            DeBuffPlayer();
            getDeBuff = false;
            EventPanel.SetActive(false);
        }
    }

    public void ChangeDeBuff()
    {
        getDeBuff = true;
    }

    private void DeBuffPlayer()
    {
        var health = player.GetComponent<PlayerController>();
        if (health != null)
        {
            health.maxHealth = 50;
            Debug.Log("Buff applied! Player damage = " + health.maxHealth);
        }
        else
        {
            Debug.LogWarning("PlayerAttack component not found on Player!");
        }
    }
}
