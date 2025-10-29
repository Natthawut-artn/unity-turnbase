using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApocalypeBloom : MonoBehaviour
{
    private CharactorInheritance characterStats; // Use base class for Boss/Enemy compatibility

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) within which players trigger the damage.")]
    public float skillRange = 10f;

    [Tooltip("Fixed damage amount dealt by the skill.")]
    public int damageAmount = 300;

    [Tooltip("Number of turns the skill is inactive after use.")]
    public int cooldownTurns = 5;

    [Tooltip("Time the camera should hold after the attack animation finishes.")]
    public float skillPauseDuration = 2f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    // --- Cooldown Tracking ---
    public int currentCooldown = 0; // Starts ready to use

    Animator animator;
    private void Awake()
    {
        characterStats = GetComponent<CharactorInheritance>();
        if (characterStats == null)
        {
            Debug.LogError("ApocalypeBloom requires a CharactorInheritance component (EnemyData or BossData).");
        }
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Call this method at the start of the enemy's turn phase to reduce the cooldown.
    /// </summary>
    public void DecrementCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown--;
            Debug.Log($"{gameObject.name} ApocalypeBloom cooldown decremented: {currentCooldown} turns remaining.");
        }
    }

    /// <summary>
    /// Checks if any player is within the skill range. Used by EnemySkillDecider.
    /// </summary>
    public bool IsPlayerInSkillRange()
    {
        // Simple check just for range, not cooldown
        Collider[] hits = Physics.OverlapSphere(transform.position, skillRange, targetLayer);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerData playerData) && playerData.currentHP > 0)
            {
                return true; // Found at least one valid player
            }
        }
        return false;
    }

    /// <summary>
    /// Executes the Bloom skill if cooldown is ready and players are in range.
    /// Called by TurnManager at the END of the enemy's turn segment.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator ExecuteSkill()
    {
        // 1. Check Cooldown
        if (currentCooldown > 0)
        {
            // Log is optional, TurnManager already handles the call timing.
            // Debug.Log($"{gameObject.name} ApocalypeBloom is on cooldown ({currentCooldown} turns left).");
            yield break; // Exit silently if on cooldown
        }

        if (characterStats == null || characterStats.currentHP <= 0)
        {
            // Don't execute if owner is dead or missing stats
            yield break;
        }

        // 2. Find Players in Range
        List<PlayerData> playersInRange = FindPlayersInRange();
        if (playersInRange.Count == 0)
        {
            // Debug.Log($"{gameObject.name} ApocalypeBloom: No players in range.");
            yield break; // Exit silently if no targets
        }

        Debug.Log($"{gameObject.name} executing ApocalypeBloom on {playersInRange.Count} players.");

        // --- Execute the Attack ---
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("ApocalypeBloomAttack"); // Use your animation trigger name

        // Focus camera on the unit using the skill
        MainCamera.Instance.SetPlayer(gameObject.transform);

        // Wait for wind-up
        yield return new WaitForSeconds(0.5f);

        // Apply Damage
        foreach (PlayerData player in playersInRange)
        {
            if (player != null && player.currentHP > 0) // Double check target validity
            {
                player.TakeDamage(damageAmount);
                HPBarsHUDController.Update();
                Debug.Log($"ApocalypeBloom hit {player.name} for {damageAmount} damage.");
            }
        }

        // 3. Start Cooldown
        currentCooldown = cooldownTurns;
        Debug.Log($"{gameObject.name} ApocalypeBloom used. Cooldown set to {currentCooldown} turns.");

        animator.SetTrigger("Skill");
        // Wait for the skill animation/effect to visually complete
        yield return new WaitForSeconds(skillPauseDuration);
    }

    /// <summary>
    /// Helper to find all active PlayerData components within the skillRange.
    /// </summary>
    private List<PlayerData> FindPlayersInRange()
    {
        List<PlayerData> foundPlayers = new List<PlayerData>();
        Collider[] hits = Physics.OverlapSphere(transform.position, skillRange, targetLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerData playerData) && playerData.currentHP > 0)
            {
                foundPlayers.Add(playerData);
            }
        }
        return foundPlayers;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}


