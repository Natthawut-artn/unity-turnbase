using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;
    public NavMeshAgent agent;
    public Transform Player;
    public float StoppingDistance = 2f;

    public LayerMask WhatIsGround, WhatIsPlayer;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float stepSize = 2f;   // Grid step size

    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public bool Stopzone = false;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    EnemyData enemyData;
    
    
    void Awake()
    {
        Instance = this;
        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindWithTag("Player").transform;

        // Disable auto rotation
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        enemyData = GetComponent<EnemyData>();
        enemyData.currentHP = enemyData.HP;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
    public IEnumerator TakeTurn()
    {
        Debug.Log($"{name} starting turn");

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        // Decide action
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange && Stopzone == false)
        {
            ChasePlayer();
        }
        else if (playerInSightRange && playerInAttackRange)
        {
            AttackPlayer();
            yield return new WaitForSeconds(1f); // wait so player sees attack
            Debug.Log($"{name} finished turn (attack)");
            yield break;
        }

        // Wait for movement
        float maxTime = 2f;
        float timer = 0f;

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.05f)
        {
            FaceMovementDirection();
            timer += Time.deltaTime;
            if (timer >= maxTime) break;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        Debug.Log($"{name} finished turn (move)");
    }

    public void Patroling()
    {

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        if (Vector3.Distance(transform.position, walkPoint) < 0.2f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        int dir = Random.Range(0, 4);
        Vector3 offset = Vector3.zero;

        switch (dir)
        {
            case 0: offset = new Vector3(stepSize, 0, 0); break;   // Right
            case 1: offset = new Vector3(-stepSize, 0, 0); break;  // Left
            case 2: offset = new Vector3(0, 0, stepSize); break;   // Forward
            case 3: offset = new Vector3(0, 0, -stepSize); break;  // Backward
        }

        walkPoint = new Vector3(
            Mathf.Round(transform.position.x + offset.x),
            transform.position.y,
            Mathf.Round(transform.position.z + offset.z)
        );

        if (Physics.Raycast(walkPoint, -transform.up, 2f, WhatIsGround))
        {
            walkPointSet = true;
        }
    }

    public void ChasePlayer()
    {
        Vector3 direction = Player.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= StoppingDistance)
        {
            agent.SetDestination(transform.position); 
            return;
        }

        // Snap direction to grid (only X or Z)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            direction = new Vector3(Mathf.Sign(direction.x), 0, 0);
        }
        else
        {
            direction = new Vector3(0, 0, Mathf.Sign(direction.z));
        }

        Vector3 chasePoint = transform.position + direction * stepSize;
        chasePoint = new Vector3(
            Mathf.Round(chasePoint.x),
            transform.position.y,
            Mathf.Round(chasePoint.z)
        );

        agent.SetDestination(chasePoint);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(Player);
        if (!alreadyAttacked)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsPlayer);
            foreach (Collider player in hitEnemies)
            {
                Debug.Log("Hit " + player.name);
                player.GetComponent<PlayerData>().TakeDamage(10);
                HPBarHUDController.UpProgression();
            }
            Debug.Log("Attacking Player");
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void FaceMovementDirection()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 dir = agent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }
}
