using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Required for List filtering

public class BloodveilSaber : MonoBehaviour
{
    private CharactorInheritance characterStats; // Boss's stats

    [Header("Skill Parameters")]
    [Tooltip("The inner radius of the 'safe zone'. Players INSIDE this range (5 tiles = 10 units) will NOT be hit.")]
    public float safeZoneRadius = 10f;

    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 20;

    [Tooltip("The total number of random hits this skill performs.")]
    public int hitCount = 6;

    [Tooltip("Percentage of the enemy's ATK used as base damage per hit (e.g., 0.5 for 50%).")]
    [Range(0.1f, 2.0f)]
    public float damagePercentage = 0.5f; // 50%

    [Tooltip("The short pause between each random hit.")]
    public float timeBetweenHits = 0.3f;

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
            Debug.LogError("BloodveilSaber requires a CharactorInheritance component.");
        }
    }

    /// <summary>
    /// Helper to find all living players who are OUTSIDE the safe zone radius.
    /// </summary>
    private List<PlayerData> FindEligiblePlayers()
    {
        List<PlayerData> eligiblePlayers = new List<PlayerData>();
        // Find all PlayerData components in the scene
        PlayerData[] allPlayers = FindObjectsOfType<PlayerData>();

        foreach (PlayerData player in allPlayers)
        {
            if (player == null || player.currentHP <= 0) continue; // Skip dead players

            // Calculate distance from the boss
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // Add to list ONLY if they are OUTSIDE the safe zone
            if (distance > safeZoneRadius)
            {
                eligiblePlayers.Add(player);
            }
        }
        return eligiblePlayers;
    }

    /// <summary>
    /// Called by the EnemySkillDecider. Checks conditions and executes the skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null || EnemySkillPoint.Instance == null)
        {
            Debug.LogError("BloodveilSaber: Missing CharacterStats or EnemySkillPoint Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find players OUTSIDE the safe zone)
        List<PlayerData> eligiblePlayers = FindEligiblePlayers();

        // Fail if no players are far away
        if (eligiblePlayers.Count == 0)
        {
            // Debug.Log("BloodveilSaber check failed: All players are too close.");
            yield break;
        }

        // 2. Check Global Skill Point Cost
        if (EnemySkillPoint.Instance.currentSkillPointPool < skillCost)
        {
            Debug.Log("BloodveilSaber check failed: Not enough Global Skill Points.");
            yield break;
        }

        // --- All Conditions Met: Execute Skill ---

        // 3. Spend Skill Points
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log("BloodveilSaber check failed: Failed to spend points.");
            yield break;
        }

        Debug.Log($"{gameObject.name} executing Bloodveil Saber on {eligiblePlayers.Count} eligible players.");

        // 4. Focus Camera & Play Animation
        MainCamera.Instance.SetPlayer(gameObject.transform);
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Skill6"); // Use your animation trigger

        // Wait for wind-up animation
        yield return new WaitForSeconds(9f);

        // 5. Calculate Damage
        int damagePerHit = Mathf.RoundToInt(characterStats.currentATK * damagePercentage);
        if (damagePerHit < 1) damagePerHit = 1;

        int playersHitThisSkill = 0;

        // 6. Apply Multi-Hit Random Damage
        for (int i = 0; i < hitCount; i++)
        {
            // Re-check the list of eligible players *for every hit*
            // This ensures we don't try to hit a player who died mid-combo
            List<PlayerData> livingEligiblePlayers = FindEligiblePlayers();

            // If all eligible players died mid-combo, stop the attack
            if (livingEligiblePlayers.Count == 0)
            {
                Debug.Log("Bloodveil Saber: All eligible targets died mid-combo.");
                break;
            }

            // Select a random target from the currently eligible list
            PlayerData randomTarget = livingEligiblePlayers[Random.Range(0, livingEligiblePlayers.Count)];

            // Apply damage
            randomTarget.TakeDamage(damagePerHit);
            HPBarsHUDController.Update();
            playersHitThisSkill++;
            Debug.Log($"Bloodveil Saber Hit {i + 1}/{hitCount} on {randomTarget.name} for {damagePerHit} damage.");

            // Wait for the next hit animation/effect
            yield return new WaitForSeconds(timeBetweenHits);
        }

        // 7. Set Success Flag (only if we landed at least one hit)
        if (playersHitThisSkill > 0)
        {
            DidExecuteSuccessfully = true;
        }

        // 8. Wait for Skill Animation/Effect to conclude
        yield return new WaitForSeconds(skillPauseDuration);
    }

    private void OnDrawGizmos()
    {
        // Visualize the safe zone radius in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, safeZoneRadius);
    }
}
