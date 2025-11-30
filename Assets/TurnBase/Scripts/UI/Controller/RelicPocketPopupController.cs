using UnityEngine;
using DelegateManager;
using System.Collections.Generic;

public static class RelicPocketPopupController
{
    public static void Show()
    {
        RelicPocketPopup.Data data = new RelicPocketPopup.Data(() =>
        {
            PocketHUDController.Show();
            PlayersInfoHUDController.Show();
            if (TurnManager.Instance.isInTurnSystem)
            {
                PlayerOptionHUDController.Show();
            }
            ServiceLocator.Get<SkillPointsHUD>().Show();
            Hide();
        } , (index) =>
        {
            FindRelic(index);
        });

        ServiceLocator.Get<RelicPocketPopup>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<RelicPocketPopup>().Hide();
    }

    public static void Update()
    {
        ServiceLocator.Get<RelicPocketPopup>().Update();
    }
    
    public static void FindRelic(int index)
    {
        if (index >= RelicStroageManager.RelicsInUserPocket.Count) return;
        else
        {
            RelicData relicData = RelicStroageManager.RelicsInUserPocket[index];
            List<Delegate.CallBack> callBacks = new List<Delegate.CallBack>()
            {
                () =>
                {
                    GameObject striker = GameManager.Instance.Players_alive[0];
                    RelicsStoage relicsPockt = striker.GetComponent<RelicsStoage>();
                    relicsPockt.InstallRelic(relicData);
                },
                () =>
                {
                    GameObject support = GameManager.Instance.Players_alive[1];
                    RelicsStoage relicsPockt = support.GetComponent<RelicsStoage>();
                    relicsPockt.InstallRelic(relicData);
                },
                () =>
                {
                    GameObject protector = GameManager.Instance.Players_alive[2];
                    RelicsStoage relicsPockt = protector.GetComponent<RelicsStoage>();
                    relicsPockt.InstallRelic(relicData);
                }
            };
            RelicInstPopup.Data data = new RelicInstPopup.Data(index,callBacks);
            ServiceLocator.Get<RelicInstPopup>().Show(data);
        }
    }
}