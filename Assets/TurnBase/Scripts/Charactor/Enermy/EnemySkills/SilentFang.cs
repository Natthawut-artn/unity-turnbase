using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilentFang : MonoBehaviour
{
    private EnemyData enemyData;

    [Header("Skill Parameters")]
    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 4;

    // REMOVED: damagePercentage is no longer needed

    // Radius must cover 1 tile in all directions (3x3 grid total). 
    private const float AOE_RADIUS = 2.8f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
    }

    /// <summary>
    /// Checks if at least one player is within the enemy's attack range.
    /// </summary>
    public bool IsPlayerInAttackRange()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Executes the full skill: checks cost, deducts from global pool, and applies Blood Flowing status.
    /// </summary>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("SilentFang failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Check and Deduct from Global Pool ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            yield break; // Failed to spend points (pool is empty)
        }

        // --- 2. Find and Apply Status ---
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);
        int playersHit = 0;

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    // CRITICAL: Apply Blood Flowing status (Setup for synergy)
                    targetPlayer.isOnBloodFlowing = true;

                    Debug.Log($"{gameObject.name} hit {hitCollider.name} with Silent Fang (Blood Flowing applied).");
                    playersHit++;
                }
            }
        }

        // --- 3. Wait for Animation/Effect Duration ---
        yield return new WaitForSeconds(1.5f);

        // --- 4. Final Check ---
        if (playersHit == 0)
        {
            // If we spent the points but hit nothing, we refund the cost.
            EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
            Debug.Log("Silent Fang failed to hit any player. Cost refunded to Global SP Pool.");
        }
    }
}
