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

    public IEnumerator MoveToCell2(GameObject To)
    {
        if ((transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        || tilesMoved >= maxTilesPerTurn)
        {
            SetAnimationState(PlayerMoveState.Stand);
            // StartCoroutine(SwitchTurn());
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
            Debug.Log(tilesMoved);
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
        Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.4f, Enemy);
        if (hitEnemies.Length > 0)
        {
            Debug.Log("Enemy blocking path → stop movement.");
            SetAnimationState(PlayerMoveState.Stand);
            yield break;
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
            StartCoroutine(SwitchTurn());
        }
        else
        {
            // Continue toward destination if not final cell
            StartCoroutine(MoveToCell2(To));
        }

    }


    public IEnumerator MoveToCell(GameObject To)
    {
        if ((transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        || tilesMoved >= maxTilesPerTurn)
        {
            SetAnimationState(PlayerMoveState.Stand);
            StartCoroutine(SwitchTurn());
            Room room = To.GetComponentInParent<Room>();
            if (room.roomState == RoomState.CanExit)
            {
                BoardManager.Instance.CheckPlayerOnExit(To);
            }
            BoardManager.Instance.isTransferring = false;
            yield break;
        }

        if (transform.position.x != To.transform.position.x)
        {
            Debug.Log($" Player : {tilesMoved})");
            playerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(transform.position.x - To.transform.position.x, 0, 0).normalized;
            int x = Dir.x > 0 ? x = -1 : x = 1;
            Vector3 targetPos = transform.position + new Vector3(x, 0, 0);
            float speed = 5f;
            SetAnimationState(PlayerMoveState.Walk);
            transform.rotation = Quaternion.LookRotation(Dir * -1);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            playerData.GridPosition.y -= x;

            Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.4f, Enemy);
            if (hitEnemies.Length > 0)
            {
                Debug.Log("Enemy blocking path → stop movement.");
                SetAnimationState(PlayerMoveState.Stand);
                yield break;
            }

            tilesMoved++;

            StartCoroutine(MoveToCell(To));
        }
        else if (transform.position.z != To.transform.position.z)
        {
            playerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(0, 0, transform.position.z - To.transform.position.z).normalized;
            int z = Dir.z > 0 ? z = -1 : z = 1;
            Vector3 targetPos = transform.position + new Vector3(0, 0, z);
            float speed = 5f;
            SetAnimationState(PlayerMoveState.Walk);
            transform.rotation = Quaternion.LookRotation(Dir * -1);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            playerData.GridPosition.x -= z;
            Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.4f, Enemy);
            if (hitEnemies.Length > 0)
            {
                Debug.Log("Enemy blocking path → stop movement.");
                SetAnimationState(PlayerMoveState.Stand);
                yield break;
            }

            tilesMoved++;

            StartCoroutine(MoveToCell(To));
        }
        
        if ((transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        || tilesMoved >= maxTilesPerTurn)
        {
            SetAnimationState(PlayerMoveState.Stand);
            StartCoroutine(SwitchTurn());
            Room room = To.GetComponentInParent<Room>();
            if (room.roomState == RoomState.CanExit)
            {
                BoardManager.Instance.CheckPlayerOnExit(To);
            }
            BoardManager.Instance.isTransferring = false;
            yield break;
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
