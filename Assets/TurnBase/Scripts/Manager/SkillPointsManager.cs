using UnityEngine;

public static class SkillPointsManager
{
    public static int currentSkillPoint = 0;
    public static int maxSkillPoint = 6;
    public static void UseSkillPoint(int value)
    {
        if (currentSkillPoint >= value) { currentSkillPoint -= value; }
    }
    public static void ReceiveSkillPoint(int value)
    {
        if (currentSkillPoint < maxSkillPoint) { currentSkillPoint += value; }
        if (currentSkillPoint > maxSkillPoint) { currentSkillPoint = maxSkillPoint; }
    }
    public static void ResetSkillPonit()
    {
        currentSkillPoint = 0;
    }
}