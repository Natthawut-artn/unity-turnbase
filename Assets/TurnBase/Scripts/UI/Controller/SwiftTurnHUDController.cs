public static class SwiftTurnHUDController
{
    public static void Show()
    {
        ServiceLocator.Get<SwiftTurnHUD>().Show(new SwiftTurnHUD.Data());
    }

    public static void Hide()
    {
        ServiceLocator.Get<SwiftTurnHUD>().Hide();
    }
}