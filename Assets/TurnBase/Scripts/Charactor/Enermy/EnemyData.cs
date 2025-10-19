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

    public virtual void EnemyTakeDamage(int Damage)
    {
        currentHP -= Damage;
      
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }
}

