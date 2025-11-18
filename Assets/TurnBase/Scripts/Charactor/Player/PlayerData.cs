using UnityEngine;

public enum PlayerMoveState
{
    Stand,
    Walk,
    Die,
    UseSkill1,
    UseSkill2,
    UseSkill3,
}

public class PlayerData : CharactorInheritance
{
    public Vector2 GridPosition;
    public float moveSpeed = 5f; 
    public PlayerMoveState playerMoveState = PlayerMoveState.Stand;
}
