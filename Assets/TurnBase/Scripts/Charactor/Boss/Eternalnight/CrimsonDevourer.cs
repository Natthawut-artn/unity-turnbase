using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonDevourer : MonoBehaviour
{
    private CharactorInheritance characterStats; // Boss's stats

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) to check for players. 3 tiles = 6 units if stepSize is 2.")]
    public float skillRange = 6f;

    [Tooltip("Percentage of the enemy's ATK used as base damage (e.g., 0.05 for 5%).")]
    [Range(0.01f, 1.0f)]
    public float damagePercentage = 0.05f; // 5%

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
            Debug.LogError("CrimsonDevourer requires a CharactorInheritance component (EnemyData or BossData).");
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
    /// Called by the EnemySkillDecider. Checks for the nearest player and executes the skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null || BossPhaseManager.Instance == null)
        {
            Debug.LogError("CrimsonDevourer cannot execute: Missing CharacterStats or BossPhaseManager Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find nearest player in range)
        PlayerData targetPlayer = FindNearestPlayerInRange();

        // Fail if no player is in range
        if (targetPlayer == null)
        {
            yield break; // No target, don't execute
        }

        // --- Target Found: Execute Skill ---
        Debug.Log($"{gameObject.name} executing CrimsonDevourer on {targetPlayer.name}.");

        // 2. Focus Camera & Play Animation
        MainCamera.Instance.SetPlayer(gameObject.transform);
        // Face the target
        Vector3 lookPos = targetPlayer.transform.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("CrimsonDevourerAttack"); // Use your animation trigger

        // Wait for wind-up animation
        yield return new WaitForSeconds(0.5f);

        // 3. Calculate Damage (5% ATK)
        int damageAmount = Mathf.RoundToInt(characterStats.ATK * damagePercentage);
        if (damageAmount < 1) damageAmount = 1; // Ensure at least 1 damage

        // 4. Apply Damage to the single nearest target
        if (targetPlayer != null && targetPlayer.currentHP > 0)
        {
            targetPlayer.TakeDamage(damageAmount);
            HPBarsHUDController.Update(); // Update health bars
            Debug.Log($"CrimsonDevourer hit {targetPlayer.name} for {damageAmount} damage.");
        }

        // 5. ADD STACK to the Global Manager
        BossPhaseManager.Instance.AddDevourerStack();

        // 6. Set Success Flag
        DidExecuteSuccessfully = true;

        // 7. Wait for Skill Animation/Effect
        yield return new WaitForSeconds(skillPauseDuration);
    }

    private void OnDrawGizmos()
    {
        // Visualize skill range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}
