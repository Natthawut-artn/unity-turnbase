using System.Collections;
using System.Collections.Generic;
// using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

public class BuffEvent : MonoBehaviour
{
    private GameObject player;
    public bool firstRelic;

    public bool getBuff;
    public bool getCritBuff;
    public bool getDefBuff;
    public bool getHpBuffButDefDebuff;
    public bool getAtkBuffCritDebuff;
    public bool getCritDmgBuffHpDebuff;
    public bool getBuffCritDmgDefDebuff;
    public bool getAtkDebuffCritDmgBuff;
    public bool getArelic;
    public bool getTworelics;
    public bool getHugeAtkBuffHPDebuff;
    public bool getHugeCritDMGBuffDefDebuff;
    public bool getHugeCritRateBuffATKDebuff;
    private bool alreadyBuffed = false;
   

    private GameObject panelToClose; // Stores the panel reference passed from the UI
    public GameObject buffText;
    public GameObject healText;
    public GameObject DefText;
    public GameObject CRtext;
    public GameObject CDtext;
    public GameObject GARtext;
    public GameObject GTRtext;

    //BuffandDebuffGameobject

    public GameObject AtkBuffCRDebuffText;
    public GameObject AtkbuffHpDebuffText;
    public GameObject AtkDebffCrBuffText;
    public GameObject CDBuffAtkDebuffText;
    public GameObject DefBuffCDDebuffText;
    public GameObject DefDebuffCDbuffText;
    public GameObject HpBuffCDDebuffText;
    public GameObject HpBuffDefDebuff;
    public GameObject HpDebuffCDbuff;

    private bool getheal = false;
    private bool healed = false;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");  
        firstRelic = false;
    }

    private void Update()
    {
        if (getHugeAtkBuffHPDebuff)
        {
            PlayAtkBuffHpDebuffAnimationText();
            HughATKBuffHpDebuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getHugeAtkBuffHPDebuff = false;
        }
        if(getHugeCritDMGBuffDefDebuff)
        {
            PlayDefbuffCDDebuffText();
            HugeCritDMGBuffDefDebuff();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getHugeCritDMGBuffDefDebuff = false;
        }
        if(getHugeCritRateBuffATKDebuff)
        {
            PlayatkDebuffCRbuffText();
            HugeCritDMGBuffDefDebuffr();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getHugeCritRateBuffATKDebuff = false;
        }
        if (getTworelics)
        {
            PlayGTRAnimationText();
            GetTworelic();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getTworelics = false;
        }
        if(getArelic)
        {
            PlayGARAnimationText();
            GetArelic();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getArelic = false;
        }
        if (getBuffCritDmgDefDebuff)
        {
            PlaydefDebuffCDbuffText();
            CritDmgdeBuffDefbuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getBuffCritDmgDefDebuff = false;
        }
        if(getAtkDebuffCritDmgBuff)
        {
            PlayCDbuffATKDebuffText();  
            ATKdeBuffCritDMGbuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getAtkDebuffCritDmgBuff = false;
        }
        if (getCritDmgBuffHpDebuff)
        {
            PlayHpDebuffCDbuffText();
            CritDmgBuffHpDebuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getCritDmgBuffHpDebuff = false;
        }
        if (getAtkBuffCritDebuff)
        {
            PlayAtkBuffCRDebuffAnimationText();
            AtkBuffCritDebuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getAtkBuffCritDebuff = false;
        }
        if (getHpBuffButDefDebuff)
        {
            PlayHpbuffDefDebuffText();
            HpBuffDefDebuffPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getHpBuffButDefDebuff = false;
        }
        if (getDefBuff)
        {
            PlayDefAnimationText();
            BuffDefPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getDefBuff = false;
        }
        if (getCritBuff)
        {
            PlayCRAnimationText();
            BuffCritPlayer();
            if (panelToClose != null)
            {
                panelToClose.SetActive(false);
                panelToClose = null;
            }
            getCritBuff = false;
        }
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
    public void Critplayer(GameObject panelRef)
    {
        if (getCritBuff == true) return;
        getCritBuff = true;
        panelToClose = panelRef;
    }
    public void Defplayer(GameObject panelRef)
    {
        if (getDefBuff == true) return;
        getDefBuff = true;
        panelToClose = panelRef;
    }
    public void HpBuffDefDebuffPlayer(GameObject panelRef)
    {
        if (getHpBuffButDefDebuff == true) return;
        getHpBuffButDefDebuff = true;
        panelToClose = panelRef;
    }
    public void AtkBuffCritDebuffPlayer(GameObject panelRef)
    {
        if (getHpBuffButDefDebuff == true) return;
        getHpBuffButDefDebuff = true;
        panelToClose = panelRef;
    }
    public void CritDmgBuffHpDebuffPlayer(GameObject panelRef)
    {
        if (getCritDmgBuffHpDebuff == true) return;
        getCritDmgBuffHpDebuff = true;
        panelToClose = panelRef;
    }
    public void CritDmgDeBuffDefbuffPlayer(GameObject panelRef)
    {
        if (getBuffCritDmgDefDebuff == true) return;
        getBuffCritDmgDefDebuff = true;
        panelToClose = panelRef;
    }
    public void AtkDeBuffCritDmgbuffPlayer(GameObject panelRef)
    {
        if (getAtkDebuffCritDmgBuff == true) return;
        getAtkDebuffCritDmgBuff = true;
        panelToClose = panelRef;
    }
    public void ArelicBuff(GameObject panelRef)
    {
        if (getArelic == true) return;
        getArelic = true;
        panelToClose = panelRef;
    }
    public void TworelicsBuff(GameObject panelRef)
    {
        if (getTworelics == true) return;
        panelToClose = panelRef;
        getTworelics = true;
        
    }
    public void HugeAtkBuffHpDebuff(GameObject panelRef)
    {
        if (getHugeCritDMGBuffDefDebuff == true) return;
        getHugeCritDMGBuffDefDebuff = true;
        panelToClose = panelRef;
    }
    public void HugeCritDMGBuffDefDebuff(GameObject panelRef)
    {
        if (getHugeAtkBuffHPDebuff == true) return;
        getHugeAtkBuffHPDebuff = true;
        panelToClose = panelRef;
    }
    public void HugeCritrateBuffATKDebuff(GameObject panelRef)
    {
        if (getHugeCritRateBuffATKDebuff == true) return;
        getHugeCritRateBuffATKDebuff = true;
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
                health.HP += 10;
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
                attack.currentATK += 10;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentATK}");
            }
        }
        alreadyBuffed = true;
    }
    private void BuffCritPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentCritRate += 20;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentCritRate}");
            }
        }
        alreadyBuffed = true;
    }
    private void BuffDefPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentDefense += 10;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void HpBuffDefDebuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentDefense -= 5;
                attack.HP += 20;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void AtkBuffCritDebuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentATK += 10;
                attack.currentCritRate -= 5;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void CritDmgBuffHpDebuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.HP -= 20;
                attack.currentCritDGM += 5;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void CritDmgdeBuffDefbuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentCritDGM -= 5;
                attack.currentDefense += 5;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void ATKdeBuffCritDMGbuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                if(attack.currentATK > 0) 
                { 
                    attack.currentATK -= 10;
                    if (attack.currentATK < 0)
                    {
                        attack.currentATK = 0; // Ensure ATK does not go below 0
                    }
                }
                else
                {
                    Debug.Log("ATK cannot be decreased further.");
                }
                attack.currentCritDGM += 5;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void GetArelic()
    {
        if (alreadyBuffed) return;
        RelicStroageManager.RandomRelic();
        alreadyBuffed = true;
    }
    private void GetTworelic()
    {
        if (alreadyBuffed) return;
        // 2. Just call the function twice
        RelicStroageManager.RandomRelic();
        RelicStroageManager.RandomRelic();

        // 3. Mark as done
        alreadyBuffed = true;

        // Optional: Log it so you can see it in console
        Debug.Log("Two Relics Given!");
       

    }
    private void HughATKBuffHpDebuffPlayer()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentATK += 30;
                attack.currentHP -= 30;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void HugeCritDMGBuffDefDebuff()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                attack.currentDefense -= 20;
                attack.currentCritDGM += 40;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
            }
        }
        alreadyBuffed = true;
    }
    private void HugeCritDMGBuffDefDebuffr()
    {
        if (alreadyBuffed) return;

        foreach (GameObject player in GameManager.Instance.Players_alive)
        {
            if (player == null) continue;

            var attack = player.GetComponent<PlayerData>();
            if (attack != null)
            {
                if (attack.currentATK > 0)
                {
                    attack.currentATK -= 30;
                    if (attack.currentATK < 0)
                    {
                        attack.currentATK = 0; // Ensure ATK does not go below 0
                    }
                }
                else
                {
                    Debug.Log("ATK cannot be decreased further.");
                }
                attack.currentCritRate += 40;
                Debug.Log($"Buff applied! {player.name}'s ATK = {attack.currentDefense}");
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
    public void PlayDefAnimationText()
    {
        StartCoroutine(ShowDefText());
    }
    public void PlayCRAnimationText()
    {
        StartCoroutine(ShowCRText());
    }
    public void PlayCDAnimationText()
    {
        StartCoroutine(ShowCDText());
    }
    public void PlayGARAnimationText()
    {
        StartCoroutine(ShowGARText());
    }
    public void PlayGTRAnimationText()
    {
        StartCoroutine(ShowGTRText());
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
    IEnumerator ShowDefText()
    {
        DefText.SetActive(true);
        yield return new WaitForSeconds(4f);
        DefText.SetActive(false);
    }
    IEnumerator ShowCRText()
    {
        CRtext.SetActive(true);
        yield return new WaitForSeconds(4f);
        CRtext.SetActive(false);
    }
    IEnumerator ShowCDText()
    {
        CDtext.SetActive(true);
        yield return new WaitForSeconds(4f);
        CDtext.SetActive(false);
    }
    IEnumerator ShowGARText()
    {
        GARtext.SetActive(true);
        yield return new WaitForSeconds(4f);
        GARtext.SetActive(false);
    }
    IEnumerator ShowGTRText()
    {
        GTRtext.SetActive(true);
        yield return new WaitForSeconds(4f);
        GTRtext.SetActive(false);
    }
    //BuffAndDebuffTextAnim

    public void PlayAtkBuffCRDebuffAnimationText()
    {
        StartCoroutine(atkBuffCRDebuffText());
    }
    public void PlayAtkBuffHpDebuffAnimationText()
    {
        StartCoroutine(atkBuffHpDebuffText());
    }
    public void PlayatkDebuffCRbuffText()
    {
        StartCoroutine(atkDebuffCRbuffText());
    }
    public void PlayCDbuffATKDebuffText()
    {
        StartCoroutine(CDbuffATKDebuffText());
    }
    public void PlayDefbuffCDDebuffText()
    {
        StartCoroutine(DefbuffCDDebuffText());
    }
    public void PlaydefDebuffCDbuffText()
    {
        StartCoroutine(defDebuffCDbuffText());
    }
    public void PlayHpbuffCDDebuffText()
    {
        StartCoroutine(HpbuffCDDebuffText());
    }
    public void PlayHpbuffDefDebuffText()
    {
        StartCoroutine(HpbuffDefDebuffText());
    }
    public void PlayHpDebuffCDbuffText()
    {
        StartCoroutine(HpDebuffCDbuffText());
    }
    IEnumerator atkBuffCRDebuffText()
    {
        AtkBuffCRDebuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        AtkBuffCRDebuffText.SetActive(false);
    }
    IEnumerator atkBuffHpDebuffText()
    {
        AtkbuffHpDebuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        AtkbuffHpDebuffText.SetActive(false);
    }
    IEnumerator atkDebuffCRbuffText()
    {
        AtkDebffCrBuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        AtkDebffCrBuffText.SetActive(false);
    }

    IEnumerator CDbuffATKDebuffText()
    {
        CDBuffAtkDebuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        CDBuffAtkDebuffText.SetActive(false);
    }
    IEnumerator DefbuffCDDebuffText()
    {
        DefBuffCDDebuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        DefBuffCDDebuffText.SetActive(false);
    }
    IEnumerator defDebuffCDbuffText()
    {
        DefDebuffCDbuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        DefDebuffCDbuffText.SetActive(false);
    }
    IEnumerator HpbuffCDDebuffText()
    {
        HpBuffCDDebuffText.SetActive(true);
        yield return new WaitForSeconds(4f);
        HpBuffCDDebuffText.SetActive(false);
    }
    IEnumerator HpbuffDefDebuffText()
    {
        HpBuffDefDebuff.SetActive(true);
        yield return new WaitForSeconds(4f);
        HpBuffDefDebuff.SetActive(false);
    }
    IEnumerator HpDebuffCDbuffText()
    {
        HpDebuffCDbuff.SetActive(true);
        yield return new WaitForSeconds(4f);
        HpDebuffCDbuff.SetActive(false);
    }
}