using System.Collections.Generic;
using UnityEngine;
using DelegateManager;

public static class BrokenRelicPocketController
{
    public static void Show()
    {
        BrokenRelicPocket.Data data = new BrokenRelicPocket.Data(() =>
        {
            PocketHUDController.Show();
            Hide();
        },
        (index) =>
        {
            FindRelic(index);
        });
        ServiceLocator.Get<BrokenRelicPocket>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<BrokenRelicPocket>().Hide();
    }

    public static void FindRelic(int index)
    {
        if (index >= BrokenRelicManager.BrokenRelicPocket.Count) return;
        else
        {
            RelicData relicData = BrokenRelicManager.BrokenRelicPocket[index];
            List<Delegate.CallBack> callBacks = new List<Delegate.CallBack>()
            {
                () =>
                {
                    BrokenRelicManager.InstallInPlayer("Striker", relicData);
                },
                () =>
                {
                    BrokenRelicManager.InstallInPlayer("Support", relicData);
                },
                () =>
                {
                    BrokenRelicManager.InstallInPlayer("Protector", relicData);
                }
            };
            Delegate.CallBack destroy = new Delegate.CallBack(() =>
            {
                BrokenRelicManager.Destroy(index);
            });
            BrokenRelicInstPopup.Data data = new BrokenRelicInstPopup.Data(index,callBacks,destroy);
            ServiceLocator.Get<BrokenRelicInstPopup>().Show(data);
        }
    }
}