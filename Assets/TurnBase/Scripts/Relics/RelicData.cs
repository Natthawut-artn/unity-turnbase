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
        player.HP += HP;
        player.ATK += ATK;
        player.MagicATK += MagicATK;
        player.Defense += Defense;
        player.CritRate += CritRate;
        player.CritDGM += CritDGM;
    }

    public void RemoveRelic(PlayerData player)
    {
        player.HP -= HP;
        player.ATK -= ATK;
        player.MagicATK -= MagicATK;
        player.Defense -= Defense;
        player.CritRate -= CritRate;
        player.CritDGM -= CritDGM;

        if (player.HP < 0) player.HP = 0;
        if (player.ATK < 0) player.ATK = 0;
        if (player.MagicATK < 0) player.MagicATK = 0;
        if (player.Defense < 0) player.Defense = 0;
        if (player.CritDGM < 0) player.CritRate = 0;
        if (player.CritDGM < 0) player.CritDGM = 0;
    }
}
