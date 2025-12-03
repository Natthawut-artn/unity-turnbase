using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffEvent : MonoBehaviour
{
    private GameObject player;
    public bool getDeBuff;
    public bool getAtkDeBuff;
    public bool getDefDeBuff;
    public bool getCritDeBuff;
    public bool getDebuffAllstatus;
    public bool getsmallDebuffAllstatus;
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
        if(getsmallDebuffAllstatus)
        {
            PlayAnimationText();
            smallDebuffAllstatus();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getsmallDebuffAllstatus = false;
        }
        if (getDebuffAllstatus)
        {
            PlayAnimationText();
            DebuffAllstatus();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getDebuffAllstatus = false;
        }
        if (getCritDeBuff)
        {
            PlayAnimationText();
            DeBuffCritPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getCritDeBuff = false;
        }
        if (getDefDeBuff)
        {
            PlayAnimationText();
            DeBuffDefPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getDefDeBuff = false;
        }
        if (getAtkDeBuff)
        {
            PlayAnimationText();
            DeBuffAtkPlayer();

            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }

            getAtkDeBuff = false;
        }
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
    public void ChangeAtkDeBuff(GameObject panelRef)
    {
        getAtkDeBuff = true;
        panelToClose = panelRef;
    }
    public void ChangeDefDeBuff(GameObject panelRef)
    {
        getDefDeBuff = true;
        panelToClose = panelRef;
    }
    public void ChangeCritDeBuff(GameObject panelRef)
    {
        getCritDeBuff = true;
        panelToClose = panelRef;
    }
    public void ChangeAllStatusDeBuff(GameObject panelRef)
    {
        getDebuffAllstatus = true;
        panelToClose = panelRef;
    }
    public void ChangesmallAllStatusDeBuff(GameObject panelRef)
    {
        getsmallDebuffAllstatus = true;
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
                health.HP -= 10;
                Debug.Log($"Debuff Applied {player.name}'s Health = {health.HP}");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }
    private void DeBuffAtkPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                if (health.currentATK > 0)
                {
                    health.currentATK -= 10;
                    if (health.currentATK < 0)
                    {
                        health.currentATK = 0; // Ensure ATK does not go below 0
                    }
                }
                else
                {
                    Debug.Log("ATK cannot be decreased further.");
                }
                Debug.Log($"Debuff Applied {player.name}'s Health = {health.currentATK}");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }
    private void DeBuffDefPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                health.currentDefense -= 20;
                Debug.Log($"Debuff Applied {player.name}'s Health = {health.currentDefense}");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }
    private void DeBuffCritPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                health.currentCritRate -= 20;
                Debug.Log($"Debuff Applied {player.name}'s Health = {health.currentCritRate}");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }
    private void DebuffAllstatus()
    {
        if (alreadyBuffed) return;
        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;
            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                if (health.currentATK > 0)
                {
                    health.currentATK -= 10;
                    if (health.currentATK < 0)
                    {
                        health.currentATK = 0; // Ensure ATK does not go below 0
                    }
                }
                else
                {
                    Debug.Log("ATK cannot be decreased further.");
                }
                health.currentCritDGM -= 10;
                health.currentDefense -= 10;
                health.currentCritRate -= 10;
                health.currentHP -= 10;
                health.currentCritDGM -= 10; ;
                Debug.Log($"Debuff Applied {player.name}'s All Status");
                HPBarsHUDController.Update();
            }
        }
        alreadyBuffed = true;
    }
    private void smallDebuffAllstatus()
    {
        if (alreadyBuffed) return;
        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;
            var health = player.GetComponent<PlayerData>();
            if (health != null)
            {
                if (health.currentATK > 0)
                {
                    health.currentATK -= 5;
                    if (health.currentATK < 0)
                    {
                        health.currentATK = 0; // Ensure ATK does not go below 0
                    }
                }
                else
                {
                    Debug.Log("ATK cannot be decreased further.");
                }
                health.currentCritDGM -= 5;
                health.currentDefense -= 5;
                health.currentCritRate -= 5;
                health.currentHP -= 5;
                health.currentCritDGM -= 5; 
                Debug.Log($"Debuff Applied {player.name}'s All Status");
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