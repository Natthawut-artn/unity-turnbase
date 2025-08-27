using UnityEngine;

public enum EnemyMoveState
{
    Stand,
    Walk
}

public static class EnemyData
{
    public static Vector2 GridPosition;
    public static float moveSpeed = 5f;
    public static EnemyMoveState playerMoveState = EnemyMoveState.Stand;
}

