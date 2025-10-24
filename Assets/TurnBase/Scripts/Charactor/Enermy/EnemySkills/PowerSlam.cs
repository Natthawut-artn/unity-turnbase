using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSlam : MonoBehaviour
{
    private EnemyData enemyData;

    [Header("Skill Parameters")]
    [Tooltip("The Skill Point cost deducted from the Global Pool to use this ability.")]
    public int skillCost = 10;

    // Radius must cover 1 tile in all directions (3x3 grid total). 
    private const float AOE_RADIUS = 2.8f;

    [Tooltip("LayerMask containing Player units.")]
    public LayerMask targetLayer;

    private void Awake()
    {
        // PowerSlam requires EnemyData to access ATK and Defense stats
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
    /// Executes the Power Slam skill: checks cost, deducts from global pool, and applies damage.
    /// Damage is calculated as (0.5 * ATK) + (0.5 * Defense).
    /// </summary>
    /// <returns>True if cost was deducted and damage was applied, false otherwise.</returns>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("PowerSlam failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Check and Deduct from Global Pool ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            yield break;
        }

        // --- 2. Calculate Damage ---
        // Damage = (50% ATK) + (50% Defense)
        float baseDamage = (float)enemyData.ATK * 0.5f;
        float defenseBonus = (float)enemyData.Defense * 0.5f;

        // Use Mathf.CeilToInt to ensure non-fractional damage, favoring the player's perspective
        int skillDamage = Mathf.CeilToInt(baseDamage + defenseBonus);

        // --- 3. Find Targets and Apply Damage ---
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);
        int playersHit = 0;

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    targetPlayer.TakeDamage(skillDamage);
                    HPBarsHUDController.Update();
                    Debug.Log(gameObject.name + " slammed " + hitCollider.name + " for " + skillDamage + " damage (ATK+DEF scale).");
                    playersHit++;
                }
            }
        }

        // --- 4. Animation Delay & Transaction Finalization ---
        yield return new WaitForSeconds(1.5f);

        if (playersHit > 0)
        {
            // Skill succeeded
            yield break;
        }

        // 5. If we spent the points but hit nothing, we refund the cost.
        EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
        Debug.Log("PowerSlam missed all targets. Cost refunded.");
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AOE_RADIUS);
    }
}
