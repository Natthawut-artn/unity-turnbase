using UnityEngine;

public static class PlayerOptionHUDController
{
    public static void Show()
    {
        GameObject Player = MainCamera.Instance.Player.gameObject;
        PlayerAttack pa = Player.GetComponent<PlayerAttack>();
        PlayerOptionHUD.Data data = new PlayerOptionHUD.Data(() =>
        {
            Player.GetComponent<PlayerController>().SkipTurn();
        },
        () =>
        {
            // --- MODIFIED: Switch from Attack() to ToggleTargetingMode() ---
            if (pa != null)
            {
                pa.ToggleTargetingMode();
            }
            else
            {
                Debug.LogError("PlayerAttack component not found on player!");
            }
        });

        ServiceLocator.Get<PlayerOptionHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayerOptionHUD>().Hide();
    }
}