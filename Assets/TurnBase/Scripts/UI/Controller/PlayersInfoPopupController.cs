using UnityEngine;
using DelegateManager;
using System.Collections.Generic;

public static class PlayersInfoPopupController
{
    public static void Show()
    {
        PlayersInfoPopup.Data data = new PlayersInfoPopup.Data(() =>
        {
            PlayersInfoHUDController.Show();
            PocketHUDController.Show();
            Hide();
        }, (PlayerIndex, RelicIndex) =>
        {
            FindRelic(PlayerIndex, RelicIndex);
        });

        ServiceLocator.Get<PlayersInfoPopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<PlayersInfoPopup>().Hide();
    }
    
    public static void FindRelic(int PlayerIndex, int RelicIndex)
    {
        GameObject player = GameManager.Instance.Players_alive[PlayerIndex];
        RelicsStoage relicsStoage = player.GetComponent<RelicsStoage>();
        if (RelicIndex >= relicsStoage.RelicsInPocket.Count) return;
        RelicData relicData = relicsStoage.RelicsInPocket[RelicIndex];
        RelicUnInstPopup.Data data = new RelicUnInstPopup.Data(relicData, relicsStoage);
        ServiceLocator.Get<RelicUnInstPopup>().Show(data);
    }
}