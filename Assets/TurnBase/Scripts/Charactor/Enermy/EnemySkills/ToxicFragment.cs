using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicFragment : MonoBehaviour
{
    private EnemyData enemyData;

    [Tooltip("The Skill Point cost deducted from the Global Pool to use this ability.")]
    public int skillCost = 5;

    [Tooltip("Duration in seconds the camera should pause after the skill effect.")]
    public float skillPauseDuration = 1.5f; // NEW: Pause duration

    // Radius must cover 1 tile in all directions (3x3 grid total). 
    private const float AOE_RADIUS = 2.8f;

    // NOTE: Set the correct LayerMask for players in the Unity Inspector
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
    /// Executes the full skill: checks cost, deducts from global pool, applies damage, and pauses.
    /// </summary>
    /// <returns>IEnumerator to manage the skill sequence and timing.</returns>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("ToxicFragment failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Check and Deduct from Global Pool ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log(gameObject.name + " failed to use skill due to insufficient global SP.");
            yield break; // Fails to use skill
        }

        // --- 2. Apply Damage ---
        int skillDamage = enemyData.ATK / 2;
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, AOE_RADIUS, targetLayer);

        int playersHit = 0;

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    targetPlayer.TakeDamage(skillDamage);
                    playersHit++;
                }
            }
        }

        // --- 3. Pause for Animation/Effect ---
        // This is where the camera holds on the enemy!
        Debug.Log(gameObject.name + " used Toxic Fragment! Waiting " + skillPauseDuration + "s.");
        yield return new WaitForSeconds(skillPauseDuration);


        // 4. Final Check: Ensure the skill actually did something
        if (playersHit > 0)
        {
            Debug.Log($"Toxic Fragment success. Hit {playersHit} targets.");
            // No return true needed in IEnumerator, the coroutine simply finishes.
        }
        else
        {
            // If we spent the points but hit nothing, we refund the cost.
            EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
            Debug.Log("Toxic Fragment failed to hit any player. Cost refunded to Global SP Pool.");
        }
    }
}
