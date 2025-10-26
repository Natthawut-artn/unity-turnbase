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
    }
    private Marker currentOccupiedMarker;

    public LayerMask MarkerLayer;
    public PlayerData playerData;
    public PlayerAttack playerAttack;
    public LayerMask Enemy;
    public bool alreadyAttack = false;
    public bool alreadyWalk = false;
    private Animator animator;

    // ✅ Movement limit
    public int maxTilesPerTurn = 4;
    public int tilesMoved = 0;

    private Marker GetMarkerAtCurrentPosition()
    {
        Vector2 gridPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));

        // Assuming the player is always in a room that is a parent or sibling of the BoardManager's rooms.
        // We need the Room GameObject to search for markers. If the player is not currently in a room, this will fail.
        Room playerRoom = GetComponentInParent<Room>();
        if (playerRoom == null) return null;

        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, playerRoom.gameObject);
        if (markerGO != null)
        {
            return markerGO.GetComponent<Marker>();
        }
        return null;
    }

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
        if (playerAttack.hasAttackedThisTurn && TurnManager.Instance.isInTurnSystem)
        {
            Debug.Log("Can't move after attacking.");
            yield break;
        }
        if (tilesMoved >= maxTilesPerTurn && TurnManager.Instance.isInTurnSystem) alreadyWalk = true;
        else if (!TurnManager.Instance.isInTurnSystem) { tilesMoved = 0; }
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
        if (currentOccupiedMarker != null)
        {
            currentOccupiedMarker.IsOccupied = false;
            currentOccupiedMarker = null; // Clear reference until new one is acquired
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
        Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.5f, Enemy);
        if (hitEnemies.Length > 0)
        {
            Debug.Log("Enemy blocking path → stop movement.");
            SetAnimationState(PlayerMoveState.Stand);
            currentOccupiedMarker = GetMarkerAtCurrentPosition();
            if (currentOccupiedMarker != null) currentOccupiedMarker.IsOccupied = true;

            yield break;
        }

        Marker newMarker = To.GetComponent<Marker>();

        if (newMarker != null)
        {
            // Find the marker at the old position and set it to unoccupied
            Vector2 oldGridPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
            // Assuming To.transform.parent.gameObject provides the room object correctly
            GameObject oldMarkerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(oldGridPos, To.transform.parent.gameObject);
            if (oldMarkerGO != null)
            {
                oldMarkerGO.GetComponent<Marker>().IsOccupied = false;
            }

            // Set the marker at the new position to occupied
            newMarker.IsOccupied = true;
        }

        currentOccupiedMarker = GetMarkerAtCurrentPosition();
        if (currentOccupiedMarker != null)
        {
            currentOccupiedMarker.IsOccupied = true;
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
        ServiceLocator.Get<SkillPointsHUD>().Show();
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
                // Assuming ResetBuffForTurn() is needed here
                // playerData.ResetBuffForTurn(); 
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
        currentOccupiedMarker = GetMarkerAtCurrentPosition();
        if (currentOccupiedMarker != null) currentOccupiedMarker.IsOccupied = true;
    }


    /// <summary>
    /// Handles both Movement and Attack Targeting based on the PlayerAttack state.
    /// </summary>
    void Update()
    {
        // Existing H key logic...
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerData.Healing(20);
        }

        // --- LEFT-CLICK (Both Target Confirmation & Movement) Logic ---
        if (Input.GetMouseButtonDown(0) // LEFT CLICK
            && !EventSystem.current.IsPointerOverGameObject()
            && TurnManager.Instance != null
            && TurnManager.Instance.currentPhase == Phase.Player)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Priority 1: Check if in Targeting Mode (Confirmed by PlayerAttack script)
            if (playerAttack.isTargetingMode)
            {
                // Raycast specifically for an enemy (using the Enemy layer)
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, Enemy))
                {
                    // Call the PlayerAttack script to attempt the attack confirmation
                    playerAttack.ConfirmAttack(hit.transform);
                    // NOTE: ConfirmAttack handles cleanup, so we STOP here.
                }
                else
                {
                    // Clicked outside the enemy in targeting mode, cancel targeting
                    Debug.Log("Click missed target. Exiting targeting mode.");
                    playerAttack.ToggleTargetingMode(); // Toggle off
                }
                return;
            }

            // Priority 2: Handle Movement (only if not targeting)
            if (!alreadyWalk
                && playerData.playerMoveState == PlayerMoveState.Stand)
            {
                // Raycast specifically for a movement Marker
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
                {
                    GameObject clickedTile = hit.collider.gameObject;
                    Vector3 tilePosition = clickedTile.transform.position;

                    // Check for enemies on the tile before moving
                    Collider[] enemiesOnTile = Physics.OverlapSphere(tilePosition, 0.5f, Enemy);
                    if (enemiesOnTile.Length > 0)
                    {
                        Debug.Log("Cannot move to this tile. An enemy is there.");
                        return;
                    }

                    // If no enemy is found, proceed with movement
                    StartCoroutine(MoveToCell(clickedTile));
                }
            }
        }
    }
}
