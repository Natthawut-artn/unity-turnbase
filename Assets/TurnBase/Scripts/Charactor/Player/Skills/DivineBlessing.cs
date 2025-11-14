using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DivineBlessing : Skill
{
    private int BuffTurnAmount = 2;
    public DivineBlessing()
    {
        SkillName = "Divine Blessing";
        SkillSprite_Path = "Graphic2D/Skill/Arcanist15";
        Description = "";
        UseSkillPoints = 3;
    }

    public override void UseSkill(GameObject player = null)
    {
        if(SkillPointsManager.currentSkillPoint < UseSkillPoints){ Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
        for (int i = 0; i < GameManager.Instance.Players_alive.Count; i++)
        {
            PlayerData data = GameManager.Instance.Players_alive[i].GetComponent<PlayerData>();
            data.StatusBuffTurnAmount = BuffTurnAmount + 1;
        }
        Activate(null , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        if (!isActivate) return;
        PlayerData playerData = null;
        if (player.name.Contains("Striker"))
        {
            playerData = player.GetComponent<PlayerData>();
        }
        else if (player.name.Contains("Support"))
        {
            playerData = player.GetComponent<PlayerData>();
        }
        else if (player.name.Contains("Protector"))
        {
            playerData = player.GetComponent<PlayerData>();
        }

        if (playerData.StatusBuffTurnAmount > 0 && !playerData.alreadySetUp)
        {
            if (player.name.Contains("Striker"))
            {
                playerData.OldCritRateStatus = (int)(0.2f * playerData.currentCritRate);
                playerData.OldCritDGMStatus = (int)(0.4f * playerData.currentCritDGM);
                playerData.currentCritRate += playerData.OldCritRateStatus;
                playerData.currentCritDGM += playerData.OldCritDGMStatus;
            }
            else if (player.name.Contains("Support"))
            {
                playerData.OldATKStatus = (int)(0.2f * playerData.currentATK);
                playerData.OldHpStatus = (int)(0.2f * playerData.currentHP);
                playerData.currentATK += playerData.OldATKStatus;
                playerData.currentHP += playerData.OldHpStatus;
            }
            else if (player.name.Contains("Protector"))
            {
                playerData.OldDefenceStatus = (int)(0.2f * playerData.currentDefense);
                playerData.OldHpStatus = (int)(0.2f * playerData.currentHP);
                playerData.currentDefense += playerData.OldDefenceStatus;
                playerData.currentHP += playerData.OldHpStatus;
            }
            playerData.alreadySetUp = true;
        }

        if (playerData.StatusBuffTurnAmount > 0)
        {
            playerData.StatusBuffTurnAmount--;
        }

        if (playerData.StatusBuffTurnAmount <= 0 && playerData.alreadySetUp)
        {
            if (player.name.Contains("Striker"))
            {
                playerData.currentCritRate -= playerData.OldCritRateStatus;
                playerData.currentCritDGM -= playerData.OldCritDGMStatus;
            }
            else if (player.name.Contains("Support"))
            {
                playerData.currentATK -= playerData.OldATKStatus;
                playerData.currentHP -= playerData.OldHpStatus;
            }
            else if (player.name.Contains("Protector"))
            {
                playerData.currentDefense -= playerData.OldDefenceStatus;
                playerData.currentHP -= playerData.OldHpStatus;
            }
            playerData.alreadySetUp = false;
            playerData.StatusBuffTurnAmount = 0;
        }
        
        int[] ints = new int[3];
        for (int i = 0; i < GameManager.Instance.Players_alive.Count; i++)
        {
            PlayerData data = GameManager.Instance.Players_alive[i].GetComponent<PlayerData>();
            ints[i] = data.StatusBuffTurnAmount;
        }
        if(ints.All(x => x == 0))
        {
            isActivate = false;
        }
    }
}