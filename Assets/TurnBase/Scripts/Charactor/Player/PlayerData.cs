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
    private SkillBook skillBook;

    void Start()
    {
        skillBook = GetComponent<SkillBook>();
    }

    public void Upgrade()
    {
        LV += 1;
        if(LV >= 9)
        {
            skillBook.skillsSet[2].Unlock = true;
        }
        else if (LV >= 6)
        {
            skillBook.skillsSet[1].Unlock = true;
        }
        else if (LV >= 3)
        {
            skillBook.skillsSet[0].Unlock = true;
        }
    }
}
