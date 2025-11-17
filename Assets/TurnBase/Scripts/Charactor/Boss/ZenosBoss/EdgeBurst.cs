using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeBurst : MonoBehaviour
{
    // Changed to generic CharactorInheritance to grab stats from EnemyData or BossData
    private EnemyData enemyData;
    public bool playerTriggeredSkill = false;
    public static bool DidExecuteSuccessfully = false;
    [Header("Skill Parameters")]
    [Tooltip("The range (in units) within which a player must be to trigger this skill.")]
    public float UsageRange = 10f;

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
    public Animator animator;
    private void Awake()
    {
        // FIX: Use CharactorInheritance, which EnemyData and BossData both inherit from.
        enemyData = GetComponent<EnemyData>();

        if (enemyData == null)
        {
            Debug.LogError("FrontalSmash requires a CharactorInheritance component (EnemyData or BossData) to function.");
        }

        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Finds the nearest active player's transform within the UsageRange.
    /// </summary>
    public Transform FindNearestPlayer()
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
    /// Checks conditions, rotates the unit to the nearest target, and executes the line attack.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;
        if (enemyData == null)
        {
            playerTriggeredSkill = false;
            yield break;
        }

        // 1. Find the nearest player within the trigger range
        Transform target = FindNearestPlayer();

        if (target == null || !playerTriggeredSkill)
        {
            playerTriggeredSkill = false;
            yield break;
        }

        // 2. Rotate the unit instantly to face the target.
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);

        animator.SetTrigger("Skill1");
        yield return new WaitForSeconds(3f);

        // 4. EXECUTE ATTACK LOGIC
        if (TileDamageMultipliers == null || TileDamageMultipliers.Length < AttackRangeTiles)
        {
            Debug.LogError($"Frontal Smash: Falloff array must be at least {AttackRangeTiles} elements long!");
            playerTriggeredSkill = false;
            yield break;
        }

        // Use the current ATK stat from the inherited component
        float maxDamage = (float)enemyData.ATK;
        int playersHitCount = 0;

        // Iterate through each tile along the frontal path
        for (int tileDistance = 1; tileDistance <= AttackRangeTiles; tileDistance++)
        {
            float damageMultiplier = TileDamageMultipliers[tileDistance - 1];
            damageMultiplier = Mathf.Max(0f, damageMultiplier);

            int finalDamage = Mathf.RoundToInt(maxDamage * damageMultiplier);

            Vector3 checkPosition = transform.position + transform.forward * (tileDistance * GridStepSize);

            // Find and Damage Targets at this tile position
            Collider[] hitTargets = Physics.OverlapSphere(checkPosition, HitCheckRadius, targetLayer);

            foreach (Collider hitCollider in hitTargets)
            {
                if (hitCollider.TryGetComponent(out PlayerData targetPlayer))
                {
                    if (targetPlayer.currentHP <= 0) continue;

                    targetPlayer.TakeDamage(finalDamage);
                    HPBarsHUDController.Update();
                    playersHitCount++;

                    Debug.Log($"Smash Hit {hitCollider.name} at Tile {tileDistance} for {finalDamage} damage.");
                }
            }
            
        }
        DidExecuteSuccessfully = true;
        playerTriggeredSkill = false;
        // 5. Final Pause and Cleanup
        yield return new WaitForSeconds(SkillPauseDuration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, UsageRange);
        Gizmos.color = Color.yellow;
        for (int tileDistance = 1; tileDistance <= AttackRangeTiles; tileDistance++)
        {
            Vector3 checkPosition = transform.position + transform.forward * (tileDistance * GridStepSize);
            Gizmos.DrawWireSphere(checkPosition, HitCheckRadius);
        }
    }
}
