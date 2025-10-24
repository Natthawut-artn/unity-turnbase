using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earthshatter : MonoBehaviour
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
        // Earthshatter requires EnemyData to access ATK stat
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
    /// Executes the Earthshatter skill: checks cost, deducts from global pool, and applies damage.
    /// Damage = (0.5 * ATK) * (Number of Players Hit).
    /// </summary>
    /// <returns>True if cost was deducted and damage was applied, false otherwise.</returns>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("Earthshatter failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Transaction Check (Before Target Scan) ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            yield break;
        }

        // --- 2. Find Targets and Calculate Base Damage ---
        // Base damage is 50% of ATK, which is then multiplied by the player count
        float baseDamage = (float)enemyData.ATK * 0.5f;

        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);
        List<PlayerData> playersToHit = new List<PlayerData>();

        // Filter and collect actual PlayerData components
        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    playersToHit.Add(targetPlayer);
                }
            }
        }

        // --- 3. Apply Damage Multiplier ---
        int playersHitCount = playersToHit.Count;

        if (playersHitCount == 0)
        {
            // Missed everyone, refund the cost and exit
            EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
            Debug.Log("Earthshatter missed all targets. Cost refunded.");
            yield break;
        }

        // Calculate final damage using the player count as the multiplier
        int finalSkillDamage = Mathf.CeilToInt(baseDamage * playersHitCount);

        // --- 4. Apply Damage to Targets ---
        foreach (PlayerData targetPlayer in playersToHit)
        {
            targetPlayer.TakeDamage(finalSkillDamage);
            HPBarsHUDController.Update();
            Debug.Log(gameObject.name + " slammed " + targetPlayer.gameObject.name + " for " + finalSkillDamage + " total damage (x" + playersHitCount + ").");
        }

        // --- 5. Animation Delay ---
        yield return new WaitForSeconds(1.5f);

        // Skill succeeded (points were spent and damage was dealt)
        // No refund needed
    }
}
