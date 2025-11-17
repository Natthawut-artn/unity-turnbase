using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleSlash : MonoBehaviour
{
    // FIX: Use EnemyData directly
    private EnemyData enemyData;

    [Header("Skill Parameters")]
    [Tooltip("The range (in Unity units) within which a player must be at the end of the Player Phase to trigger the skill.")]
    public float TriggerRange = 5f; // 5 tiles if stepSize is 2

    [Tooltip("Percentage of the enemy's ATK used as damage.")]
    [Range(0.1f, 1.0f)]
    public float DamagePercentage = 0.5f; // 50% of ATK

    [Tooltip("Time the camera should hold after the attack animation finishes.")]
    public float SkillPauseDuration = 1.5f;

    [Tooltip("LayerMask containing only Player units.")]
    public LayerMask targetLayer;

    private bool _skillExecutedThisCheck = false;

    private void Awake()
    {
        // FIX: Get the specific EnemyData component
        enemyData = GetComponent<EnemyData>();
        if (enemyData == null)
        {
            Debug.LogError("MultipleSlash requires an EnemyData component on the same GameObject.");
        }
    }

    /// <summary>
    /// Called by the TurnManager at the end of the Player Phase.
    /// Checks if any player is in range and executes the skill if conditions are met.
    /// </summary>
    public IEnumerator CheckAndExecuteSkill()
    {
        // FIX: Use the EnemyData reference for checks
        if (enemyData == null || enemyData.currentHP <= 0)
        {
            yield break; // Don't execute if dead or missing data
        }

        _skillExecutedThisCheck = false;

        List<PlayerData> playersInRange = FindPlayersInRange();

        if (playersInRange.Count > 0)
        {
            Debug.Log($"MultipleSlash triggered: {playersInRange.Count} players in range.");
            MainCamera.Instance.SetPlayer(gameObject.transform);

            // Execute the Attack using stats from enemyData
            yield return StartCoroutine(ExecuteDamage(playersInRange));

            _skillExecutedThisCheck = true;
        }
    }

    /// <summary>
    /// Finds all active PlayerData components within the TriggerRange.
    /// </summary>
    private List<PlayerData> FindPlayersInRange()
    {
        List<PlayerData> foundPlayers = new List<PlayerData>();
        Collider[] hits = Physics.OverlapSphere(transform.position, TriggerRange, targetLayer);

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
    /// Applies damage to the list of players and waits for animation time.
    /// </summary>
    private IEnumerator ExecuteDamage(List<PlayerData> targets)
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Skill3");
        }

        yield return new WaitForSeconds(3.5f);

        // FIX: Calculate Damage using the EnemyData reference
        int damageAmount = Mathf.RoundToInt(enemyData.ATK * DamagePercentage);

        foreach (PlayerData player in targets)
        {
            if (player != null && player.currentHP > 0)
            {
                player.TakeDamage(damageAmount);
                HPBarsHUDController.Update();
                Debug.Log($"MultipleSlash hit {player.name} for {damageAmount} damage.");
            }
        }

        // Player death check should happen within PlayerData.TakeDamage

        yield return new WaitForSeconds(SkillPauseDuration);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, TriggerRange);
    }
}

