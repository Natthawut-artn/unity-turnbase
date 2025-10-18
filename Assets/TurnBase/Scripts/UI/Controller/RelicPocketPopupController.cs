using UnityEngine;

public static class RelicPocketPopupController
{
    public static void Show()
    {
        RelicPocketPopup.Data data = new RelicPocketPopup.Data(() =>
        {
            PocketHUDController.Show();
            PlayersInfoHUDController.Show();
            Hide();
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
}