using UnityEngine;

public class CarnageSlice : Skill
{
    int BypassDef_BloodFlowing = 70;

    public CarnageSlice()
    {
        SkillName = "Carnage Slice";
        SkillSprite_Path = "";
        Description = "";
        UseSkillPoints = 1;
    }

    public override void UseSkill(GameObject player)
    {
        PlayerData playerData = player?.GetComponent<PlayerData>();
        if(playerData.currentSkillPoint < UseSkillPoints){ Debug.Log("not enough skill point"); return; }
        playerData.ishavebuff = true;
        playerData.BuffAmountTarget = 3;
        playerData.currentSkillPoint -= UseSkillPoints;
        isActivate = true;
    }

    public override void Activate(GameObject enemy, GameObject player)
    {
        EnemyData enemyData = enemy?.GetComponent<EnemyData>();
        PlayerData playerData = player?.GetComponent<PlayerData>();

        if (playerData.BuffAmountTarget <= 0) return;
        playerData.BuffAmountTarget--;
        playerData.BypassDef_BloodFlowing += BypassDef_BloodFlowing;
        if (playerData.BuffAmountTarget <= 0) { playerData.ishavebuff = false; }
        
        int enemyDef = enemyData.Defense + GameManager.Instance.BloodFlowing;
        float MaxHpDGM = (5f / 100f) * (float)enemyData.HP;
        float BaseATK = (25f / 100f) * (float)playerData.currentATK;
        int DGM = (int)Mathf.Round(BaseATK) + (int)Mathf.Round(MaxHpDGM);

        if (playerData.BypassDef_BloodFlowing >= enemyDef)
        {
            playerData.BypassDef_BloodFlowing = 0;
            enemyData.BloodFlowing(DGM);
        }

        isActivate = false;
    }
}