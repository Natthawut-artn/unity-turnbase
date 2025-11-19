using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class SwordRain : Skill
{
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

    public override async void Activate(GameObject enemy = null, GameObject player = null)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        GameObject target = controller.markertarget;

        PlayerData playerData = player.GetComponent<PlayerData>();
        float damage = 0.05f * (float)playerData.currentATK;
        if (damage < 1f)
        {
            damage = 1f;
        }

        controller.SetAnimationState(PlayerMoveState.UseSkill3);
        Vector3 Dir = (target.transform.position - player.transform.position).normalized;
        Quaternion Rotation = Quaternion.LookRotation(Dir);
        Rotation *= Quaternion.Euler(0, -65, 0);
        player.transform.rotation = Rotation;
        await Task.Delay(1500);

        VFXManager.Instance.AddVFXOnWorld(target , VFX.Striker_Skill3 , 2f);
        for (int i = 0; i < maxAttack ; i++)
        {
            Collider[] hits = Physics.OverlapBox(target.transform.position, new Vector3(2f, 1f , 2f), target.transform.rotation, controller.Enemy);
            foreach (Collider hit in hits)
            {
                EnemyData enemyData = hit.gameObject.GetComponent<EnemyData>();
                enemyData.TakeDamage((int)damage);
                enemyData.myHealthBar.UpdateEnemyHealthBar(enemyData.HP, enemyData.currentHP);
            }
            await Task.Delay(100); 
        }
        isActivate = false;
        playerData.playerMoveState = PlayerMoveState.Stand;
    }
}
 