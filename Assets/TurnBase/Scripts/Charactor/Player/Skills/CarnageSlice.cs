using UnityEngine;

public class CarnageSlice : Skill
{
    public CarnageSlice()
    {
        SkillName = "Carnage Slice";
        SkillSprite_Path = "";
        Description = "";
        UseSkillPoints = 1;
    }
    
    public override void Activate(PlayerData player)
    {
        // if (!isActivate) return;
        int ATK = player.currentATK;
        int ATK1 = player.currentATK;
        Debug.Log(ATK);
        Debug.Log(ATK1);
    }
}