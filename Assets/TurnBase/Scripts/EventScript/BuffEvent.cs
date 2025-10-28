using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffEvent : MonoBehaviour
{
    private GameObject player;
    public bool getBuff;
    private bool alreadyBuffed = false;
    private GameObject panelToClose; // Stores the panel reference passed from the UI
    public GameObject buffText;
    public GameObject healText;
    private bool getheal = false;
    private bool healed = false;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");  
    }

    private void Update()
    {
        // Apply buff when triggered
        if (getBuff)
        {
            PlayAnimationText();
            BuffPlayer();

            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }

            getBuff = false;
        }

        if (getheal)
        {
            PlayHealAnimationText();
            healme();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getheal = false;
        }
    }

    // This function is called by the UI Button, passing the panel reference
    public void ChangeBuff(GameObject panelRef)
    {
        if(getBuff == true) return;
        getBuff = true;
        panelToClose = panelRef;
    }
    public void Healplayer(GameObject panelRef)
    {
        if (getheal == true) return;
        getheal = true;
        panelToClose = panelRef;
    }
    public void healme()
    {
        if (healed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                health.currentHP += 50;
                HPBarsHUDController.Update();
                Debug.Log($"Buff applied! {player.name} health = {health.currentHP}");
            }
        }
        healed = true;
    }
    private void BuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentATK += 35;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentATK}");
            }
        }
        alreadyBuffed = true;
    }

    public void PlayAnimationText()
    {
        StartCoroutine(ShowBuffText());
    }
    public void PlayHealAnimationText()
    {
        StartCoroutine(ShowHealText());
    }
    IEnumerator ShowBuffText()
    {
        buffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        buffText.SetActive(false);
    }
    IEnumerator ShowHealText()
    {
        healText.SetActive(true);
        yield return new WaitForSeconds(4f);
        healText.SetActive(false);
    }
}