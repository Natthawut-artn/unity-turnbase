using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform Player;

    public LayerMask WhatIsGround, WhatIsPlayer;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float stepSize = 2f;   // Grid step size

    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        Player = GameObject.FindWithTag("Player").transform;

        // Disable auto rotation
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();

        FaceMovementDirection();
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


