public static class SettingHUDController
{
    public static void Show()
    {
        SettingHUD.Data data = new SettingHUD.Data(() =>
        {
            if (ServiceLocator.Get<StartGamePopup>().SelfGameObject != null)
            {
                return;    
            }
            SettingPopupController.Show();
            HPBarsHUDController.Hide();
            PlayerOptionHUDController.Hide();
            PocketHUDController.Hide();
            PlayersInfoHUDController.Hide();
            ServiceLocator.Get<SkillPointsHUD>().Hide();
        });
        ServiceLocator.Get<SettingHUD>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<SettingHUD>().Hide();
    }
}