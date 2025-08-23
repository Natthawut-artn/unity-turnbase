using UnityEngine;

public enum PlayerMoveState
{
    Stand,
    Walk
}

public static class PlayerData
{
    public static Vector2 GridPosition;
    public static float moveSpeed = 5f; 
    public static PlayerMoveState playerMoveState = PlayerMoveState.Stand;
}
