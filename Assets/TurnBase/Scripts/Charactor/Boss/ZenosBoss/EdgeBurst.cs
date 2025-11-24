using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeBurst : MonoBehaviour
{

    [HideInInspector] public bool DidExecuteSuccessfully = false;

    private CharactorInheritance characterStats;

    [Header("Skill Parameters")]
    [Tooltip("The range (in units) to check for a target.")]
    public float UsageRange = 10f;

    public int AttackRangeTiles = 10;
    public float[] TileDamageMultipliers;
    public float GridStepSize = 2f;
    public float HitCheckRadius = 0.5f;
    public float SkillPauseDuration = 1.0f;
    public LayerMask targetLayer;

    private void Awake()
    {
        characterStats = GetComponent<CharactorInheritance>();
    }

    /// <summary>
    /// Finds the nearest active player's transform within the UsageRange.
    /// </summary>
    public Transform FindNearestPlayer()
    {
        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;
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
    /// Checks conditions (Player in Range), rotates, executes the attack, and sets the success flag.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        DidExecuteSuccessfully = false;

        if (characterStats == null)
        {
            yield break;
        }

        // 1. Find Target
        Transform target = FindNearestPlayer();

        // --- LOGIC CHANGE: Pure Proximity Check ---
        // Only check if a valid target was found in range.
        // Removed the check for playerTriggeredSkill.
        if (target == null)
        {
            yield break; // No player in range, do nothing
        }

        // --- Conditions met (Player is in range), proceed with execution ---

        // 2. Rotate to face target
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos);
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Skill1");
        }
        yield return new WaitForSeconds(3.5f);


        if (TileDamageMultipliers == null || TileDamageMultipliers.Length < AttackRangeTiles)
        {
            Debug.LogError($"Frontal Smash: Falloff array missing or too short!");
            yield break;
        }

        float maxDamage = (float)characterStats.ATK;

        // 3. Apply Damage
        for (int tileDistance = 1; tileDistance <= AttackRangeTiles; tileDistance++)
        {
 
            float damageMultiplier = TileDamageMultipliers[tileDistance - 1];
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
            int finalDamage = Mathf.RoundToInt(maxDamage * damageMultiplier);
            Vector3 checkPosition = transform.position + transform.forward * (tileDistance * GridStepSize);

            Collider[] hitTargets = Physics.OverlapSphere(checkPosition, HitCheckRadius, targetLayer);
            foreach (Collider hitCollider in hitTargets)
            {
                if (hitCollider.TryGetComponent(out PlayerData targetPlayer))
                {
                    if (targetPlayer.currentHP <= 0) continue;
                    targetPlayer.TakeDamage(finalDamage);
                }
            }
        }

        // 4. Set Success Flag
        DidExecuteSuccessfully = true;

        yield return new WaitForSeconds(SkillPauseDuration);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, UsageRange);
    }
}
