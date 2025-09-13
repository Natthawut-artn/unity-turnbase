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


        if (transform.position.x != To.transform.position.x)
        {
            playerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(transform.position.x - To.transform.position.x, 0, 0).normalized;
            int x = Dir.x > 0 ? -1 : 1;
            Vector3 targetPos = transform.position + new Vector3(x, 0, 0);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            playerData.GridPosition.x -= x;
            StartCoroutine(MoveToCell(To));
        }
        else if (transform.position.z != To.transform.position.z)
        {
            playerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(0, 0, transform.position.z - To.transform.position.z).normalized;
            int z = Dir.z > 0 ? -1 : 1;
            Vector3 targetPos = transform.position + new Vector3(0, 0, z);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, playerData.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            playerData.GridPosition.y -= z;
            StartCoroutine(MoveToCell(To));
        }
    }
    public IEnumerator SwitchTurn()
    {
        yield return new WaitForSeconds(1f);
        TurnManager.Instance.currentPhase = Phase.Enemy;
        TurnManager.Instance.StartCoroutine(TurnManager.Instance.EnemyPhase());
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
    }
}
