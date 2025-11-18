using UnityEngine;
using System.Threading.Tasks;

public class WardoftheHoly : Skill
{
    private int BlockAttackAmount = 3;
    public WardoftheHoly()
    {
        SkillName = "Ward of the Holy";
        SkillSprite_Path = "Graphic2D/Skill/Paladin14";
        Description = "";
        UseSkillPoints = 3;
    }

    public override async void UseSkill(GameObject player = null)
    {
        if(SkillPointsManager.currentSkillPoint < UseSkillPoints){ Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
        PlayerController controller = player.GetComponent<PlayerController>();
        controller.SetAnimationState(PlayerMoveState.UseSkill3);
        await Task.Delay(2150);
        VFXManager.Instance.AddVFXOnGameObject(player , VFX.Protector_Skill3);
        Activate(null , player);
    }

    public override void Activate(GameObject enemy = null, GameObject player = null)
    {
        Collider[] hits = Physics.OverlapBox(player.transform.position, new Vector3(1.5f, 1.5f, 1.5f), Quaternion.identity, GameManager.Instance.PlayerLayer);
        foreach (Collider hit in hits)
        {
            PlayerData playerData = hit.gameObject.GetComponent<PlayerData>();
            playerData.RemoveDebuff();
            playerData.BlockAttackAmount = BlockAttackAmount;
            Debug.Log($"{hit.name} get protection for Zonya");
        }
        PlayerData data = player.GetComponent<PlayerData>();
        data.playerMoveState = PlayerMoveState.Stand;
    }

}
