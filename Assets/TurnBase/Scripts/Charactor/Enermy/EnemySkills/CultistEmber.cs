using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistEmber : MonoBehaviour
{
    private EnemyData enemyData;

    [Header("Skill Parameters")]
    [Tooltip("The Skill Point cost deducted from the Global Pool to use this ability.")]
    public int skillCost = 5;

    [Tooltip("The number of turns the Burning status will last.")]
    public int burnTurns = 3;

    [Tooltip("Percentage of the enemy's ATK used as base damage per turn.")]
    public float damagePercentage = 0.25f; // 25% of ATK

    // AoE Detection
    private const float AOE_RADIUS = 2.8f;
    public LayerMask targetLayer;

    Animator animator;
    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        animator = GetComponentInChildren<Animator>();
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
                // Ensure the target is a Player, not another Enemy
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Executes the full skill: checks cost, deducts from global pool, and applies the Burning status.
    /// </summary>
    /// <returns>IEnumerator to handle the wait time after execution.</returns>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("CultistEmber failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Check and Deduct from Global Pool ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            yield break;
        }

        // --- 2. Calculate Damage Per Turn ---
        int burnDGMPerTurn = Mathf.RoundToInt(enemyData.ATK * damagePercentage);

        // 3. Find targets and apply status
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);
        int playersHit = 0;

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    // Call the Burning method on the target (CharactorInheritance)
                    // This method handles setting BurnDGMPerTurn and BurnTurnAmount
                    targetPlayer.Burning(burnDGMPerTurn, burnTurns);
                    targetPlayer.isOnBurning = true;
                    Debug.Log(gameObject.name + " applied Burning to " + hitCollider.name + ".");
                    playersHit++;
                }
            }
        }

        // 4. Final Check: Ensure the skill actually did something
        if (playersHit > 0)
        {
            // Wait for the skill animation/effect time
            animator.SetTrigger("isSkill2");
            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            // If we spent the points but hit nothing, we refund the cost.
            EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
            Debug.Log("CultistEmber failed to hit any player. Cost refunded to Global SP Pool.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AOE_RADIUS);
    }
}
