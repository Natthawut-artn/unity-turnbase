using UnityEngine;

public static class HPBarHUDController
{
    static PlayerData playerData;
    static float currentHP;
    public static void Show()
    {
        GameObject Player = GameManager.Instance.Player;
        playerData = Player.GetComponent<PlayerData>();

        currentHP = (float)playerData.currentHP / (float)playerData.HP;

        HPBarHUD.Data data = new HPBarHUD.Data(currentHP);

        ServiceLocator.Get<HPBarHUD>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<HPBarHUD>().Hide();
    }

    public static void UpProgression()
    {
        currentHP = (float)playerData.currentHP / (float)playerData.HP;
        ServiceLocator.Get<HPBarHUD>().UpProgression(currentHP);
    }
}