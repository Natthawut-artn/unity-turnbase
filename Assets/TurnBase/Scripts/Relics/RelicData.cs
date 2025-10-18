using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "RelicData")]

public class RelicData : ScriptableObject
{
    public string RelicName;
    public Sprite RelicSprite;
    public int HP;
    public int ATK;
    public int MagicATK;
    public int Defense;
    public int CritRate;
    public int CritDGM;

    public void InstallRelic(PlayerData player)
    {
        player.currentHP += HP;
        player.currentATK += ATK;
        player.currentDefense += Defense;
        player.currentCritRate += CritRate;
        player.currentCritDGM += CritDGM;
    }

    public void RemoveRelic(PlayerData player)
    {
        player.currentHP -= HP;
        player.currentATK -= ATK;
        player.currentDefense -= Defense;
        player.currentCritRate -= CritRate;
        player.currentCritDGM -= CritDGM;

        if (player.currentHP < 0) player.currentHP = 0;
        if (player.currentATK < 0) player.currentATK = 0;
        if (player.currentDefense < 0) player.currentDefense = 0;
        if (player.currentCritRate < 0) player.currentCritRate = 0;
        if (player.currentCritDGM < 0) player.currentCritDGM = 0;
    }
}
