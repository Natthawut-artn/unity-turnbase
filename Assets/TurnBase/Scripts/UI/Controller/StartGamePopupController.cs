public static class StartGamePopupController
{
    public static void Show()
    {
        StartGamePopup.Data data = new StartGamePopup.Data(() =>
        {
            AppStateManager.ChangeAppState(AppState.State);
            GameManager.Instance.StartGame();
            Hide();
        });
        ServiceLocator.Get<StartGamePopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<StartGamePopup>().Hide();
    }
}