public static class StartGamePopupController
{
    public static void Show(Theme theme)
    {
        StartGamePopup.Data data = new StartGamePopup.Data(() =>
        {
            AppStateManager.ChangeAppState(AppState.State);
            GameManager.Instance.StartGame(theme);
            Hide();
        });
        ServiceLocator.Get<StartGamePopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<StartGamePopup>().Hide();
    }
}