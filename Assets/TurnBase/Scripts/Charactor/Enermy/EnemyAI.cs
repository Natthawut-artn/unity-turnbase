using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private EnemySkillDecider skillDecider;
    private Vector2 currentGridPos;
    public static EnemyAI Instance;
    public NavMeshAgent agent;
    public Transform Player;
    public float StoppingDistance = 2f;

    [Header("Enemy Avoidance")]
    [Tooltip("Radius around the candidate tile to check for other friendly units, preventing crowding.")]
    public float CrowdingRadius = 1.0f; // NEW PROPERTY

    public LayerMask WhatIsGround, WhatIsPlayer, WhatIsEnemy;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float stepSize = 2f;

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

        skillDecider = GetComponent<EnemySkillDecider>();

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            Player = playerGO.transform;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.stoppingDistance = 0.05f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        enemyData = GetComponent<EnemyData>();

        if (enemyData != null)
        {
            enemyData.currentHP = enemyData.HP;
        }

        currentGridPos = GetGridPosition(transform.position);
        SetMarkerOccupancy(currentGridPos, true);
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
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(transform.position, CrowdingRadius); // Uncomment to visualize crowding area
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

    /// <summary>
    /// Checks if the tile at gridPos is claimed by any unit *other* than this enemy.
    /// </summary>
    private bool IsTileClaimedByOtherUnit(Vector2 gridPos)
    {
        if (parentRoom == null || BoardManager.Instance == null) return false;

        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, parentRoom.gameObject);
        if (markerGO != null)
        {
            Marker marker = markerGO.GetComponent<Marker>();
            // 1. Is the marker occupied at all?
            if (marker != null && marker.IsOccupied)
            {
                // 2. Is the occupied position NOT our current position?
                return gridPos != GetGridPosition(transform.position);
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the tile at the given position is too close to another friendly enemy unit.
    /// Uses the public CrowdingRadius.
    /// </summary>
    private bool IsTileAttackRangeOccupied(Vector3 position)
    {
        // Check for other enemies within the CrowdingRadius
        // Ensure WhatIsEnemy LayerMask is set correctly!
        Collider[] hitEnemies = Physics.OverlapSphere(position, CrowdingRadius, WhatIsEnemy);

        foreach (Collider collider in hitEnemies)
        {
            // Ignore ourselves if we are part of the overlap test (unlikely, but safe)
            if (collider.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }


    public IEnumerator TakeTurn()
    {
        if (agent == null || enemyData.currentHP <= 0) yield break;

        // 1. DYNAMICALLY FIND PLAYER TARGET & CAMERA SET
        GameObject targetPlayerGO = GameObject.FindWithTag("Player");
        if (targetPlayerGO != null)
        {
            Player = targetPlayerGO.transform;
            MainCamera.Instance.SetPlayer(gameObject.transform);
        }
        else
        {
            yield break;
        }

        // 2. SKILL DECISION CHECK (PRIORITY 1)
        if (skillDecider != null)
        {
            yield return StartCoroutine(skillDecider.DecideAndExecuteSkill());

            if (skillDecider.SkillExecutedThisTurn)
            {
                yield break; // Skill was used, turn is over.
            }
        }

        // 3. REGULAR ATTACK/MOVEMENT FLOW
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, WhatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, WhatIsPlayer);

        Vector2 oldGrid = currentGridPos;

        if (playerInAttackRange)
        {
            AttackPlayer();
            yield return new WaitForSeconds(1.5f);
            yield break;
        }

        // MOVEMENT LOGIC
        if (!playerInAttackRange)
        {
            if (agent == null) yield break;

            // DETERMINE MOVEMENT DESTINATION
            if (!playerInSightRange)
                Patroling();
            else
                ChasePlayer();

            Vector3 intendedDestination = agent.destination;
            Vector2 targetGrid = GetGridPosition(intendedDestination);

            // CHECK IF A MOVE WAS PLANNED 
            Vector2 oldGridPos = GetGridPosition(transform.position);
            bool isMoving = targetGrid != oldGridPos;

            // FINAL CANCELLATION CHECK: Tile Claiming
            if (isMoving && IsTileClaimedByOtherUnit(targetGrid))
            {
                isMoving = false; // Cancel the move
                agent.SetDestination(transform.position); // Stay put
            }

            if (isMoving)
            {
                // RESERVE NEW TILE AND RELEASE OLD TILE
                SetMarkerOccupancy(targetGrid, true);
                SetMarkerOccupancy(oldGrid, false);

                // EXECUTE MOVEMENT
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

                // SNAP TO GRID
                transform.position = new Vector3(
                    Mathf.Round(transform.position.x),
                    transform.position.y,
                    Mathf.Round(transform.position.z)
                );

                // Update the current grid position variable
                currentGridPos = GetGridPosition(transform.position);
            }
            else
            {
                animator.SetBool("isWalking", false);
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
            Vector3 candidatePos = new Vector3(targetGrid.x, transform.position.y, targetGrid.y);

            // Check stacking AND crowding before setting destination
            if (!IsTileClaimedByOtherUnit(targetGrid) && !IsTileAttackRangeOccupied(candidatePos))
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

            // Check if on ground, NOT occupied, AND NOT crowded
            if (Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround) &&
                !IsTileClaimedByOtherUnit(gridPos) &&
                !IsTileAttackRangeOccupied(candidate))
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
        if (Player == null) return;

        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= StoppingDistance)
        {
            agent.SetDestination(transform.position);
            animator.SetBool("isWalking", false);
            return;
        }

        // Define all 4 cardinal step directions
        Vector3[] cardinalDirs = new Vector3[]
        {
            new Vector3(stepSize, 0, 0),
            new Vector3(-stepSize, 0, 0),
            new Vector3(0, 0, stepSize),
            new Vector3(0, 0, -stepSize)
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

            // 1. Check stacking
            if (IsTileClaimedByOtherUnit(grid)) continue;

            // 2. Check overcrowding
            if (IsTileAttackRangeOccupied(candidate)) continue;

            // 3. Check if valid ground
            if (!Physics.Raycast(candidate, -transform.up, 2f, WhatIsGround)) continue;

            float candidateDist = Vector3.Distance(candidate, Player.position);

            // We only move if it brings us strictly closer to the player
            if (candidateDist < distance)
            {
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
        if (Player == null) return;

        agent.SetDestination(transform.position);
        Vector3 targetPos = new Vector3(Player.position.x, transform.position.y, Player.position.z);
        transform.LookAt(targetPos);

        if (!alreadyAttacked)
        {
            animator.SetTrigger("isAttack");

            int baseDamage = 10;

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, WhatIsPlayer);
            foreach (Collider player in hitEnemies)
            {
                if (player.TryGetComponent(out PlayerData playerData))
                {
                    // --- FIX: Implement Dual-Action Attack ---

                    // 1. Always deal the base damage.
                    playerData.TakeDamage(baseDamage);
                    HPBarsHUDController.Update();
                    // 2. Conditionally activate the synergy effect.
                    if (playerData.isOnBloodFlowing)
                    {
                        // This method is assumed to apply the bonus damage/effect and handle status cleanup.
                        playerData.BloodFlowing(baseDamage);
                        HPBarsHUDController.Update();
                        Debug.Log($"Blood Flowing synergy triggered on {player.name}.");

                        playerData.isOnBloodFlowing = false;
                        Debug.Log("Blood Flowing status consumed and reset.");
                    }

                    // --- END FIX ---
                }
            }

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
        if (parentRoom == null || parentRoom.roomState == RoomState.UnPlaying)
        {
            DestroySelf();
        }

        if (GameManager.Instance != null && AppStateManager.appState == AppState.Hub)
        {
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        SetMarkerOccupancy(currentGridPos, false);
        Destroy(gameObject, 0.1f);
    }
}