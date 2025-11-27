
using Unity.VisualScripting;

public enum AppState
{
    Open,
    Hub,
    State
}

public static class AppStateManager
{
    public static AppState appState = AppState.Open;
    public static Theme theme = Theme.Scifi;
    public static void ChangeAppState(AppState newAppState)
    {
        appState = newAppState;
    }
}