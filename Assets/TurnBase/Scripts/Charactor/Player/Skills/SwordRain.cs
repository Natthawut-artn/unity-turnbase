using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordRain : Skill
{
    private int currentAttack = 0;
    private int maxAttack = 10;
    public SwordRain()
    {
        SkillName = "Sword Rain";
        SkillSprite_Path = "Graphic2D/Skill/Enchanter2";
        Description = "";
        UseSkillPoints = 3;
    }

    public override void UseSkill(GameObject player = null)
    {
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
        Debug.Log($"{player.name} Use Sword Rain");
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        GameObject target = controller.markertarget;

        PlayerData playerData = player.GetComponent<PlayerData>();
        float damage = 0.05f * (float)playerData.currentATK;
        if (damage < 1f)
        {
            damage = 1f;
        }

        if (currentAttack < maxAttack)
        {
            Collider[] hits = Physics.OverlapBox(target.transform.position, new Vector3(0.505f, 0.505f , 0.505f), target.transform.rotation, controller.Enemy);
            foreach (Collider hit in hits)
            {
                EnemyData enemyData = hit.gameObject.GetComponent<EnemyData>();
                enemyData.TakeDamage((int)damage);
            }
            currentAttack++;
            if (currentAttack >= maxAttack)
            {
                isActivate = false;
                currentAttack = 0;
            }
        }
        Debug.Log($"Sword Rain for {currentAttack} time");
    }
}
 