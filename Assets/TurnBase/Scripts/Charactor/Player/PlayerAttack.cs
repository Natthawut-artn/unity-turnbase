using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack Instance;
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    private Animator animator;
    private PlayerController player;



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
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsEnemy);
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
            closestEnemy.GetComponent<EnemyData>().TakeDamage(attackDamage);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && TurnManager.Instance.isInTurnSystem)
        {
            Attack();
            if (animator != null) animator.SetTrigger("Attacking");
            StartCoroutine(SwitchTurn());
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    IEnumerator SwitchTurn()
    {
        yield return new WaitForSeconds(5f);
        player.alreadyAttack = true;
    }

  
}


