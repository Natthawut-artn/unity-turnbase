using UnityEngine;
using System.Threading.Tasks;

public class Shunpo : Skill
{
    public Shunpo()
    {
        SkillName = "Shunpo";
        SkillSprite_Path = "Graphic2D/Skill/Shunpo";
        Description = "";
        UseSkillPoints = 2;
    }

    public override void UseSkill(GameObject player)
    {
        if (SkillPointsManager.currentSkillPoint < UseSkillPoints) { Debug.Log("not enough skill point"); return; }
        SkillPointsManager.UseSkillPoint(UseSkillPoints);
        isActivate = true;
    }

    public override async void Activate(GameObject enemy, GameObject player)
    {
        if (!isActivate) return;
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerData playerData = player.GetComponent<PlayerData>();
        if (controller == null) return;

        Vector3 targetPos = new Vector3(controller.markertarget.transform.position.x , 
        controller.transform.position.y , 
        controller.markertarget.transform.position.z);
        Vector3 dashDir = (targetPos - player.transform.position).normalized;
        Quaternion dashRotation = Quaternion.LookRotation(dashDir);
        controller.SetAnimationState(PlayerMoveState.UseSkill2);
        player.transform.LookAt(dashDir);
        await Task.Delay(1750);
        controller.DashTo(targetPos);
        VFXManager.Instance.AddVFXOnWorld(player, VFX.Striker_Skill2, 2f, dashRotation);
        SFXManager.Instance.PlayOneShot(SFX.Dash);
        isActivate = false;
        playerData.playerMoveState = PlayerMoveState.Stand;
    }
}