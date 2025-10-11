using System.Collections.Generic;
using UnityEngine;

public static class HPBarsHUDController
{
    public static void Show()
    {
        List<float> currentHpPlayers = new List<float>();
        List<GameObject> Players = GameManager.Instance.Players_alive;

        foreach (GameObject Player in Players)
        {
            PlayerData playerData = Player.GetComponent<PlayerData>();
            float Hp = playerData.currentHP / playerData.HP;
            currentHpPlayers.Add(Hp);
        }

        HpBarsHUD.Data data = new HpBarsHUD.Data(currentHpPlayers);

        ServiceLocator.Get<HpBarsHUD>().Show(data);
    }

    public static void Hide()
    {
        ServiceLocator.Get<HpBarsHUD>().Hide();
    }
    
    public static void Update()
    {
        List<float> currentHpPlayers = new List<float>();
        List<GameObject> Players = GameManager.Instance.Players_alive;

        foreach (GameObject Player in Players)
        {
            PlayerData playerData = Player.GetComponent<PlayerData>();
            Debug.Log(playerData.currentHP);
            Debug.Log(playerData.HP);
            float Hp = (float)playerData.currentHP / (float)playerData.HP;
            currentHpPlayers.Add(Hp);
        }
        
        ServiceLocator.Get<HpBarsHUD>().Update(currentHpPlayers);
    }
} 