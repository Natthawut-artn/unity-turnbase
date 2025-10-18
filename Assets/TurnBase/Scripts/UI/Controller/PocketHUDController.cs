using UnityEngine;

public static class PocketHUDController
{
    public static void Show()
    {
        PocketHUD.Data data = new PocketHUD.Data(() =>
        {
            RelicPocketPopupController.Show();
            PlayersInfoHUDController.Hide();
            Hide();
        });

        ServiceLocator.Get<PocketHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PocketHUD>().Hide();
    }
}