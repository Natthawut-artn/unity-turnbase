using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeilofFate : Skill
{
    private int BonusSkillpoint = 2;
    private int TurnBuffAmount = 1;
    public VeilofFate()
    {
        SkillName = "Veil of Fate";
        SkillSprite_Path = "";
        Description = "";
        UseSkillPoints = 2;
    }
    
    public override void UseSkill(GameObject player = null)
    {
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        Activate();
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        TurnManager.Instance.isTurnBuff = true;
        TurnManager.Instance.TurnBuffAmount += TurnBuffAmount + 1;
        SkillPointsManager.maxSkillPoint += BonusSkillpoint;
    }
}
