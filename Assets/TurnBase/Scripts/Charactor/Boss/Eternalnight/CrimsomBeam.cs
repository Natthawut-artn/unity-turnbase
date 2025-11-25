using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Used for HashSet to prevent double-hits

public class CrimsonBeam : MonoBehaviour
{
    private CharactorInheritance characterStats; // Boss's stats

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) to check for players. 10 tiles = 20 units if stepSize is 2.")]
    public float skillRange = 20f;

    [Tooltip("The width of the beam (in Unity units). 1 tile = 2 units.")]
    public float beamWidth = 2f;

    [Tooltip("The Skill Point cost deducted from the Global Pool.")]
    public int skillCost = 15;

    [Tooltip("Percentage of the enemy's ATK used as base damage (e.g., 0.5 for 50%).")]
    [Range(0.1f, 2.0f)]
    public float damagePercentage = 0.5f; // 50%

    [Tooltip("Percentage of MAX HP to heal per player hit (e.g., 0.05 for 5%).")]
    [Range(0.01f, 1.0f)]
    public float healPerPlayerHit = 0.05f; // 5%

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
            Debug.LogError("CrimsonBeam requires a CharactorInheritance component (EnemyData or BossData).");
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
    /// Called by the EnemySkillDecider. Checks conditions and executes the skill.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null || EnemySkillPoint.Instance == null)
        {
            Debug.LogError("CrimsonBeam cannot execute: Missing CharacterStats or EnemySkillPoint Singleton.");
            yield break;
        }

        // 1. Check Trigger Condition (Find nearest player to aim at)
        PlayerData targetPlayer = FindNearestPlayerInRange();

        // Fail if no player is in range to aim at
        if (targetPlayer == null)
        {
            yield break; // No target, don't execute
        }

        // 2. Check Global Skill Point Cost
        if (EnemySkillPoint.Instance.currentSkillPointPool < skillCost)
        {
            Debug.Log("CrimsonBeam check failed: Not enough Global Skill Points.");
            yield break;
        }

        // --- All Conditions Met: Execute Skill ---

        // 3. Spend Skill Points
        if (!EnemySkillPoint.Instance.TrySpendGlobalSkillPoints(skillCost))
        {
            Debug.Log("CrimsonBeam check failed: Failed to spend points.");
            yield break;
        }

        Debug.Log($"{gameObject.name} executing CrimsonBeam (aiming at {targetPlayer.name}).");

        // 4. Focus Camera & Rotate to face the nearest player
        MainCamera.Instance.SetPlayer(gameObject.transform);
        Vector3 lookPos = targetPlayer.transform.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Skill2"); // Use your animation trigger

        SFXManager.Instance?.PlayOneShot(SFX.CrimsonBeam);
        yield return new WaitForSeconds(5f); // 0.75s wind-up for a beam

        // 5. Calculate Damage & Heal amounts
        int damageAmount = Mathf.RoundToInt(characterStats.ATK * damagePercentage);
        int healAmountPerHit = Mathf.RoundToInt(characterStats.HP * healPerPlayerHit); // Use Max HP (HP)
        int totalHeal = 0;

        // 6. Fire the "thick beam" (SphereCastAll)
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,   // Start point
            beamWidth / 2,        // Radius of the beam
            transform.forward,    // Direction boss is facing
            skillRange,           // Max distance of the beam
            targetLayer           // Only hit players
        );

        // Use a HashSet to ensure we only hit and heal from each player once
        HashSet<PlayerData> playersHit = new HashSet<PlayerData>();

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out PlayerData player) && player.currentHP > 0 && !playersHit.Contains(player))
            {
                // Apply Damage
                player.TakeDamage(damageAmount);
                HPBarsHUDController.Update();
                Debug.Log($"CrimsonBeam hit {player.name} for {damageAmount} damage.");

                // Add to total heal
                totalHeal += healAmountPerHit;

                // Add to set to prevent hitting them again
                playersHit.Add(player);
            }
        }

        // 7. Apply Total Heal to Boss
        if (totalHeal > 0)
        {
            characterStats.Healing(totalHeal);
            Debug.Log($"CrimsonBeam healed {gameObject.name} for {totalHeal} HP ({playersHit.Count} players hit).");
        }

        // 8. Set Success Flag
        DidExecuteSuccessfully = true;

        // 9. Wait for Skill Animation/Effect
        yield return new WaitForSeconds(skillPauseDuration);
    }
    private void OnDrawGizmos()
    {
        // Draw skill range sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRange);
        // Draw beam width representation
        Vector3 beamStart = transform.position;
        Vector3 beamEnd = transform.position + transform.forward * skillRange;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(beamStart + transform.right * (beamWidth / 2), beamEnd + transform.right * (beamWidth / 2));
        Gizmos.DrawLine(beamStart - transform.right * (beamWidth / 2), beamEnd - transform.right * (beamWidth / 2));
    }
}
