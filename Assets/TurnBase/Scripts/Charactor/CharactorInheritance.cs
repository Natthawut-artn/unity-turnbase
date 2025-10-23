using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroClass
{
    Striker,
    Protecter,
    Support,
    Enemy,
    Boss
}

public class CharactorInheritance : MonoBehaviour
{
    // default
    public HeroClass heroclass;
    public int HP;
    public int ATK;
    public int Defense;
    public int CritRate;
    public int CritDGM;
    public int SkillPoint;

    // curruent
    public int currentHP;
    public int currentATK;
    public int currentDefense;
    public int currentCritRate;
    public int currentCritDGM;
    public int currentSkillPoint;

    // Bypass Defence
    public int BypassDef_Venom = 0;
    public int BypassDef_BloodFlowing = 0;
    public int BypassDef_Frost = 0;

    // Venom status (debuff)
    public bool isOnVenom = false;
    public int VenomTurnAmount = 0;
    public int VenomDGMPerTurn = 0;

    // Blood Flowing (debuff)
    public bool isOnBloodFlowing = false;

    // Frost (debuff)
    public bool isOnFrost = false;
    public int FrostTurnAmount = 0;
    public int FrostDGM = 0;

    // normal atk buff
    public bool ishavebuff = false;
    public int BuffAmountTarget = 0;

    // Buff
    public int Shield = 0;
    public int BuffShieldAmountTurn = 0;

    public bool ActivateInThisTurn = false;

    void Awake()
    {
        currentHP = HP;
        currentATK = ATK;
        currentDefense = Defense;
        currentCritRate = CritRate;
        currentCritDGM = CritDGM;
        currentSkillPoint = SkillPoint;
    }

    public void TakeDamage(int Damage)
    {
        if (Shield > 0)
        {
            if (Damage <= Shield)
            {
                Shield -= Damage;
            }
            else if (Damage > Shield)
            {
                Damage -= Shield;
                currentHP -= Damage;
                if (currentHP <= 0)
                {
                    currentHP = 0;
                    Die();
                }
            }
        }
        else
        {
            currentHP -= Damage;
            if (currentHP <= 0)
            {
                currentHP = 0;
                Die();
            }
        }
        
    }

    public void Healing(int Heal)
    {
        currentHP += Heal;
        if (currentHP >= HP)
        {
            currentHP = HP;
        }
        Debug.Log($"Player get Heal : {Heal}, HP {currentHP}");
    }

    public void Venom(int Damage = 0, int TurnAmount = 0)
    {
        if (Damage == 0 && TurnAmount == 0)
        {
            Damage = VenomDGMPerTurn;
            TurnAmount = VenomTurnAmount;
        }
        else
        {
            VenomDGMPerTurn = Damage;
            VenomTurnAmount = TurnAmount;
        }

        if (isOnVenom == false) { return; }
        TakeDamage(Damage);
        VenomTurnAmount -= 1;
        Debug.Log($"Venom activate DGM : {Damage} TurnAmount : {VenomTurnAmount}");
        if (VenomTurnAmount <= 0) { isOnVenom = false; VenomDGMPerTurn = 0; VenomTurnAmount = 0; }
    }

    public void BloodFlowing(int Damage)
    {
        TakeDamage(Damage);
        Debug.Log($"Blood Flowing activate DGM : {Damage}");
    }

    public void Frost(int TurnAmount = 0 , int Damage = 0 , bool isThisTurn = false)
    {
        if (Damage == 0 && TurnAmount == 0)
        {
            Damage = FrostDGM;
            TurnAmount = FrostTurnAmount;
        }
        else
        {
            FrostDGM = Damage;
            FrostTurnAmount = TurnAmount;
        }

        if (isThisTurn) return;
        
        TakeDamage(Damage);
        FrostTurnAmount -= 1;
        Debug.Log($"Frost activate DGM : {Damage} TurnAmount : {FrostTurnAmount}");
        if (FrostTurnAmount <= 0) { isOnFrost = false; FrostDGM = 0; FrostTurnAmount = 0; }
    }

    public void ResetBuffForTurn()
    {
        if (ishavebuff && BuffAmountTarget > 0 && !ActivateInThisTurn)
        {
            BuffAmountTarget--;
            Debug.Log($"{BuffAmountTarget} targer more left");
        }
        else if (ishavebuff && BuffAmountTarget > 0 && ActivateInThisTurn)
        {
            Debug.Log($"{BuffAmountTarget} targer more left");
            ActivateInThisTurn = false;
        }
        else if (BuffAmountTarget <= 0)
        {
            ishavebuff = false;
        }

        if (BuffShieldAmountTurn > 0)
        {
            BuffShieldAmountTurn--;
            Debug.Log($"Shield {BuffShieldAmountTurn} turn more left");
        }

        if (BuffShieldAmountTurn <= 0)
        {
            Shield = 0;
        }
    }

    public void Die()
    {
        string side;
        if (heroclass == HeroClass.Enemy)
        {
            side = "Enemy";
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            enemyAI.enabled = false;
            GameManager.Instance.Enemys_alive.Remove(enemyAI.gameObject);
            Destroy(gameObject, 1f);
        }
        else
        {
            side = "Player";
            StartCoroutine(Wait());
        }
            Debug.Log($"{side} died.");
        // Add death logic here (e.g., respawn, game over screen)
    }

    IEnumerator Wait()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.enabled = false;
        PlayerData playerData = GetComponent<PlayerData>();
        playerData.playerMoveState = PlayerMoveState.Die;
        yield return new WaitForSeconds(0.75f);
        if (GameManager.Instance.Players_alive.Count <= 0)
        {
            SettingPopupController.Show();
        }
    }
}
