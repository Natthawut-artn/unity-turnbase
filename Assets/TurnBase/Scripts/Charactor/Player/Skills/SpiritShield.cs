using System.Collections.Generic;
using UnityEngine;

public class SpiritShield : Skill
{
    public SpiritShield()
    {
        SkillName = "Spirit Shield";
        SkillSprite_Path = "Graphic2D/Skill/Paladin14";
        Description = "";
        UseSkillPoints = 1;
    }

    public override void UseSkill(GameObject player = null)
    {
        PlayerData playerData = player?.GetComponent<PlayerData>();
        if(SkillPointsManager.currentSkillPoint < UseSkillPoints){ Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
        Activate(null , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        List<GameObject> team = GameManager.Instance.Players_alive;

        float Shield = (75f / 100f) * (float)player.GetComponent<PlayerData>().Defense;

        foreach (GameObject Player in team)
        {
            PlayerData playerData = Player.GetComponent<PlayerData>();
            playerData.BuffShieldAmountTurn = 3;
            playerData.Shield += (int)Shield;
        }
        isActivate = false;
    }
}