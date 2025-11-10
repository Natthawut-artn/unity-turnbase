using UnityEngine;

public class Shunpo : Skill
{
    public Shunpo()
    {
        SkillName = "Shunpo";
        SkillSprite_Path = "";
        Description = "";
        UseSkillPoints = 2;
    }

    public override void UseSkill(GameObject player)
    {
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
    }

    public override void Activate(GameObject enemy, GameObject player)
    {
        if (!isActivate) return;
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null) return;

        Vector3 targetPos = controller.markertarget.transform.position;

        controller.DashTo(targetPos);

        isActivate = false;
    }
}