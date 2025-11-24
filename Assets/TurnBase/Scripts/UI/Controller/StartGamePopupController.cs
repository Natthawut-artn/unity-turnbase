public static class StartGamePopupController
{
    private const string mother = "Graphic2D/Boss/Motherart";
    private const string eternal = "Graphic2D/Boss/EternalNight-ART";
    private const string zenos = "Graphic2D/Boss/Zenos";
    private const string CoverMother = "Graphic2D/WideTheMother";
    private const string CoverEternal = "Graphic2D/WideEternalNight";
    private const string CoverZenos = "Graphic2D/WideZenos";
    public static void Show(Theme theme)
    {
        string Boss = "";
        string Cover = "";
        if (theme == Theme.Scifi)
        {
            Boss = mother;
            Cover = CoverMother;
        }
        else if (theme == Theme.Medieval)
        {
            Boss = eternal;
            Cover = CoverEternal;
        }
        else
        {
            Boss = zenos;
            Cover = CoverZenos;
        }
        StartGamePopup.Data data = new StartGamePopup.Data(() =>
        {
            AppStateManager.ChangeAppState(AppState.State);
            GameManager.Instance.StartGame(theme);
            Hide();
        }, Cover , Boss);
        ServiceLocator.Get<StartGamePopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<StartGamePopup>().Hide();
    }
}