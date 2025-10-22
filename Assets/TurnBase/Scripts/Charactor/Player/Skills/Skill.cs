using UnityEngine;

public abstract class Skill
{
    public string SkillName;
    public string SkillSprite_Path;
    public string Description;
    public int UseSkillPoints;
    public bool isActivate = false;

    public abstract void Activate(PlayerData player);
}