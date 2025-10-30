using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodClawDominion : MonoBehaviour
{
    private CharactorInheritance characterStats; // Boss's stats

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) to check for players. 3 tiles = 6 units if stepSize is 2.")]
    public float skillRange = 6f;

    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 10;

    [Tooltip("Percentage of the enemy's CURRENT ATK used as base damage per hit (e.g., 0.25 for 25%).")]
    [Range(0.1f, 1.0f)]
    public float damagePercentage = 0.25f; // 25%

    [Tooltip("The number of times the skill hits the target.")]
    public int hitCount = 3;

    [Tooltip("The short pause between each hit in the flurry.")]
    public float timeBetweenHits = 0.2f; // 0.2s between hits

    [Tooltip("Time the camera should hold after the attack animation finishes.")]
    public float skillPauseDuration = 1.0f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    // Flag to signal success back to the EnemySkillDecider
    [HideInInspector] public bool DidExecuteSuccessfully = false;

    private void Awake()
    {
        characterStats = GetComponent<CharactorInheritance>();
        if (characterStats == null)
        {
            Debug.LogError("BloodClawDominion requires a CharactorInheritance component.");
        }
    }

    /// <summary>
    /// Helper to find the NEAREST active PlayerData component within the skillRange.
    /// </summary>
    private PlayerData FindNearestPlayerInRange()
    {
        PlayerData nearestPlayer = null;
        float shortestDistance = Mathf.Infinity;

        Collider[] hits = Physics.OverlapSphere(transform.position, skillRange, targetLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerData playerData) && playerData.currentHP > 0)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPlayer = playerData;
                }
            }
        }
        return nearestPlayer; // Will be null if no players are in range
    }

    /// <summary>
    /// Called by the EnemySkillDecider. Checks conditions and executes the multi-hit skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null || EnemySkillPoint.Instance == null)
        {
            Debug.LogError("BloodClawDominion: Missing CharacterStats or EnemySkillPoint Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find nearest player in range)
        PlayerData targetPlayer = FindNearestPlayerInRange();

        if (targetPlayer == null)
        {
            yield break; // No target in 3-tile range, skill doesn't activate
        }

        // 2. Check Global Skill Point Cost
        if (EnemySkillPoint.Instance.currentSkillPointPool < skillCost)
        {
            Debug.Log("BloodClawDominion check failed: Not enough Global Skill Points.");
            yield break;
        }

        // --- All Conditions Met: Execute Skill ---

        // 3. Spend Skill Points
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log("BloodClawDominion check failed: Failed to spend points.");
            yield break;
        }

        Debug.Log($"{gameObject.name} executing BloodClaw Dominion on {targetPlayer.name}.");

        // 4. Focus Camera & Rotate to face the target
        MainCamera.Instance.SetPlayer(gameObject.transform);
        Vector3 lookPos = targetPlayer.transform.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Skill4"); // Use your animation trigger

        // Wait for wind-up animation
        yield return new WaitForSeconds(0.5f);

        // 5. Calculate Damage (based on CURRENT ATK)
        int damageAmountPerHit = Mathf.RoundToInt(characterStats.currentATK * damagePercentage);
        if (damageAmountPerHit < 1) damageAmountPerHit = 1;

        // 6. Apply Multi-Hit Damage
        for (int i = 0; i < hitCount; i++)
        {
            // Check if target died mid-combo
            if (targetPlayer == null || targetPlayer.currentHP <= 0)
            {
                Debug.Log("BloodClawDominion: Target died mid-combo.");
                break; // Stop the combo
            }

            targetPlayer.TakeDamage(damageAmountPerHit);
            HPBarsHUDController.Update();
            Debug.Log($"BloodClaw Hit {i + 1}/{hitCount} on {targetPlayer.name} for {damageAmountPerHit} damage.");

            // Wait for the next hit
            yield return new WaitForSeconds(timeBetweenHits);
        }

        // 7. Apply Status Effect
        // Check again if target is still alive before applying
        if (targetPlayer != null && targetPlayer.currentHP > 0)
        {
            targetPlayer.isOnBloodFlowing = true;
            Debug.Log($"Applied BloodFlowing status to {targetPlayer.name}.");
        }

        // 8. Set Success Flag
        DidExecuteSuccessfully = true;

        // 9. Wait for Skill Animation/Effect to conclude
        yield return new WaitForSeconds(skillPauseDuration);
    }
}
