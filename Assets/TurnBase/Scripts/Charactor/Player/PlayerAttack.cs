using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public int attackDamage = 20;

    public LayerMask WhatIsEnemy;

    public void Attack()
    {
        // Detect enemies in range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsEnemy);
        // Damage them
        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            // Assume enemy has a method TakeDamage(int amount)
            enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
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
        yield return new WaitForSeconds(1f);
        TurnManager.Instance.EndTurn();
    }
}


