public static class SettingPopupController
{
    public static void Show()
    {
        SettingPopup.Data data = new SettingPopup.Data(() =>
        {
            GameManager.Instance.BackToTheHub();
            Hide();
        });
        ServiceLocator.Get<SettingPopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<SettingPopup>().Hide();
    }
}