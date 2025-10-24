using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private EnemySkillDecider skillDecider; // NEW: Reference for skill usage
    private Vector2 currentGridPos;
    public static EnemyAI Instance;
    public NavMeshAgent agent;
    public Transform Player;
    public float StoppingDistance = 2f;

    public LayerMask WhatIsGround, WhatIsPlayer, WhatIsEnemy; // WhatIsEnemy added for targeting check

    public Vector3 walkPoint;
    bool walkPointSet;
    public float stepSize = 2f;    // Grid step size

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

        // --- Initialization Fix ---
        skillDecider = GetComponent<EnemySkillDecider>();
        // --- End Initialization Fix ---

        // Find the initial player target (though this is updated dynamically in TakeTurn)
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            Player = playerGO.transform;
        }


        // Disable auto rotation
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.stoppingDistance = 0.05f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        enemyData = GetComponent<EnemyData>();

        if (enemyData != null)
        {
            enemyData.currentHP = enemyData.HP;
        }

        // Register starting position
        currentGridPos = GetGridPosition(transform.position);

        SetMarkerOccupancy(currentGridPos, true);
    }
    void Start()
    {
        // Player reference is now found dynamically or in Awake, cleaning up Start redundancy
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
                Debug.Log($"Marker {gridPos} set to {isOccupied}");
            }
        }
    }
    private Vector2 GetGridPosition(Vector3 worldPos)
    {
        return new Vector2(Mathf.Round(worldPos.x), Mathf.Round(worldPos.z));
    }

    // NEW: Helper function to check if a tile is occupied by another unit
    private bool IsGridOccupied(Vector2 gridPos)
    {
        if (parentRoom == null || BoardManager.Instance == null) return false;

        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, parentRoom.gameObject);
        if (markerGO != null)
        {
            Marker marker = markerGO.GetComponent<Marker>();
            // True if occupied AND the position is not the enemy's current position
            if (marker != null && marker.IsOccupied)
            {
                // This check is a final failsafe, as the movement should check occupancy via the marker.
                // However, we ensure the enemy isn't checking its own current position as "occupied" for a move calculation.
                return gridPos != currentGridPos;
            }
        }
        return false;
    }


    public IEnumerator TakeTurn()
    {
        if (agent == null || enemyData.currentHP <= 0) yield break;

        // 1. DYNAMICALLY FIND PLAYER TARGET (since players can die/switch)
        // Find the first alive player unit
        GameObject targetPlayerGO = GameObject.FindWithTag("Player");
        if (targetPlayerGO != null)
        {
            Player = targetPlayerGO.transform;
        }
        else
        {
            // If no player found, skip turn
            yield break;
        }
        
        MainCamera.Instance.SetPlayer(gameObject.transform);
        
        if (skillDecider != null)
        {
            // Execute the Decider's coroutine and WAIT for it to finish.
            yield return StartCoroutine(skillDecider.DecideAndExecuteSkill());

            // CRITICAL FIX: Check the flag set by the Decider to end the turn.
            if (skillDecider.SkillExecutedThisTurn)
            {
                yield break; // Skill was used, turn is over.
            }
        }
       

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        Vector2 oldGrid = currentGridPos;

        if (playerInAttackRange)
        {
            // Attack and end turn
            AttackPlayer();
            yield return new WaitForSeconds(1.5f);
            yield break;
        }

        // MOVEMENT LOGIC
        if (!playerInAttackRange)
        {
            if (agent == null) yield break;

            // 1. DETERMINE MOVEMENT DESTINATION
            if (!playerInSightRange)
                Patroling();
            else // playerInSightRange (Chase)
                ChasePlayer();

            Vector3 intendedDestination = agent.destination;
            Vector2 targetGrid = GetGridPosition(intendedDestination);

            // 2. CHECK IF A MOVE WAS PLANNED (destination changed from current position)
            bool isMoving = targetGrid != oldGrid;

            // 3. FINAL CANCELLATION CHECK: Is the target spot occupied by another unit?
            if (isMoving && IsGridOccupied(targetGrid))
            {
                // Cancel the move, stay put, but don't skip the turn (it just won't move)
                isMoving = false;
                agent.SetDestination(transform.position);
                Debug.Log(gameObject.name + " blocked from moving to " + targetGrid + ". Staying put.");
            }

            if (isMoving)
            {
                // 4. RESERVE NEW TILE AND RELEASE OLD TILE
                SetMarkerOccupancy(targetGrid, true);

                // Release the old tile ONLY IF we are moving to a DIFFERENT tile
                SetMarkerOccupancy(oldGrid, false);


                // 5. EXECUTE MOVEMENT
                float maxTime = 2f;
                float timer = 0f;

                while (agent != null && agent.pathPending == false &&
                        agent.remainingDistance > agent.stoppingDistance + 0.05f)
                {
                    if (this == null) yield break;
                    FaceMovementDirection();
                    animator.SetBool("isWalking", true);
                    timer += Time.deltaTime;
                    if (timer >= maxTime) break;
                    yield return null;
                }

                if (agent == null) yield break;
                animator.SetBool("isWalking", false);

                // 6. FIX: SNAP TO GRID
                // Force position to the precise grid center of the intended destination
                transform.position = new Vector3(
                    Mathf.Round(transform.position.x),
                    transform.position.y,
                    Mathf.Round(transform.position.z)
                );

                // 7. Update the current grid position variable to the final landed spot
                currentGridPos = GetGridPosition(transform.position);

            }
            else
            {
                // If no move was planned (e.g., already at destination or path blocked)
                animator.SetBool("isWalking", false);
                // Ensure the current tile is claimed
                SetMarkerOccupancy(oldGrid, true);
            }
        }
    }


    public void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            Vector2 targetGrid = GetGridPosition(walkPoint);

            // Check if occupied BEFORE setting destination
            if (!IsGridOccupied(targetGrid))
            {
                agent.SetDestination(walkPoint);
                animator.SetBool("isWalking", true);
            }
            else
            {
                // If occupied, search for a new point
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

            // Check if on ground AND not occupied
            if (Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround) && !IsGridOccupied(gridPos))
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

        // If the enemy is close enough to attack range, stay put
        if (distance <= StoppingDistance)
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
            return;
        }

        Vector3 direction = (Player.position - transform.position).normalized;

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

            // 1. Check if the target marker is occupied (by another unit)
            if (IsGridOccupied(grid)) continue;

            // 2. Check if the candidate spot is on the ground/valid walk point
            if (!Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround)) continue;

            // 3. Find the tile that results in the shortest distance to the player (modified logic)
            float candidateDist = Vector3.Distance(candidate, Player.position);

            // We only care about tiles that are strictly closer to the player than the current position.
            if (candidateDist < distance)
            {
                // 4. Find the closest valid, unoccupied point to the player
                if (candidateDist < bestDist)
                {
                    bestDist = candidateDist;
                    bestPoint = candidate;
                    foundBetter = true;
                }
            }
        }

        if (foundBetter)
        {
            agent.SetDestination(bestPoint);
            animator.SetBool("isWalking", true);
        }
        else
        {
            // No better path found (completely blocked or only paths lead away)
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

    // This method MUST be public so EnemyData.Die() can call it for cleanup.
    public void DestroySelf()
    {
        SetMarkerOccupancy(currentGridPos, false);
        // Delaying destruction slightly to allow other turn logic to finish
        Destroy(gameObject, 0.1f);
    }

    private void OnDestroy()
    {
        // This is a safety measure. The primary release should happen in DestroySelf().
        SetMarkerOccupancy(currentGridPos, false);
    }
}
