using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DivineRevival : Skill
{
    public DivineRevival()
    {
        SkillName = "Divine Revival";
        SkillSprite_Path = "Graphic2D/Skill/Divine Revival";
        Description = "";
        UseSkillPoints = 2;
    }

    public override async void UseSkill(GameObject player = null)
    {
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        PlayerController controller = player.GetComponent<PlayerController>();
        controller.SetAnimationState(PlayerMoveState.UseSkill2);
        SFXManager.Instance.PlayOneShot(SFX.Revive);
        await Task.Delay(1750);
        PlayerData data = player.GetComponent<PlayerData>();
        data.playerMoveState = PlayerMoveState.Stand;
        Activate(null , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        GameObject target = GameManager.Instance.lstPlayerDie;

        List<GameObject> markers = new List<GameObject>();

        Collider[] findmarkers = Physics.OverlapSphere(player.transform.position, 3.5f);
        foreach (Collider marker in findmarkers)
        {
            GameObject gameObject = marker.gameObject;
            if (gameObject.layer == LayerMask.NameToLayer("Marker"))
            {
                markers.Add(marker.gameObject);
            }
        }

        foreach (GameObject marker in markers)
        {
            Collider[] enemiesOnTile = Physics.OverlapSphere(marker.transform.position, 1f, GameManager.Instance.EnemyLayer);
            if (enemiesOnTile.Length <= 0)
            {
                target.SetActive(true);
                Vector3 newPos = new Vector3(marker.transform.position.x , target.transform.position.y - 0.3f, marker.transform.position.z);
                target.transform.position = newPos;
                VFXManager.Instance.AddVFXOnWorld(target , VFX.Protector_Skill2 , 2f);

                PlayerData playerData = target.GetComponent<PlayerData>();
                float Hp = 0.5f * (float)playerData.HP;
                playerData.currentHP = (int)Hp;

                if (target.name.Contains("Striker"))
                {
                    GameManager.Instance.Players_alive.Insert(0, target);
                }
                else if (target.name.Contains("Support"))
                {
                    GameManager.Instance.Players_alive.Insert(1, target);
                }
                else if (target.name.Contains("Protectotr"))
                {
                    GameManager.Instance.Players_alive.Insert(2, target);
                }
                GameManager.Instance.lstPlayerDie = null;
                HPBarsHUDController.Update();
                return;
            }
        }
    }
}
