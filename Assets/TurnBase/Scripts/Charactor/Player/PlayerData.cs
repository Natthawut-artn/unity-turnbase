using UnityEngine;

public enum PlayerMoveState
{
    Stand,
    Walk
}

public class PlayerData : CharactorInheritance
{
    public static PlayerData Instance;
    void Awake()
    {
        Instance = this;
    }

    public Vector2 GridPosition;
    public float moveSpeed = 5f; 
    public PlayerMoveState playerMoveState = PlayerMoveState.Stand;
}
