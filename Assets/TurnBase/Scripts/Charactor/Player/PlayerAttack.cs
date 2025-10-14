using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    private Animator animator;
    private PlayerController player;
    public int playerAttackPoint = 1;
    public int playerAttacked = 0;
    public bool hasAttackedThisTurn = false;



    public LayerMask WhatIsEnemy;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = GetComponent<PlayerController>();
    }

    public void Attack()
    {
        if (hasAttackedThisTurn || TurnManager.Instance.currentPhase != Phase.Player)
        {
            return;
        }
        
        // Check for enemies in range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsEnemy);

        // Only proceed if there is at least one enemy found
        if (hitEnemies.Length > 0)
        {
            // ✅ Find the closest enemy
            Collider closestEnemy = hitEnemies[0];
            float closestDist = Vector3.Distance(transform.position, closestEnemy.transform.position);

            foreach (Collider enemy in hitEnemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestEnemy = enemy;
                    closestDist = dist;
                }
            }

            // ✅ Face the closest enemy
            Vector3 dir = (closestEnemy.transform.position - transform.position).normalized;
            dir.y = 0; // keep rotation flat on ground
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }

            // ✅ Damage that enemy
            Debug.Log("Hit " + closestEnemy.name);
            closestEnemy.GetComponent<EnemyData>().TakeDamage(attackDamage * 10);

            // --- Attack successful: Play animation and proceed with turn logic ---

            if (animator != null)
            {
                animator.SetTrigger("Attacking");
                animator.applyRootMotion = false;
            }

            playerAttacked++;

            if (playerAttacked >= playerAttackPoint)
            {
                StartCoroutine(SwitchTurn());
            }
        }
        else
        {
            // Optional: Add a debug log or sound effect for a failed attack attempt
            Debug.Log("No enemy in range to attack.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && TurnManager.Instance.isInTurnSystem)
        {
            Attack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    IEnumerator SwitchTurn()
    {
        hasAttackedThisTurn = true;
        yield return new WaitForSeconds(5f);
        player.alreadyAttack = true;
        GameManager.Instance.CheckEnemys();
        
    }
    public void ResetAttackStatus()
    {
        hasAttackedThisTurn = false;
        playerAttacked = 0; 
        Debug.Log("Player attack status reset for the new turn.");
    }

}


