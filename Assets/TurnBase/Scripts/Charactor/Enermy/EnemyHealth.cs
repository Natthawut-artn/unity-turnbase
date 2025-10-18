using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float EnemyMaxhealth = 100;
    public float EnemyCurrenthealth;

    public EnemyAI enemyAI;

    void Start()
    {
        EnemyCurrenthealth = EnemyMaxhealth;
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float amount)
    {
        EnemyCurrenthealth -= amount;
        Debug.Log($"{name} took {amount} damage, current health: {EnemyCurrenthealth}");
        if (EnemyCurrenthealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} died.");
        // Disable enemy AI
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }
        // Optionally, play death animation or effects here
        // Destroy the enemy game object after a short delay to allow for death effects
        Destroy(gameObject, 1f);
    }
}
