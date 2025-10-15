using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    void Awake()
    {
        Instance = this;
        playerData = GetComponent<PlayerData>();
        playerAttack = GetComponent<PlayerAttack>();
        maxHealth = playerData.HP;
        playerData.currentHP = maxHealth;
    }

    public LayerMask MarkerLayer;
    public PlayerData playerData;
    public PlayerAttack playerAttack;
    public int maxHealth;
    public LayerMask Enemy;
    public bool alreadyAttack = false;
    public bool alreadyWalk = false;
    private Animator animator;

    // ✅ Movement limit
    public int maxTilesPerTurn = 4;
    public int tilesMoved = 0;

    private void SetAnimationState(PlayerMoveState state)
    {
        if (animator == null) return;
        playerData.playerMoveState = state;

        // Reset first
        animator.SetBool("Walking", false);

        switch (state)
        {
            case PlayerMoveState.Stand:
                animator.SetBool("Walking", false);
                break;

            case PlayerMoveState.Walk:
                animator.SetBool("Walking", true);
                break;
        }
    }

    public IEnumerator MoveToCell(GameObject To)
    {
        if(playerAttack.hasAttackedThisTurn && TurnManager.Instance.isInTurnSystem)
        {
            Debug.Log("Can't move after attacking.");
            yield break;
        }
        if (tilesMoved >= 16 && TurnManager.Instance.isInTurnSystem) alreadyWalk = true;
        else if(!TurnManager.Instance.isInTurnSystem){ tilesMoved = 0; }
        if ((transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        || tilesMoved >= maxTilesPerTurn)
        {
            SetAnimationState(PlayerMoveState.Stand);
            Room room = To.GetComponentInParent<Room>();
            if (room.roomState == RoomState.CanExit)
            {
                BoardManager.Instance.CheckPlayerOnExit(To);
            }
            BoardManager.Instance.isTransferring = false;
            yield break;
        }

        Vector3 targetPos = transform.position;

        if (Mathf.Abs(To.transform.position.x - transform.position.x) > 0.01f)
        {
            SetAnimationState(PlayerMoveState.Walk);
            int x = (To.transform.position.x > transform.position.x) ? 1 : -1;
            targetPos = transform.position + new Vector3(x, 0, 0);

            Vector3 dir = new Vector3(x, 0, 0);
            transform.rotation = Quaternion.LookRotation(dir);
            // ✅ Move smoothly
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }
        else if (Mathf.Abs(To.transform.position.z - transform.position.z) > 0.01f)
        {
            SetAnimationState(PlayerMoveState.Walk);
            int z = (To.transform.position.z > transform.position.z) ? 1 : -1;
            targetPos = transform.position + new Vector3(0, 0, z);

            Vector3 dir = new Vector3(0, 0, z);
            transform.rotation = Quaternion.LookRotation(dir);

            // ✅ Move smoothly
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }

        // 🛑 Enemy blocking?
        Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 2f, Enemy);
        if (hitEnemies.Length > 0)
        {
            Debug.Log("Enemy blocking path → stop movement.");
            SetAnimationState(PlayerMoveState.Stand);
            yield break;
        }

        Marker newMarker = To.GetComponent<Marker>();

        if (newMarker != null)
        {
            // 🛑 IMPORTANT: If the new marker is already occupied (e.g., by another Player/Hazard not caught by your Physics.OverlapSphere)
            // You might want to stop here, but since your existing logic only prevents enemy block, we proceed, 
            // relying on the movement system to prevent collisions.

            // This is tricky without explicit tracking of the *previous* marker. 
            // A better long-term solution is adding: 'public Marker currentOccupiedMarker;' to PlayerController.

            // --- TEMPORARY SOLUTION: Check/Clear/Set Marker state based on position ---

            // Find the marker at the old position and set it to unoccupied
            Vector2 oldGridPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
            GameObject oldMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(oldGridPos, To.transform.parent.gameObject);
            if (oldMarkerGO != null)
            {
                oldMarkerGO.GetComponent<Marker>().IsOccupied = false;
            }

            // Set the marker at the new position to occupied
            newMarker.IsOccupied = true;
        }

        // ✅ Count tile
        tilesMoved++;

        // Update grid position (directly based on difference)
        playerData.GridPosition.x = Mathf.RoundToInt(transform.position.x);
        playerData.GridPosition.y = Mathf.RoundToInt(transform.position.z);

        if (tilesMoved >= maxTilesPerTurn ||
        Vector3.Distance(transform.position, To.transform.position) < 0.01f)
        {
            Debug.Log("Used all moves or reached destination → switching turn.");
            SetAnimationState(PlayerMoveState.Stand);
        }
        else
        {
            // Continue toward destination if not final cell
            StartCoroutine(MoveToCell(To));
        }
    }

    public IEnumerator TakeTurn()
    {
        MainCamera.Instance.SetPlayer(gameObject.transform);
        PlayerOptionHUDController.Show();

        bool turnEnded = false;
        if (alreadyWalk || alreadyAttack)
        {
            turnEnded = true;
        }

        while (!turnEnded)
        {
            yield return null;
            if (alreadyWalk || alreadyAttack)
            {
                turnEnded = true;
            }
        }

        FinishedTurn();
        PlayerOptionHUDController.Hide();
    }

    public void FinishedTurn()
    {
        // Reset movement for next turn
        alreadyWalk = false;
        alreadyAttack = false;
        playerAttack.ResetAttackStatus();
        playerData.playerMoveState = PlayerMoveState.Stand;
        SetAnimationState(PlayerMoveState.Stand);
        tilesMoved = 0;
    }

    public void SkipTurn()
    {
        playerAttack.ResetAttackStatus();
        alreadyAttack = true;
        alreadyWalk = true;
        tilesMoved = 0;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0)
            && playerData.playerMoveState == PlayerMoveState.Stand
            && !EventSystem.current.IsPointerOverGameObject()
            && !alreadyWalk)
        {
            if (TurnManager.Instance == null || TurnManager.Instance.currentPhase != Phase.Player) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
            {
                GameObject clickedTile = hit.collider.gameObject;
                Vector3 tilePosition = clickedTile.transform.position;

                // 🛑 NEW CHECK: Check for enemies at the clicked tile's position
                // Use a small radius (e.g., 0.1f) to check if an enemy is precisely on the tile.
                Collider[] enemiesOnTile = Physics.OverlapSphere(tilePosition, 1f, Enemy);

                if (enemiesOnTile.Length > 0)
                {
                    // Enemy is on the tile, so prevent movement
                    Debug.Log("Cannot move to this tile. An enemy is there.");
                    return; // Stop execution, don't start the MoveToCell coroutine
                }

                // If no enemy is found, proceed with movement
                StartCoroutine(MoveToCell(clickedTile));
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            playerData.Healing(20);
        }
    }
}
