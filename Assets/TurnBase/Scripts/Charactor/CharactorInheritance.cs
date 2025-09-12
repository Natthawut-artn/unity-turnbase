using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroClass
{
    Striker,
    Protecter,
    Support

}

public class CharactorInheritance : MonoBehaviour
{
    public HeroClass heroclass;
    public int HP;
    public int ATK;
    public int MagicATK;
    public int Defense;
    public int CritRate;
    public int CritDGM;
    public int Speed;

    public void TakeDamage(int Damage)
    {
        HP -= Damage;
        if (HP <= 0) { HP = 0; Debug.Log("Player Died"); }
        else { Debug.Log($"Player HP : {HP}"); }
    }

    public void Healing(int Heal)
    {
        HP += Heal;
        Debug.Log($"Player get Heal : {Heal}, HP {HP}");
    }

    public void ReceiveCC(float Time)
    {
        
    }
}
