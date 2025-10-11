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

    public Room parentRoom;
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
        Player = GameObject.FindWithTag("Player").transform;
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
        if (agent == null) yield break;
        MainCamera.Instance.SetPlayer(gameObject.transform);

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        Vector2 oldGrid = currentGridPos;

        if (!playerInAttackRange)
        {
            if (agent == null) yield break;
            if (!playerInSightRange)
                Patroling();
            else if (playerInSightRange && !Stopzone)
                ChasePlayer();

            OccupiedPositions.Remove(oldGrid);

            float maxTime = 2f;
            float timer = 0f;
            while (agent != null && agent.pathPending == false &&
                   agent.remainingDistance > agent.stoppingDistance + 0.05f)
            {
                if (this == null) yield break;
                FaceMovementDirection();
                timer += Time.deltaTime;
                if (timer >= maxTime) break;
                yield return null;
            }

            if (agent == null) yield break;
            animator.SetBool("isWalking", false);

            currentGridPos = GetGridPosition(transform.position);
            OccupiedPositions.Add(currentGridPos);
        }

        if (playerInAttackRange && this != null)
        {
            AttackPlayer();
            yield return new WaitForSeconds(1.5f);
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
        Vector3 direction = (Player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= StoppingDistance)
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
            return;
        }

        // Main grid direction toward player
        Vector3 mainDir = Vector3.zero;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            mainDir = new Vector3(Mathf.Sign(direction.x), 0, 0);
        else
            mainDir = new Vector3(0, 0, Mathf.Sign(direction.z));

        Vector3 mainTarget = transform.position + mainDir * stepSize;
        mainTarget = new Vector3(Mathf.Round(mainTarget.x), transform.position.y, Mathf.Round(mainTarget.z));
        Vector2 mainGrid = GetGridPosition(mainTarget);

        // 🟩 Try main path first
        if (!OccupiedPositions.Contains(mainGrid))
        {
            // Reserve target immediately to avoid overlap
            OccupiedPositions.Add(mainGrid);
            agent.SetDestination(mainTarget);
            animator.SetBool("isWalking", true);
            return;
        }

        // 🟨 If blocked, check side paths that still move generally toward player
        Vector3[] altDirs = new Vector3[]
        {
        new Vector3(mainDir.x, 0, 0),
        new Vector3(0, 0, mainDir.z),
        new Vector3(-mainDir.x, 0, 0),
        new Vector3(0, 0, -mainDir.z)
        };

        float bestDist = Mathf.Infinity;
        Vector3 bestPoint = transform.position;
        bool foundBetter = false;

        foreach (var dir in altDirs)
        {
            Vector3 candidate = transform.position + dir * stepSize;
            candidate = new Vector3(Mathf.Round(candidate.x), transform.position.y, Mathf.Round(candidate.z));
            Vector2 grid = GetGridPosition(candidate);

            if (OccupiedPositions.Contains(grid)) continue;
            if (!Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround)) continue;

            // Only move forward or sideways toward player
            Vector3 toCandidate = (candidate - transform.position).normalized;
            float forwardDot = Vector3.Dot(toCandidate, direction);
            if (forwardDot < 0.1f) continue;

            float candidateDist = Vector3.Distance(candidate, Player.position);
            if (candidateDist < bestDist)
            {
                bestDist = candidateDist;
                bestPoint = candidate;
                foundBetter = true;
            }
        }

        if (foundBetter)
        {
            Vector2 targetGrid = GetGridPosition(bestPoint);
            OccupiedPositions.Add(targetGrid); // Reserve before moving
            agent.SetDestination(bestPoint);
            animator.SetBool("isWalking", true);
        }
        else
        {
            // No good path found this turn
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
            FaceMovementDirection();
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
                HPBarsHUDController.Update();
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

    void Update()
    {
        // Destroy if room is unplaying
        if (parentRoom == null || parentRoom.roomState == RoomState.UnPlaying)
        {
            DestroySelf();
        }

        // Destroy if player goes back to hub
        if (GameManager.Instance != null && AppStateManager.appState == AppState.Hub)
        {
            DestroySelf();
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

    private void DestroySelf()
    {
        if (OccupiedPositions.Contains(currentGridPos))
            OccupiedPositions.Remove(currentGridPos);

        Destroy(gameObject);
    }

}
