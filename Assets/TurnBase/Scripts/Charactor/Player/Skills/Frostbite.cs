using UnityEngine;
using System.Threading.Tasks;

public class Frostbite : Skill
{
    private Quaternion fixedRotation = Quaternion.identity;

    public Frostbite()
    {
        SkillName = "Frostbite";
        SkillSprite_Path = "Graphic2D/Skill/Frostbite";
        Description = "";
        UseSkillPoints = 2;
    }
    
    public override async void UseSkill(GameObject player = null)
    {
        SFXManager.Instance.PlayOneShot(SFX.Freeze);
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        Debug.Log("Use Frostbite");

        PlayerController controller = player.GetComponent<PlayerController>();
        GameObject target = null;
        Collider[] hits = Physics.OverlapBox(player.transform.position, new Vector3(5f, 1f, 5f), fixedRotation, controller.Enemy);
        if (hits.Length == 0)
        {
            Debug.Log("Enemy out of Range");
            return;
        }
        else
        {
            float closestDistance = Mathf.Infinity;
            Collider closestHit = null;

            foreach (Collider hit in hits)
            {
                float dist = Vector3.Distance(player.transform.position, hit.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestHit = hit;
                }
            }

            if (closestHit != null)
            {
                target = closestHit.gameObject;
                Debug.Log("Target: " + target.name);
            }
        }
        player.transform.LookAt(target.transform);
        controller.SetAnimationState(PlayerMoveState.UseSkill1);
        await Task.Delay(1750);
        VFXManager.Instance.AddVFXOnWorld(player , VFX.Support_Skill1 , 2f , player.transform.rotation);
        Activate(target , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        EnemyData enemyData = enemy.GetComponent<EnemyData>();
        PlayerData playerData = player.GetComponent<PlayerData>();

        float DGM = (25f / 100f) * (float)playerData.currentATK;
        Debug.Log((int)DGM);

        enemyData.isOnFrost = true;
        enemyData.FrostEffect.SetActive(true);
        enemyData.Frost(1 , (int)DGM , true);
        playerData.playerMoveState = PlayerMoveState.Stand;
    }
}