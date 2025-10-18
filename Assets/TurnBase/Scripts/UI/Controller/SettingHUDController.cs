public static class SettingHUDController
{
    public static void Show()
    {
        SettingHUD.Data data = new SettingHUD.Data(() =>
        {
            SettingPopupController.Show();
        });
        ServiceLocator.Get<SettingHUD>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<SettingHUD>().Hide();
    }
}