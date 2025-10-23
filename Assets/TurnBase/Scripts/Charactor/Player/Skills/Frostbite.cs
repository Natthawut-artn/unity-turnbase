using UnityEngine;

public class Frostbite : Skill
{
    private Quaternion fixedRotation = Quaternion.identity;

    public Frostbite()
    {
        SkillName = "Frostbite";
        SkillSprite_Path = "";
        Description = "";
        UseSkillPoints = 1;
    }
    
    public override void UseSkill(GameObject player = null)
    {
        PlayerData playerData = player.GetComponent<PlayerData>();

        if (playerData.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        playerData.currentSkillPoint -= UseSkillPoints;
        Debug.Log("Use Frostbite");

        PlayerController controller = player.GetComponent<PlayerController>();
        Collider[] hits = Physics.OverlapBox(player.transform.position, new Vector3(10f,2f,10f) , fixedRotation, controller.Enemy);
        GameObject target = hits[0].gameObject;
        Activate(target , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        EnemyData enemyData = enemy.GetComponent<EnemyData>();
        PlayerData playerData = player.GetComponent<PlayerData>();

        float DGM = (25f / 100f) * (float)playerData.currentATK;
        Debug.Log((int)DGM);

        enemyData.isOnFrost = true;
        enemyData.Frost(1 , (int)DGM , true);
    }
}