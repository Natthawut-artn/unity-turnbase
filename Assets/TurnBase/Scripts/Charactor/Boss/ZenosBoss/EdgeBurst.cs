using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeBurst : MonoBehaviour
{
    private BossData bossEnemyData;

    [Header("Skill Parameters")]
    [Tooltip("The range (in tiles/units) within which a player must be to trigger this counter-skill.")]
    public float UsageRange = 10f; // 5 tiles equivalent if stepSize is 2

    [Tooltip("Number of tiles the frontal attack extends.")]
    public int AttackRangeTiles = 10;

    [Tooltip("Array of multipliers (0.0 to 1.0) applied at each tile distance (Index 0 = Tile 1).")]
    public float[] TileDamageMultipliers;

    [Tooltip("Size of one grid step, used for spacing the damage checks.")]
    public float GridStepSize = 2f;

    [Tooltip("Radius for the overlap sphere check at each tile interval.")]
    public float HitCheckRadius = 0.5f;

    [Tooltip("Time the camera should hold after the attack animation finishes.")]
    public float SkillPauseDuration = 1.0f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    private void Awake()
    {
        // BossData is assumed to inherit CharactorInheritance to get the ATK stat
        bossEnemyData = GetComponent<BossData>();
    }

    /// <summary>
    /// Finds the nearest active player's transform.
    /// </summary>
    /// <returns>Transform of the nearest player, or null.</returns>
    private Transform FindNearestPlayer()
    {
        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        // Find all colliders in the immediate vicinity based on UsageRange
        Collider[] hits = Physics.OverlapSphere(transform.position, UsageRange, targetLayer);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out PlayerData playerData))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPlayer = hit.transform;
                }
            }
        }
        return nearestPlayer;
    }

    /// <summary>
    /// Checks conditions, rotates the boss to the nearest target, and executes the line attack.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_FrontalSmash failed: Boss is missing BossData component.");
            yield break;
        }

        // 1. Find the nearest player within the trigger range
        Transform target = FindNearestPlayer();

        // If no player is found within the UsageRange, the skill is not used.
        if (target == null)
        {
            yield break;
        }

        // 2. CRITICAL: Rotate the boss instantly to face the target.
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        // 3. Pause for an initial wind-up animation
        yield return new WaitForSeconds(0.5f);

        // 4. EXECUTE ATTACK LOGIC (Array-Based Falloff)
        if (TileDamageMultipliers == null || TileDamageMultipliers.Length < AttackRangeTiles)
        {
            Debug.LogError($"Frontal Smash: Falloff array must be at least {AttackRangeTiles} elements long!");
            yield break;
        }

        float maxDamage = (float)bossEnemyData.ATK;
        int playersHitCount = 0;

        // Iterate through each tile along the frontal path
        for (int tileDistance = 1; tileDistance <= AttackRangeTiles; tileDistance++)
        {
            float damageMultiplier = TileDamageMultipliers[tileDistance - 1];
            damageMultiplier = Mathf.Max(0f, damageMultiplier);

            int finalDamage = Mathf.RoundToInt(maxDamage * damageMultiplier);

            // Calculate the position of the current tile check (based on the new forward direction)
            Vector3 checkPosition = transform.position + transform.forward * (tileDistance * GridStepSize);

            // Find and Damage Targets at this tile position
            Collider[] hitTargets = Physics.OverlapSphere(checkPosition, HitCheckRadius, targetLayer);

            foreach (Collider hitCollider in hitTargets)
            {
                if (hitCollider.TryGetComponent(out PlayerData targetPlayer))
                {
                    if (targetPlayer.currentHP <= 0) continue;

                    targetPlayer.TakeDamage(finalDamage);
                    playersHitCount++;

                    Debug.Log($"Smash Hit {hitCollider.name} at Tile {tileDistance} for {finalDamage} damage (x{damageMultiplier:F2}).");
                }
            }
        }

        // 5. Final Pause and Cleanup
        yield return new WaitForSeconds(SkillPauseDuration);

        if (playersHitCount == 0)
        {
            Debug.Log("Frontal Smash missed all targets.");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the attack range in the editor
        Gizmos.color = Color.red;
        for (int tileDistance = 1; tileDistance <= AttackRangeTiles; tileDistance++)
        {
            Vector3 checkPosition = transform.position + transform.forward * (tileDistance * GridStepSize);
            Gizmos.DrawWireSphere(checkPosition, HitCheckRadius);
        }
    }
}