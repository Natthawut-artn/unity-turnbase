using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
   public static PlayerHealth Instance;
    public int maxHealth = 100;
   public int currentHealth;

    void Awake()
    {
        Instance = this;
    }   
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage, current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Player healed {amount}, current health: {currentHealth}");
    }

    void Die()
    {
        Debug.Log("Player died.");
        // Add death logic here (e.g., respawn, game over screen)
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(20);
            StartCoroutine(SwitchTurn());
        }
    }

    IEnumerator SwitchTurn()
    {
        yield return new WaitForSeconds(1f);
        TurnManager.Instance.EndTurn();
    }
}

