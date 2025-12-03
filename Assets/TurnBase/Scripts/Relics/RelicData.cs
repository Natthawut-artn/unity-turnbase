using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RelicType
{
    normal,
    percent
}

[CreateAssetMenu(fileName = "Relic", menuName = "RelicData")]

public class RelicData : ScriptableObject
{
    public string RelicName;
    public Sprite RelicSprite;
    public RelicType relicType;
    public int HP;
    public int ATK;
    public int Defense;
    public int CritRate;
    public int CritDGM;

    public void InstallRelic(PlayerData player)
    {
        switch(this.relicType)
        {
            case RelicType.normal:
                player.currentHP += HP;
                player.currentATK += ATK;
                player.currentDefense += Defense;
                player.currentCritRate += CritRate;
                player.currentCritDGM += CritDGM;
            break;
            case RelicType.percent:
                int hp = (int) HP / 10 * player.currentHP;
                int atk = (int) ATK / 10 * player.currentATK;
                int def = (int) Defense / 10 * player.currentDefense;
                int critrate = (int) CritRate / 10 * player.currentCritRate;
                int critdgm = (int) CritDGM / 10 * player.currentCritDGM;
                player.currentHP += hp;
                player.currentATK += atk;
                player.currentDefense += def;
                player.currentCritRate += critrate;
                player.currentCritDGM += critdgm;
            break;
        }
        
    }

    public void RemoveRelic(PlayerData player)
    {
        switch(this.relicType)
        {
            case RelicType.normal:
                player.currentHP -= HP;
                player.currentATK -= ATK;
                player.currentDefense -= Defense;
                player.currentCritRate -= CritRate;
                player.currentCritDGM -= CritDGM;
            break;
            case RelicType.percent:
                int hp = (int) HP / 10 * player.currentHP;
                int atk = (int) ATK / 10 * player.currentATK;
                int def = (int) Defense / 10 * player.currentDefense;
                int critrate = (int) CritRate / 10 * player.currentCritRate;
                int critdgm = (int) CritDGM / 10 * player.currentCritDGM;
                player.currentHP -= hp;
                player.currentATK -= atk;
                player.currentDefense -= def;
                player.currentCritRate -= critrate;
                player.currentCritDGM -= critdgm;
            break;
        }

        if (player.currentHP < 1) player.currentHP = 1;
        if (player.currentATK < 1) player.currentATK = 1;
        if (player.currentDefense < 1) player.currentDefense = 1;
        if (player.currentCritRate < 1) player.currentCritRate = 1;
        if (player.currentCritDGM < 1) player.currentCritDGM = 1;
    }

    public string GetInfo()
    {
        string text = "";
        text += $"{RelicName} \n";
        if(HP > 0){ text += "Hp : " + HP + "\n"; }
        if(ATK > 0){ text += "ATK : " + ATK + "\n"; }
        if(Defense > 0){ text += "Defense : " + Defense + "\n"; }
        if(CritRate > 0){ text += "Crit Rate : " + CritRate + "\n"; }
        if (CritDGM > 0) { text += "Crit DGM : " + CritDGM + "\n"; }
        return text;
    }
}
