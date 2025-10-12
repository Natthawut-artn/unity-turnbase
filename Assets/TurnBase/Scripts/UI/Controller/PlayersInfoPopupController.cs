using UnityEngine;

public static class PlayersInfoPopupController
{
    public static void Show()
    {
        PlayersInfoPopup.Data data = new PlayersInfoPopup.Data(() =>
        {
            PlayersInfoHUDController.Show();
            Hide();
        });

        ServiceLocator.Get<PlayersInfoPopup>().Show(data);
    }
    
    public static void Hide()
    {
        ServiceLocator.Get<PlayersInfoPopup>().Hide();
    }
}