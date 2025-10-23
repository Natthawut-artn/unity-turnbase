using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;
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

            // --- Attack successful: Play animation and proceed with turn logic ---

            if (animator != null)
            {
                animator.SetTrigger("Attacking");
                animator.applyRootMotion = false;
            }

            playerAttacked++;

            if (playerAttacked >= playerAttackPoint)
            {
                StartCoroutine(SwitchTurn(closestEnemy));
            }
        }
        else
        {
            // Optional: Add a debug log or sound effect for a failed attack attempt
            Debug.Log("No enemy in range to attack.");
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
    }
    

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && TurnManager.Instance.isInTurnSystem)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.M) && TurnManager.Instance.isInTurnSystem)
        {
            skillBook.skillsSet[0].UseSkill(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    IEnumerator SwitchTurn(Collider closestEnemy)
    {
        hasAttackedThisTurn = true;
        yield return new WaitForSeconds(5f);
        closestEnemy.GetComponent<EnemyData>().EnemyTakeDamage(playerData.currentATK);
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
        Debug.Log("Player attack status reset for the new turn.");
    }

}


