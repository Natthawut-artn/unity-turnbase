using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffEvent : MonoBehaviour
{
    private GameObject player;
    public bool getDeBuff;
    private bool alreadyBuffed = false;
    private GameObject panelToClose; // Stores the panel reference passed from the UI
    public GameObject debuffText;

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
            PlayAnimationText();
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

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                health.currentHP = 50;
                Debug.Log($"Debuff Applied {player.name}'s Health = {health.currentHP}");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }

    public void PlayAnimationText()
    {
        StartCoroutine(ShowBuffText());
    }

    private IEnumerator ShowBuffText()
    {
        debuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        debuffText.SetActive(false);
    }
}