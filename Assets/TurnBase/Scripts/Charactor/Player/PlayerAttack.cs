using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Removed unused UnityEditor and Unity.VisualScripting references

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;

    [Header("Support Character Effect")]
    [Tooltip("Prefab of the sword effect to spawn on the enemy for Support characters.")]
    public GameObject supportAttackEffectPrefab; // NEW: Prefab reference
    [Tooltip("How long the sword effect stays before being destroyed.")]
    public float effectDuration = 2.0f; // NEW: Duration for the effect
    // --- NEW STATE VARIABLES for Targeting Mode (Option 2) ---
    [Header("Targeting State")]
    public bool isTargetingMode = false;
    // This variable stores the Transform of the confirmed target until the attack sequence is complete.
    public Transform currentSelectedEnemy = null;
    public float attackRange = 1.5f;
    private Animator animator;
    private PlayerController player;
    public int playerAttackPoint = 1;
    public int playerAttacked = 0;
    public bool hasAttackedThisTurn = false;
    public GameObject AtkArea;
    private SkillBook skillBook;
    private PlayerData playerData;


    public LayerMask WhatIsEnemy;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = GetComponent<PlayerController>();
        skillBook = GetComponent<SkillBook>();
        playerData = GetComponent<PlayerData>();
        SetTargetingVisuals(false); // Ensure visuals are off at start
    }

    // --- NEW HELPER: Manages the visual indicator on the enemy ---
    private void SetEnemyIndicator(Transform enemy, bool active)
    {
        if (enemy == null) return;

        // IMPORTANT: Looks for a child GameObject named "SelectionIndicator"
        Transform indicator = enemy.Find("SelectionIndicator");
        if (indicator != null)
        {
            indicator.gameObject.SetActive(active);
        }
        else
        {
            Debug.LogWarning($"Enemy {enemy.name} is missing a child GameObject named 'SelectionIndicator'. Cannot set visual.");
        }
    }

    /// <summary>
    /// Replaces the old Attack() method. Toggles the targeting state when Spacebar is pressed.
    /// </summary>
    public void ToggleTargetingMode()
    {
        if (hasAttackedThisTurn || TurnManager.Instance.currentPhase != Phase.Player)
        {
            if (hasAttackedThisTurn) Debug.Log("Already attacked this turn.");
            return;
        }

        // --- DESELECTION LOGIC (Manual Cancel) ---
        if (isTargetingMode)
        {
            // Clear the indicator if the user presses Spacebar to cancel targeting
            SetEnemyIndicator(currentSelectedEnemy, false);
            currentSelectedEnemy = null;
        }

        // Toggle the state
        isTargetingMode = !isTargetingMode;
        SetTargetingVisuals(isTargetingMode);

        if (isTargetingMode)
        {
            Debug.Log("Entered Targeting Mode. Left-Click an enemy to attack.");
        }
        else
        {
            Debug.Log("Exited Targeting Mode.");
        }
    }

    /// <summary>
    /// The actual damage execution, called by PlayerController after a mouse click on an enemy.
    /// </summary>
    /// <param name="target">The Transform of the clicked enemy.</param>
    public void ConfirmAttack(Transform target)
    {
        if (hasAttackedThisTurn || TurnManager.Instance.currentPhase != Phase.Player)
        {
            return;
        }

        currentSelectedEnemy = target; // Store reference for use in SwitchTurn/Cleanup

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            // ✅ Face the selected enemy
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0; // keep rotation flat on ground
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }

            // --- SHOW INDICATOR ON TARGET ---
            SetEnemyIndicator(target, true);

            Debug.Log("Attack confirmed against " + target.name + ". Initiating animation.");

            // --- Attack successful: Play animation and proceed with turn logic ---
            if (animator != null)
            {
                animator.SetTrigger("Attacking");
                animator.applyRootMotion = false;
            }

            playerAttacked++;

            // Start the coroutine to handle damage, skill, and turn switch
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null)
            {
                StartCoroutine(SwitchTurn(targetCollider));
            }
            else
            {
                Debug.LogError("Target enemy is missing a Collider component!");
                // Clean up visual state if attack failed due to missing component
                SetEnemyIndicator(target, false);
                currentSelectedEnemy = null;
            }

            // Exit targeting mode immediately upon initiating attack
            isTargetingMode = false;
            SetTargetingVisuals(false);
        }
        else
        {
            Debug.Log("Target is out of range (" + distanceToTarget.ToString("F1") + "m). Attack failed.");
            // Reset targeting mode
            isTargetingMode = false;
            SetTargetingVisuals(false);
            currentSelectedEnemy = null;
        }
    }

    private void SetTargetingVisuals(bool active)
    {
        if (AtkArea != null)
        {
            // Activates the visual indicator for attack range
            AtkArea.SetActive(active);
        }
    }

    private void OnEnable()
    {
        if (AtkArea != null && isTargetingMode)
        {
            AtkArea.SetActive(true);
        }
    }

    private void OnDisable()
    {
        SetTargetingVisuals(false);
        // Clean up indicator on the enemy if the PlayerAttack script is disabled
        SetEnemyIndicator(currentSelectedEnemy, false);
        isTargetingMode = false;
        currentSelectedEnemy = null;
    }

    void Update()
    {
        // Spacebar now calls the toggle function to enter/exit targeting
        // Note: The old Input.GetKeyDown(KeyCode.Mouse0) is removed here
        if (Input.GetKeyDown(KeyCode.Space) && TurnManager.Instance.isInTurnSystem)
        {
            ToggleTargetingMode();
        }

        if (Input.GetKeyDown(KeyCode.M) && TurnManager.Instance.isInTurnSystem)
        {
            // Skill activation logic (assuming skillBook is not null)
            if (skillBook != null && !skillBook.skillsSet[0].isActivate)
            {
                skillBook.skillsSet[0].UseSkill(gameObject);
                EdgeBurst[] smashSkills = FindObjectsOfType<EdgeBurst>();
                foreach (EdgeBurst skill in smashSkills)
                {
                    // Check if the enemy owning the skill is alive and enabled
                    if (skill != null && skill.enabled && skill.gameObject.activeInHierarchy)
                    {
                        // Set the flag on each enemy with the skill
                        skill.playerTriggeredSkill = true;
                        Debug.Log($"Notified {skill.gameObject.name} that player used a skill.");
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    IEnumerator SwitchTurn(Collider targetCollider)
    {
        hasAttackedThisTurn = true;

        // Wait for animation to finish
        yield return new WaitForSeconds(3f);

        // --- Damage application ---
        EnemyData enemyData = targetCollider.GetComponent<EnemyData>();
        if (enemyData != null)
        {
            enemyData.EnemyTakeDamage(playerData.currentATK);
        }
        if (playerData != null && gameObject.name.Contains("Support") && supportAttackEffectPrefab != null)
        {
            // Instantiate the effect at the enemy's position + a slight offset if needed
            Vector3 spawnPosition = targetCollider.transform.position + Vector3.up * 1.0f; // Example offset
            GameObject effectInstance = Instantiate(supportAttackEffectPrefab, spawnPosition, Quaternion.identity);

            // Ensure the effect plays towards the player or has a fixed rotation
            effectInstance.transform.LookAt(transform.position + Vector3.up * 1.0f); // Make it look towards the attacker slightly elevated

            // Destroy the effect after a duration
            Destroy(effectInstance, effectDuration);
            Debug.Log($"Spawned support attack effect on {targetCollider.name}");
            yield return new WaitForSeconds(1f);
        }
        // --- Skill Activation Logic ---
        if (playerData.ishavebuff && skillBook != null)
        {
            skillBook.skillsSet[0].Activate(targetCollider.gameObject, gameObject);
        }

        if(targetCollider != null)
        {
            SetEnemyIndicator(targetCollider.transform, false);
        }
            
        // --- Turn completion logic ---
        player.alreadyAttack = true;
        GameManager.Instance.CheckEnemys();

        currentSelectedEnemy = null; // Clear target after turn actions complete
    }

    public void ResetAttackStatus()
    {
        // --- HIDE INDICATOR ON TARGET ---
        // Safety check to ensure any previously selected indicator is turned off
        if (currentSelectedEnemy != null)
        {
            SetEnemyIndicator(currentSelectedEnemy, false);
        }

        hasAttackedThisTurn = false;
        playerAttacked = 0;
        isTargetingMode = false;
        SetTargetingVisuals(false);
        currentSelectedEnemy = null;
        Debug.Log("Player attack status reset for the new turn.");
    }
}
