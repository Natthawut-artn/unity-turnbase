using UnityEngine;

public enum PlayerMoveState
{
    Stand,
    Walk,
    Die
}

public class PlayerData : CharactorInheritance
{
    public Vector2 GridPosition;
    public float moveSpeed = 5f; 
    public PlayerMoveState playerMoveState = PlayerMoveState.Stand;
}
