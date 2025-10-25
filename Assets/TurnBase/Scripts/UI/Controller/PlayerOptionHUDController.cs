using UnityEngine;

public static class PlayerOptionHUDController
{
    public static void Show()
    {
        GameObject Player = MainCamera.Instance.Player.gameObject;
        PlayerAttack pa = Player.GetComponent<PlayerAttack>();
        SkillBook skillBook = Player.GetComponent<SkillBook>();

        Skill skill = skillBook.skillsSet[0];

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
        () =>{
            skillBook.skillsSet[0].UseSkill(Player);
        } , skill);

        ServiceLocator.Get<PlayerOptionHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayerOptionHUD>().Hide();
    }
}