using UnityEngine;

public enum BossMoveState
{
    Stand,
    Walk
}

public class BossData : CharactorInheritance
{
    public Vector2 GridPosition;
    public float moveSpeed = 5f;
    public BossMoveState playerMoveState = BossMoveState.Stand;
}
