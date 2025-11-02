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
    public string EnemyType;
    public EnemyHealthBar myHealthBar;
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
    // Add this method to your BossData.cs or EnemyData.cs script
    // This method MUST be in your Phase 2 Boss's data script (BossData.cs or EnemyData.cs)
    public override void Die()
    {
        // First, check if this is a Player
        if (heroclass != HeroClass.Enemy && heroclass != HeroClass.Boss)
        {
            base.Die(); // Call base player death logic
            return;
        }

        // --- It's an Enemy or Boss ---
        Debug.Log($"<color=red>Die() override triggered for {gameObject.name}</color>");

        // 1. Check for Phase Swap
        SwapForm swapSkill = GetComponent<SwapForm>();

        if (swapSkill != null)
        {
            // --- THIS IS A PHASE SWAP (e.g., P1 Boss) ---
            Debug.Log("SwapForm component found, executing phase change...");
            swapSkill.ExecutePhaseChange(); // Spawns P2
        }
        else if (heroclass == HeroClass.Boss)
        {
            // --- THIS IS A FINAL BOSS DEATH (Phase 2 Boss) ---
            // 'swapSkill' is null, so we run this block.
            Debug.Log($"<color=red>FINAL BOSS DEATH: {gameObject.name}</color>. Stopping turns, giving relic.");
            TurnManager.Instance?.StopTurnBaseSystem();
            GameManager.Instance.GetBrokenRelic(); // <-- GIVES THE RELIC
        }
        // else: This is a regular ENEMY death. Do nothing (no relic).


        // --- 3. STANDARD CLEANUP (for all enemies/bosses that die) ---

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Enemys_alive.Remove(gameObject);
        }

        // Clean up the grid marker and destroy the object
        BossController controller = GetComponent<BossController>();
        if (controller != null)
        {
            controller.DestroySelf();
        }
        else
        {
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.DestroySelf();
            }
            else
            {
                Destroy(gameObject); // Fallback
            }
        }
    }
}
