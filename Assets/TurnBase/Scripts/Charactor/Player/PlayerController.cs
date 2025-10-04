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
    }

    public LayerMask MarkerLayer;
    public PlayerData playerData;
    public int maxHealth;
    public int currentHealth;
    public LayerMask Enemy;
    private Animator animator;

    // ✅ Movement limit
    public int maxTilesPerTurn = 4;
    public int tilesMoved = 0;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        maxHealth = playerData.HP;
        playerData.currentHP = maxHealth;
    }
    private void SetAnimationState(PlayerMoveState state)
    {
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
        // 🛑 Stop if reached max movement
        if (tilesMoved >= maxTilesPerTurn)
        {
            Debug.Log("Movement limit reached! Auto ending turn...");
            StartCoroutine(SwitchTurn());
            yield break;
        }

        // Already at destination
        if (transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
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

        // Decide step direction
        if (Mathf.Abs(To.transform.position.x - transform.position.x) > 0.01f)
        {
            SetAnimationState(PlayerMoveState.Walk);
            int x = (To.transform.position.x > transform.position.x) ? 1 : -1;
            targetPos = transform.position + new Vector3(x, 0, 0);

            Vector3 dir = new Vector3(x, 0, 0);
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else if (Mathf.Abs(To.transform.position.z - transform.position.z) > 0.01f)
        {
            SetAnimationState(PlayerMoveState.Walk);
            int z = (To.transform.position.z > transform.position.z) ? 1 : -1;
            targetPos = transform.position + new Vector3(0, 0, z);

            Vector3 dir = new Vector3(0, 0, z);
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 🛑 Enemy blocking?
        Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.4f, Enemy);
        if (hitEnemies.Length > 0)
        {
            Debug.Log("Enemy blocking path → stop movement.");
            SetAnimationState(PlayerMoveState.Stand);
            yield break;
        }

        // ✅ Move smoothly
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        // ✅ Count tile
        tilesMoved++;

        // Update grid position (directly based on difference)
        playerData.GridPosition.x = Mathf.RoundToInt(transform.position.x);
        playerData.GridPosition.y = Mathf.RoundToInt(transform.position.z);

        // ✅ Auto-end turn if movement used up
        if (tilesMoved >= maxTilesPerTurn || (transform.position == To.transform.position))
        {
            Debug.Log("Used all moves or reached destination → switching turn.");
            SetAnimationState(PlayerMoveState.Stand);
            StartCoroutine(SwitchTurn());
        }
        else
        {
            // Continue toward destination if not final cell
            StartCoroutine(MoveToCell(To));
        }
    }

    public IEnumerator SwitchTurn()
    {
        yield return new WaitForSeconds(0.5f);

        // Reset movement for next turn
        tilesMoved = 0;

        playerData.playerMoveState = PlayerMoveState.Stand;
        SetAnimationState(PlayerMoveState.Stand);


        // Switch to Enemy phase
        TurnManager.Instance.EndTurn();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && playerData.playerMoveState == PlayerMoveState.Stand && !EventSystem.current.IsPointerOverGameObject())
        {
            if (TurnManager.Instance == null || TurnManager.Instance.currentPhase != Phase.Player) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
            {
                StartCoroutine(MoveToCell(hit.collider.gameObject));
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            playerData.Healing(20);
            StartCoroutine(SwitchTurn());
        }
    }
}
