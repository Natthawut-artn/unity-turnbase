using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostveilPrison : MonoBehaviour
{
    private EnemyData enemyData;

    [Header("Skill Parameters")]
    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 5;

    [Tooltip("Size of the area checked around the enemy.")]
    public Vector3 attackBoxSize = new Vector3(9f, 2f, 9f); // Covers a large 9x9 area (or similar)

    [Tooltip("Percentage of Enemy ATK to use as base damage (e.g., 0.25 for 25%).")]
    [Range(0.01f, 1f)]
    public float damagePercentage = 0.25f;

    [Tooltip("Number of turns the Frost debuff lasts.")]
    public int frostTurns = 1;

    // NOTE: Set the correct LayerMask for players in the Unity Inspector
    public LayerMask targetLayer;

    private void Awake()
    {
        // Get the specific EnemyData component from this GameObject
        enemyData = GetComponent<EnemyData>();
    }

    /// <summary>
    /// Checks if at least one player is within the skill's area of effect.
    /// </summary>
    /// <returns>True if a player is found, false otherwise.</returns>
    public bool IsPlayerInAttackRange()
    {
        // Physics.OverlapBox is used here, similar to your Frostbite example
        Collider[] hitTargets = Physics.OverlapBox(transform.position, attackBoxSize / 2f, Quaternion.identity, targetLayer);

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                // Verify the target is a Player/Hero and not another Enemy
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Executes the full skill: checks cost against global pool, deducts, applies status, and damages.
    /// </summary>
    /// <returns>IEnumerator to manage the skill sequence and timing.</returns>
    public IEnumerator ExecuteSkill()
    {
        if (EnemySkillPoint.Instance == null || enemyData == null)
        {
            Debug.LogError("FrostveilPrison failed: Missing required component/Singleton.");
            yield break;
        }

        // --- 1. Check and Deduct from Global Pool ---
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log(gameObject.name + " failed to use skill due to insufficient global SP.");
            yield break; // Fails to use skill
        }

        // --- 2. Calculate Damage ---
        // Damage is a percentage of the enemy's ATK stat
        int baseDamage = Mathf.RoundToInt((float)enemyData.ATK * damagePercentage);

        // --- 3. Find and Affect Targets ---
        Collider[] hitTargets = Physics.OverlapBox(transform.position, attackBoxSize / 2f, Quaternion.identity, targetLayer);
        int playersHit = 0;

        foreach (Collider hitCollider in hitTargets)
        {
            if (hitCollider.gameObject != gameObject && hitCollider.TryGetComponent(out PlayerData targetPlayer))
            {
                if (targetPlayer.heroclass != HeroClass.Enemy && targetPlayer.heroclass != HeroClass.Boss)
                {
                    // a) Apply Damage
                    targetPlayer.TakeDamage(baseDamage);
                    HPBarsHUDController.Update();

                    // b) Apply Frost Status (assuming PlayerData inherits status fields/methods)
                    targetPlayer.isOnFrost = true;
                    targetPlayer.Frost(frostTurns, baseDamage, true); // Assuming this applies the initial effect and sets up the debuff

                    Debug.Log(gameObject.name + " froze " + hitCollider.name + " for " + baseDamage + " damage.");
                    playersHit++;
                }
            }
        }

        // --- 4. Pause for Animation/Effect ---
        yield return new WaitForSeconds(1.5f); // Pause for effect visibility

        // 5. Final Check: Refund if the skill hit nothing
        if (playersHit > 0)
        {
            Debug.Log($"FrostveilPrison success. Hit {playersHit} targets.");
        }
        else
        {
            // If we spent the points but hit nothing, we refund the cost.
            EnemySkillPoint.Instance.currentSkillPointPool += skillCost;
            Debug.Log("FrostveilPrison failed to hit any player. Cost refunded to Global SP Pool.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, attackBoxSize);
    }
}
