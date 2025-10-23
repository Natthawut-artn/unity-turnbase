using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Removed unnecessary 'using' statements like Unity.VisualScripting and UnityEditor

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;

    // --- NEW: Static variable to hold the currently selected target ---
    public static Transform selectedEnemyTarget = null;
    // This will hold the Transform of the enemy the player clicked on.

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
    }

    public void Attack()
    {
        if (hasAttackedThisTurn || TurnManager.Instance.currentPhase != Phase.Player)
        {
            return;
        }
        
        // --- MODIFIED: Check if an enemy is selected ---
        if (selectedEnemyTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, selectedEnemyTarget.position);

            // --- MODIFIED: Check if the selected enemy is in range ---
            if (distanceToTarget <= attackRange)
            {
                // ✅ Face the selected enemy
                Vector3 dir = (selectedEnemyTarget.position - transform.position).normalized;
                dir.y = 0; // keep rotation flat on ground
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }

                // Get the EnemyData component from the selected target
                EnemyData enemyData = selectedEnemyTarget.GetComponent<EnemyData>();

                if (enemyData != null)
                {
                    // ✅ Damage the selected enemy
                    Debug.Log("Hit " + selectedEnemyTarget.name);

                    // --- Attack successful: Play animation and proceed with turn logic ---

                    if (animator != null)
                    {
                        animator.SetTrigger("Attacking");
                        animator.applyRootMotion = false;
                    }

                    playerAttacked++;

                    if (playerAttacked >= playerAttackPoint)
                    {
                        StartCoroutine(SwitchTurn(selectedEnemyTarget));
                    }

                    // Optional: Deselect the enemy after attacking
                    DeselectEnemy();
                }
                else
                {
                    Debug.LogError("Selected enemy is missing EnemyData component!");
                    DeselectEnemy();
                }
            }
            else
            {
                Debug.Log("Selected enemy is out of range!");
            }
        }
        else
        {
            // Optional: Add a debug log for not having a target
            Debug.Log("Please select an enemy target first.");
        }
    }

    // --- NEW: Method to clear the selected target (call this after an attack or turn end) ---
    public static void DeselectEnemy()
    {
        if (selectedEnemyTarget != null)
        {
            // You might want to add visual feedback here, e.g., turn off a selection ring
            // Example: selectedEnemyTarget.GetComponent<EnemySelection>()?.SetSelectedVisual(false);
            selectedEnemyTarget = null;
            Debug.Log("Enemy Deselected.");
        }
    }

    private void OnEnable()
    {
        if (AtkArea != null)
        {
            AtkArea.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (AtkArea != null)
        {
            AtkArea.SetActive(false);
        }
        // Deselect when the player attack component is disabled
        DeselectEnemy();
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0)) && TurnManager.Instance.isInTurnSystem)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.M) && TurnManager.Instance.isInTurnSystem)
        {
            if (!skillBook.skillsSet[0].isActivate)
            {
                skillBook.skillsSet[0].UseSkill(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    IEnumerator SwitchTurn(Transform closestEnemy)
    {
        hasAttackedThisTurn = true;
        yield return new WaitForSeconds(5f);
        closestEnemy.gameObject.GetComponent<EnemyData>().EnemyTakeDamage(playerData.currentATK);
        if (playerData.ishavebuff)
        {
            skillBook.skillsSet[0].Activate(closestEnemy.gameObject , gameObject);
        }
        player.alreadyAttack = true;
        GameManager.Instance.CheckEnemys();

    }
    public void ResetAttackStatus()
    {
        hasAttackedThisTurn = false;
        playerAttacked = 0;
        DeselectEnemy(); // Also deselect enemy on turn reset
        Debug.Log("Player attack status reset for the new turn.");
    }

}