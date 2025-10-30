using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBreaker : MonoBehaviour
{
    private CharactorInheritance characterStats; // Use base class for Boss/Enemy compatibility

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) to check for players. 6 tiles = 12 units if stepSize is 2.")]
    public float skillRange = 12f;

    [Tooltip("The minimum number of players in range required to trigger this skill.")]
    public int minPlayersToTrigger = 2;

    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 0;

    [Tooltip("Percentage of the enemy's ATK used as base damage (e.g., 0.5 for 50%).")]
    [Range(0.1f, 2.0f)]
    public float damagePercentage = 0.5f; // 50%

    [Tooltip("Time the camera should hold after the attack animation finishes.")]
    public float skillPauseDuration = 1.5f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    // Flag to signal success back to the EnemySkillDecider
    [HideInInspector] public bool DidExecuteSuccessfully = false;

    private void Awake()
    {
        characterStats = GetComponent<CharactorInheritance>();
        if (characterStats == null)
        {
            Debug.LogError("WorldBreaker requires a CharactorInheritance component (EnemyData or BossData).");
        }
    }

    /// <summary>
    /// Helper to find all active PlayerData components within the skillRange.
    /// </summary>
    private List<PlayerData> FindPlayersInRange()
    {
        List<PlayerData> foundPlayers = new List<PlayerData>();
        // Use OverlapSphere to find all colliders in the defined range on the target layer
        Collider[] hits = Physics.OverlapSphere(transform.position, skillRange, targetLayer);

        foreach (Collider hit in hits)
        {
            // Check if the collider has a PlayerData component and is alive
            if (hit.TryGetComponent(out PlayerData playerData) && playerData.currentHP > 0)
            {
                foundPlayers.Add(playerData);
            }
        }
        return foundPlayers;
    }

    /// <summary>
    /// Called by the EnemySkillDecider. Checks all conditions and executes the skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        // Reset success flag at the start of the check
        DidExecuteSuccessfully = false;

        if (characterStats == null || EnemySkillPoint.Instance == null)
        {
            Debug.LogError("WorldBreaker cannot execute: Missing CharacterStats or EnemySkillPoint Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find players in range)
        List<PlayerData> playersInRange = FindPlayersInRange();

        // Fail if not enough players are in range
        if (playersInRange.Count < minPlayersToTrigger)
        {
            // Debug.Log("WorldBreaker check failed: Not enough players in range.");
            yield break;
        }

        // 2. Check Global Skill Point Cost
        if (EnemySkillPoint.Instance.currentSkillPointPool < skillCost)
        {
            Debug.Log("WorldBreaker check failed: Not enough Global Skill Points.");
            yield break;
        }

        // --- All Conditions Met: Execute Skill ---

        // 3. Spend Skill Points
        // We use TrySpend here as a final safety check
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log("WorldBreaker check failed: Failed to spend points (race condition?).");
            yield break;
        }

        Debug.Log($"{gameObject.name} executing WorldBreaker on {playersInRange.Count} players.");

        // 4. Focus Camera & Play Animation
        MainCamera.Instance.SetPlayer(gameObject.transform);
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("WorldBreakerAttack"); // Use your animation trigger

        // Wait for wind-up animation
        yield return new WaitForSeconds(0.5f);

        // 5. Calculate Damage
        int damageAmount = Mathf.RoundToInt(characterStats.ATK * damagePercentage);

        // 6. Apply Damage to all targets
        foreach (PlayerData player in playersInRange)
        {
            if (player != null && player.currentHP > 0)
            {
                player.TakeDamage(damageAmount);
                HPBarsHUDController.Update(); // Update health bars
                Debug.Log($"WorldBreaker hit {player.name} for {damageAmount} damage.");
            }
        }

        // 7. Set Success Flag
        DidExecuteSuccessfully = true;

        // 8. Wait for Skill Animation/Effect
        yield return new WaitForSeconds(skillPauseDuration);
    }
    private void OnDrawGizmos()
    {
        // Visualize the skill range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}
