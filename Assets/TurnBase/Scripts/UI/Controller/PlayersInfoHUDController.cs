using UnityEngine;

public static class PlayersInfoHUDController
{
    public static void Show()
    {
        PlayersInfoHUD.Data data = new PlayersInfoHUD.Data(() =>
        {
            PlayersInfoPopupController.Show();
            PlayerOptionHUDController.Hide();
            ServiceLocator.Get<SkillPointsHUD>().Hide();
            PocketHUDController.Hide();
            HPBarsHUDController.Hide();
            Hide();
        });

        ServiceLocator.Get<PlayersInfoHUD>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayersInfoHUD>().Hide();
    }
}