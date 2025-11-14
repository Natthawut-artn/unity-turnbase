using System.Collections.Generic;
using UnityEngine;

public static class PlayerOptionHUDController
{
    public static void Show()
    {
        GameObject Player = MainCamera.Instance.Player.gameObject;
        PlayerAttack pa = Player.GetComponent<PlayerAttack>();
        SkillBook skillBook = Player.GetComponent<SkillBook>();

        List<Skill> skillset = skillBook.skillsSet;

        PlayerOptionHUD.Data data = new PlayerOptionHUD.Data(() =>
        {
            Player.GetComponent<PlayerController>().SkipTurn();
        },
        () =>
        {
            if (pa != null)
            {
                pa.ToggleTargetingMode();
            }
            else
            {
                Debug.LogError("PlayerAttack component not found on player!");
            }
        },
        () =>
        {
            skillBook.skillsSet[0].UseSkill(Player);
            ServiceLocator.Get<SkillPointsHUD>().Update();
        },
        () =>
        {
            skillBook.skillsSet[1].UseSkill(Player);
            ServiceLocator.Get<SkillPointsHUD>().Update();
        },
        () =>
        {
            skillBook.skillsSet[2].UseSkill(Player);
            ServiceLocator.Get<SkillPointsHUD>().Update();
        },
        skillset);

        ServiceLocator.Get<PlayerOptionHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayerOptionHUD>().Hide();
    }
}