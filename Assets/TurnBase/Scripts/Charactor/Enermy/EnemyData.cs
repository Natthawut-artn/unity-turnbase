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
}

