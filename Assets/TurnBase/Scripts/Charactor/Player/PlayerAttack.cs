using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    


    public LayerMask WhatIsEnemy;

    public void Attack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsEnemy);
        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            enemy.GetComponent<EnemyData>().TakeDamage(attackDamage);
            

        }
    }

    void Update()
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


