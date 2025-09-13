using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroClass
{
    Striker,
    Protecter,
    Support,
    Enemy
}

public class CharactorInheritance : MonoBehaviour
{
    // default
    public HeroClass heroclass;
    public int HP;
    public int ATK;
    public int MagicATK;
    public int Defense;
    public int CritRate;
    public int CritDGM;
    public int Speed;

    // curruent
    public int currentHP;

    public void TakeDamage(int Damage)
    {
        currentHP -= Damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            Debug.Log($"Player HP : {currentHP}");
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

    public void ReceiveCC(float Time)
    {

    }

    void Die()
    {
        string side;
        if (heroclass == HeroClass.Enemy)
        {
            side = "Enemy";
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            enemyAI.enabled = false;
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
        SettingPopupController.Show();
    }
}
