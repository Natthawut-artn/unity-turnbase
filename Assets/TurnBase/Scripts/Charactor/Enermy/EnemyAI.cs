using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    // Tracks occupied grid positions (X,Z)
    
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

        agent.stoppingDistance = 0.05f;

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        enemyData = GetComponent<EnemyData>();
        enemyData.currentHP = enemyData.HP;

        // Register starting position
        currentGridPos = GetGridPosition(transform.position);


        currentGridPos = GetGridPosition(transform.position);
        SetMarkerOccupancy(currentGridPos, true);


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
    private void SetMarkerOccupancy(Vector2 gridPos, bool isOccupied)
    {
        if (parentRoom == null || BoardManager.Instance == null) return;

        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, parentRoom.gameObject);
        if (markerGO != null)
        {
            Marker marker = markerGO.GetComponent<Marker>();
            if (marker != null)
            {
                marker.IsOccupied = isOccupied;
            }
        }
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
        Vector3 intendedDestination = transform.position; // Default to current position (no move)

        if (!playerInAttackRange)
        {
            if (agent == null) yield break;

            // 1. DETERMINE MOVEMENT DESTINATION
            if (!playerInSightRange)
                Patroling();
            else if (playerInSightRange && !Stopzone)
                ChasePlayer();

            // Retrieve the destination set by Patroling/ChasePlayer
            intendedDestination = agent.destination;

            // 2. CHECK IF A MOVE WAS PLANNED (destination changed from current position)
            // We use a small tolerance to account for float inaccuracy.
            bool isMoving = Vector3.Distance(transform.position, intendedDestination) > 0.1f;

            if (isMoving)
            {
                Vector2 targetGrid = GetGridPosition(intendedDestination);

                // --- CRITICAL FIX: RESERVE NEW TILE AND RELEASE OLD TILE ---

                // RESERVE the target tile *BEFORE* movement starts, but only if it's currently free
                // NOTE: Patroling/ChasePlayer should already prevent this, but this adds robustness.

                // We can skip the occupancy check here if we trust Patroling/ChasePlayer methods.
                // Assuming we trust those methods:

                // Claim the new tile
                SetMarkerOccupancy(targetGrid, true);

                // Release the old tile ONLY IF we are moving to a DIFFERENT tile
                if (oldGrid != targetGrid)
                {
                    SetMarkerOccupancy(oldGrid, false);
                }

                // -------------------------------------------------------------

                // 3. EXECUTE MOVEMENT
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

                // 4. Update the current grid position variable to the final landed spot
                currentGridPos = GetGridPosition(transform.position);

                // NOTE: The tile is already marked as occupied from the reservation step.
                // We do NOT call SetMarkerOccupancy(currentGridPos, true) again.
            }
            else
            {
                // If no move was planned (e.g., ChasePlayer found no path, or Patroling found no walkpoint)
                animator.SetBool("isWalking", false);

                // IMPORTANT: If we didn't move, we still occupy the old tile, so ensure it's claimed
                SetMarkerOccupancy(oldGrid, true);
            }
        }

        if (enemyData.isOnBloodFlowing)
        {
            enemyData.BloodFlowing();
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
            // Vector2 targetGrid = GetGridPosition(walkPoint); // Not needed
            GameObject targetMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(GetGridPosition(walkPoint), parentRoom.gameObject);
            Marker targetMarker = (targetMarkerGO != null) ? targetMarkerGO.GetComponent<Marker>() : null;

            // ⭐ FIX: Check IsOccupied on the Marker
            if (targetMarker != null && !targetMarker.IsOccupied)
            {
                agent.SetDestination(walkPoint);
                animator.SetBool("isWalking", true);
            }
            else
            {
                walkPointSet = false;
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

            GameObject candidateMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, parentRoom.gameObject);
            bool isCandidateOccupied = (candidateMarkerGO != null && candidateMarkerGO.GetComponent<Marker>().IsOccupied);


            // Check if on ground AND not occupied
            if (Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround) && !isCandidateOccupied)
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
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= StoppingDistance) // Use attackRange or StoppingDistance here
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
            return;
        }

        Vector3 direction = (Player.position - transform.position).normalized;

        // Main grid direction toward player
        Vector3 mainDir = Vector3.zero;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            mainDir = new Vector3(Mathf.Sign(direction.x), 0, 0);
        else
            mainDir = new Vector3(0, 0, Mathf.Sign(direction.z));

        Vector3 mainTarget = transform.position + mainDir * stepSize;
        mainTarget = new Vector3(Mathf.Round(mainTarget.x), transform.position.y, Mathf.Round(mainTarget.z));
        Vector2 mainGrid = GetGridPosition(mainTarget);

        // ⭐ FIX: Check IsOccupied on the Marker
        GameObject mainTargetMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(mainGrid, parentRoom.gameObject);
        bool isMainTargetOccupied = (mainTargetMarkerGO != null && mainTargetMarkerGO.GetComponent<Marker>().IsOccupied);

        // 🟩 Try main path first
        if (!isMainTargetOccupied)
        {
            agent.SetDestination(mainTarget);
            animator.SetBool("isWalking", true);
            return;
        }

        // 🟨 If blocked, check alternative (adjacent, unoccupied) paths

        // Define all 4 cardinal step directions
        Vector3[] cardinalDirs = new Vector3[]
        {
        new Vector3(stepSize, 0, 0),    // Right
        new Vector3(-stepSize, 0, 0),   // Left
        new Vector3(0, 0, stepSize),    // Forward (Z-axis)
        new Vector3(0, 0, -stepSize)    // Backward (Z-axis)
        };

        float bestDist = Mathf.Infinity;
        Vector3 bestPoint = transform.position;
        bool foundBetter = false;

        // Check all 4 adjacent steps
        foreach (var dir in cardinalDirs)
        {
            Vector3 candidate = transform.position + dir;
            candidate = new Vector3(Mathf.Round(candidate.x), transform.position.y, Mathf.Round(candidate.z));
            Vector2 grid = GetGridPosition(candidate);

            // 1. Check if the target marker is occupied
            GameObject candidateMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(grid, parentRoom.gameObject);
            bool isCandidateOccupied = (candidateMarkerGO != null && candidateMarkerGO.GetComponent<Marker>().IsOccupied);

            if (isCandidateOccupied) continue;

            // 2. Check if the candidate spot is on the ground/valid walk point
            if (!Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround)) continue;

            // 3. Only move forward or sideways toward player (prevents stepping backwards unless necessary)
            Vector3 toCandidate = (candidate - transform.position).normalized;
            float forwardDot = Vector3.Dot(toCandidate, direction);

            // This threshold (0.1f) means it must be moving forward or mostly sideways toward the player
            if (forwardDot < 0.1f) continue;

            // 4. Find the closest valid, unoccupied point to the player
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
            agent.SetDestination(bestPoint);
            animator.SetBool("isWalking", true);
        }
        else
        {
            // No good path found this turn (completely blocked on all valid adjacent steps)
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
        SetMarkerOccupancy(currentGridPos, false);
    }

    private void DestroySelf()
    {
        SetMarkerOccupancy(currentGridPos, false);
        Destroy(gameObject);
    }

}
