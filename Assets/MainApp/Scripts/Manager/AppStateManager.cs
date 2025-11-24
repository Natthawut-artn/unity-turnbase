
using Unity.VisualScripting;

public enum AppState
{
    Hub,
    State
}

public static class AppStateManager
{
    public static AppState appState = AppState.Hub;
    public static Theme theme = Theme.Scifi;
    public static void ChangeAppState(AppState newAppState)
    {
        appState = newAppState;
    }
}