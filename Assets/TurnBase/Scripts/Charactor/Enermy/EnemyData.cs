using UnityEngine;

public enum EnemyMoveState
{
    Stand,
    Walk
}

public class EnemyData : CharactorInheritance
{
    public Vector2 GridPosition;
    public float moveSpeed = 5f;
    public EnemyMoveState playerMoveState = EnemyMoveState.Stand;

    public void EnemyTakeDamage(int Damage)
    {
        currentHP -= Damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die(); // Calls the Die() from CharactorInheritance
        }
    }

    // FIX: We must override the base Die() method to include the grid cleanup logic
    public override void Die()
    {
        // Check if this character is an enemy before performing enemy-specific cleanup
        if (heroclass == HeroClass.Enemy || heroclass == HeroClass.Boss)
        {
            // 1. Get the EnemyAI component
            EnemyAI enemyAI = GetComponent<EnemyAI>();

            // 2. IMPORTANT: Call DestroySelf() which contains the logic to release the marker
            if (enemyAI != null)
            {
                GameManager.Instance.Enemys_alive.Remove(enemyAI.gameObject);
                enemyAI.DestroySelf();
                return; // Stop here, as DestroySelf handles everything, including destroying the object.
            }
        }

        // If it's not an enemy, or if the enemyAI component is missing, call the base class logic
        // for players/other entities (which handles turning off controllers, etc.)
        base.Die();
    }
}
