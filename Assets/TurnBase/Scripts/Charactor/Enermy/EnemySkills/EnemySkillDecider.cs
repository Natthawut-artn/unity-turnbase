using System.Collections;
using UnityEngine;

/// <summary>
/// Handles decision logic for unique skills based on the enemy type set by the spawner.
/// </summary>
public class EnemySkillDecider : MonoBehaviour
{
    private EnemyData enemyData;
    private ToxicFragment toxicFragment;

    [HideInInspector] public string EnemyType;

    // NEW FLAG: Signals to the EnemyAI that a skill was successfully executed and the turn should end.
    [HideInInspector] public bool SkillExecutedThisTurn = false;

    void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        toxicFragment = GetComponent<ToxicFragment>();
    }

    /// <summary>
    /// Checks the enemy's type and attempts to use the appropriate skill if conditions are met.
    /// This method is now a COROUTINE to allow it to wait for the skill execution time.
    /// </summary>
    /// <returns>IEnumerator to execute the skill and wait for it to finish.</returns>
    public IEnumerator DecideAndExecuteSkill()
    {
        // Reset the flag before running any logic
        SkillExecutedThisTurn = false;

        if (enemyData == null || EnemySkillPoint.Instance == null)
        {
            yield break;
        }

        // 1. Slime Check: Only execute if the enemy is a "Slime"
        if (EnemyType == "Slime" && toxicFragment != null)
        {
            // Check global cost before attempting skill
            int skillCost = toxicFragment.skillCost;
            if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
            {
                // Check if the skill can hit a target
                if (toxicFragment.IsPlayerInAttackRange())
                {
                    // CRITICAL FIX: Use yield return StartCoroutine to execute the skill and WAIT for it to finish.
                    yield return StartCoroutine(toxicFragment.ExecuteSkill());

                    // Mark as successful after the skill coroutine is complete
                    SkillExecutedThisTurn = true;
                    yield break;
                }
            }
        }

        // If no skill was used, the coroutine ends immediately.
    }
}