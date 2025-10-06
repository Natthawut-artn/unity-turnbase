using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    // Tracks occupied grid positions (X,Z)
    public static HashSet<Vector2> OccupiedPositions = new HashSet<Vector2>();
    private Vector2 currentGridPos;
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
    private Animator animator;
    
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

        // Register starting position
        currentGridPos = GetGridPosition(transform.position);
        OccupiedPositions.Add(currentGridPos);

    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
    private Vector2 GetGridPosition(Vector3 worldPos)
    {
        return new Vector2(Mathf.Round(worldPos.x), Mathf.Round(worldPos.z));
    }

    public IEnumerator TakeTurn()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        // Only handle movement if player not in attack range
        if (!playerInAttackRange)
        {
            if (!playerInSightRange)
                Patroling();
            else if (playerInSightRange && !Stopzone)
                ChasePlayer();

            // Wait for movement to finish
            float maxTime = 2f;
            float timer = 0f;
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.05f)
            {
                FaceMovementDirection();
                timer += Time.deltaTime;
                if (timer >= maxTime) break;
                yield return null;
            }

            animator.SetBool("isWalking", false);

            // Update current position after movement
            currentGridPos = GetGridPosition(transform.position);
            OccupiedPositions.Add(currentGridPos);
        }

        // If in attack range, just attack (no occupancy changes)
        if (playerInAttackRange)
        {
            AttackPlayer();
            yield return new WaitForSeconds(1.5f); // allow player to see attack
        }
    }


    public void Patroling()
    {

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            Vector2 targetGrid = GetGridPosition(walkPoint);
            if (!OccupiedPositions.Contains(targetGrid))
            {
                agent.SetDestination(walkPoint);
                animator.SetBool("isWalking", true);
            }
            else
            {
                walkPointSet = false; // pick a new point next turn
            }
        }

        if (Vector3.Distance(transform.position, walkPoint) < 0.2f)
        {
            walkPointSet = false;
            animator.SetBool("isWalking", false);
        }
    }

    private void SearchWalkPoint()
    {
        int attempts = 0;
        while (attempts < 10)
        {
            int dir = Random.Range(0, 4);
            Vector3 offset = Vector3.zero;

            switch (dir)
            {
                case 0: offset = new Vector3(stepSize, 0, 0); break;
                case 1: offset = new Vector3(-stepSize, 0, 0); break;
                case 2: offset = new Vector3(0, 0, stepSize); break;
                case 3: offset = new Vector3(0, 0, -stepSize); break;
            }

            Vector3 candidate = new Vector3(
                Mathf.Round(transform.position.x + offset.x),
                transform.position.y,
                Mathf.Round(transform.position.z + offset.z)
            );

            Vector2 gridPos = GetGridPosition(candidate);

            if (Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround) &&
                !OccupiedPositions.Contains(gridPos))
            {
                walkPoint = candidate;
                walkPointSet = true;
                break;
            }

            attempts++;
        }
    }


    public void ChasePlayer()
    {

        Vector3 direction = Player.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= StoppingDistance)
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);

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

        Vector2 targetGrid = GetGridPosition(chasePoint);

        if (!OccupiedPositions.Contains(targetGrid))
        {
            agent.SetDestination(chasePoint);
            animator.SetBool("isWalking", true);
        }
        else
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
        }


    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        Vector3 targetPos = new Vector3(Player.position.x, transform.position.y, Player.position.z);
        transform.LookAt(targetPos);
        if (!alreadyAttacked)
        {
            animator.SetTrigger("isAttack");

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

    private void OnDestroy()
    {
        // When enemy dies or is destroyed, free up its occupied grid cell
        if (OccupiedPositions.Contains(currentGridPos))
        {
            OccupiedPositions.Remove(currentGridPos);
        }
    }

}
