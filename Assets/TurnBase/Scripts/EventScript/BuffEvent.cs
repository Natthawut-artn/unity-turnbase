using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuffEvent : MonoBehaviour
{
    private GameObject EventPanel;
    private GameObject player;
    private bool getBuff;
    private bool alreadyBuffed = false;

    private void Update()
    {
        // If player not yet found, keep trying until found
        if(EventPanel == null)
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
        if (getBuff)
        {
            BuffPlayer();
            getBuff = false;
            EventPanel.SetActive(false);
        }
    }

    public void ChangeBuff()
    {
        getBuff = true;
    }

    private void BuffPlayer()
    {
        if (alreadyBuffed) return; // Prevent multiple buffs
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
