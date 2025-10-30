using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreamoftheVampireLord : MonoBehaviour
{
    private CharactorInheritance characterStats; // Boss/Enemy stats

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) to check for players. 10 tiles = 20 units if stepSize is 2.")]
    public float skillRange = 20f;

    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 15;

    [Tooltip("Percentage of the enemy's ATK used as base damage (e.g., 0.03 for 3%).")]
    [Range(0.01f, 1.0f)]
    public float damagePercentage = 0.03f; // 3%

    [Header("Debuff Settings")]
    [Tooltip("The defense reduction percentage (e.g., 0.2 for 20%).")]
    [Range(0.01f, 1.0f)]
    public float defenseDebuffPercentage = 0.2f; // 20%

    [Tooltip("The number of turns the defense debuff lasts.")]
    public int debuffTurns = 2;

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
            Debug.LogError("ScreamOfTheVampireLord requires a CharactorInheritance component.");
        }
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

    /// <summary>
    /// Called by the EnemySkillDecider. Checks conditions and executes the skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null || EnemySkillPoint.Instance == null)
        {
            Debug.LogError("ScreamOfTheVampireLord: Missing CharacterStats or EnemySkillPoint Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find players in range)
        List<PlayerData> playersInRange = FindPlayersInRange();

        if (playersInRange.Count == 0)
        {
            // No players in range, don't execute
            yield break;
        }

        // 2. Check Global Skill Point Cost
        if (EnemySkillPoint.Instance.currentSkillPointPool < skillCost)
        {
            Debug.Log("ScreamOfTheVampireLord check failed: Not enough Global Skill Points.");
            yield break;
        }

        // --- All Conditions Met: Execute Skill ---

        // 3. Spend Skill Points
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log("ScreamOfTheVampireLord check failed: Failed to spend points.");
            yield break;
        }

        Debug.Log($"{gameObject.name} executing Scream of the Vampire Lord on {playersInRange.Count} players.");

        // 4. Focus Camera & Play Animation
        MainCamera.Instance.SetPlayer(gameObject.transform);
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Skill1"); // Use your animation trigger

        // Wait for wind-up animation
        yield return new WaitForSeconds(0.5f);

        // 5. Calculate Damage
        int damageAmount = Mathf.RoundToInt(characterStats.ATK * damagePercentage);
        if (damageAmount < 1) damageAmount = 1;

        // 6. Apply Damage and Debuff to all targets
        foreach (PlayerData player in playersInRange)
        {
            if (player == null || player.currentHP <= 0) continue;

            // --- A: Apply small damage ---
            player.TakeDamage(damageAmount);
            HPBarsHUDController.Update(); // Update health bars
            Debug.Log($"Scream hit {player.name} for {damageAmount} damage.");

            // --- B: Apply Defense Debuff ---
            // Calculate 20% of the player's BASE defense
            int defenseReduction = Mathf.RoundToInt(player.Defense * defenseDebuffPercentage);

            // Only apply the debuff if the player isn't already under this effect
            // This prevents stacking and calculation errors
            if (!player.isOnDefenseDebuff)
            {
                player.isOnDefenseDebuff = true;
                player.DefenseDebuffAmount = defenseReduction; // Store the amount to restore later
                player.currentDefense -= defenseReduction; // Apply the debuff

                // Ensure defense doesn't go negative
                if (player.currentDefense < 0) player.currentDefense = 0;

                Debug.Log($"Applied {defenseReduction} DEF debuff to {player.name}.");
            }

            // (Re)set the turn counter
            player.DefenseDebuffTurnAmount = debuffTurns;
        }

        HPBarsHUDController.Update(); // Update health bars

        // 7. Set Success Flag
        DidExecuteSuccessfully = true;

        // 8. Wait for Skill Animation/Effect
        yield return new WaitForSeconds(skillPauseDuration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}
