using UnityEngine;

public static class PlayerOptionHUDController
{
    public static void Show()
    {
        GameObject Player = MainCamera.Instance.Player.gameObject;
        PlayerOptionHUD.Data data = new PlayerOptionHUD.Data(() =>
        {
            Player.GetComponent<PlayerController>().SkipTurn();
        },
        () =>
        {
            Player.GetComponent<PlayerAttack>().Attack();
        });

        ServiceLocator.Get<PlayerOptionHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayerOptionHUD>().Hide();
    }
}