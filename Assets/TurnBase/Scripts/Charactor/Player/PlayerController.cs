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

    public IEnumerator MoveToCell(GameObject To)
    {
        if (transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        {
            playerData.playerMoveState = PlayerMoveState.Stand;
            Room room = To.GetComponentInParent<Room>();
            if (room.roomState == RoomState.CanExit)
            {
                BoardManager.Instance.CheckPlayerOnExit(To);
            }
            BoardManager.Instance.isTransferring = false;
            BoardManager.Instance.CheckPlayerInWhatRoom();
            yield break;
        }

        Vector3 targetPos = transform.position;

        if (transform.position.x != To.transform.position.x)
        {
            playerData.playerMoveState = PlayerMoveState.Walk;
            int x = (To.transform.position.x > transform.position.x) ? 1 : -1;
            targetPos = transform.position + new Vector3(x, 0, 0);
        }
        else if (transform.position.z != To.transform.position.z)
        {
            playerData.playerMoveState = PlayerMoveState.Walk;
            int z = (To.transform.position.z > transform.position.z) ? 1 : -1;
            targetPos = transform.position + new Vector3(0, 0, z);
        }

        // 🛑 Check if targetPos has an enemy
        Collider[] hitEnemies = Physics.OverlapSphere(targetPos, 0.4f, Enemy);
        if (hitEnemies.Length > 0)
        {
            Debug.Log("Enemy blocking path → stop movement.");
            playerData.playerMoveState = PlayerMoveState.Stand;
            yield break; // stop moving if enemy is there
        }

        // ✅ Move toward target cell
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        // Update grid position
        if (transform.position.x != To.transform.position.x)
            playerData.GridPosition.x += (targetPos.x > transform.position.x) ? 1 : -1;
        else if (transform.position.z != To.transform.position.z)
            playerData.GridPosition.y += (targetPos.z > transform.position.z) ? 1 : -1;

        // Continue toward target
        StartCoroutine(MoveToCell(To));
    }
    public IEnumerator SwitchTurn()
    {
        yield return new WaitForSeconds(1f);
        TurnManager.Instance.EndTurn();
    }

    void Start()
    {
        maxHealth = playerData.HP;
        playerData.currentHP = maxHealth;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && playerData.playerMoveState == PlayerMoveState.Stand && !EventSystem.current.IsPointerOverGameObject())
            // Only allow input during Player phase
            if (TurnManager.Instance == null || TurnManager.Instance.currentPhase != Phase.Player) return;


        if (Input.GetMouseButtonDown(0) && playerData.playerMoveState == PlayerMoveState.Stand && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
            {
                StartCoroutine(MoveToCell(hit.collider.gameObject));
                StartCoroutine(SwitchTurn());
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            playerData.Healing(20);
            StartCoroutine(SwitchTurn());
        }
    }
}

