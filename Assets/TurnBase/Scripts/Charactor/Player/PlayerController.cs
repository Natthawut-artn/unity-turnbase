using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharactorInheritance
{
    public static PlayerController Instance;
    void Awake() { Instance = this; }


    public LayerMask MarkerLayer;

    public IEnumerator MoveToCell(GameObject To)
    {
        if (transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        {
            PlayerData.Instance.playerMoveState = PlayerMoveState.Stand;
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
            PlayerData.Instance.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(transform.position.x - To.transform.position.x, 0, 0).normalized;
            int x = Dir.x > 0 ? -1 : 1;
            Vector3 targetPos = transform.position + new Vector3(x, 0, 0);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, PlayerData.Instance.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            PlayerData.Instance.GridPosition.x -= x;
            StartCoroutine(MoveToCell(To));
        }
        else if (transform.position.z != To.transform.position.z)
        {
            PlayerData.Instance.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(0, 0, transform.position.z - To.transform.position.z).normalized;
            int z = Dir.z > 0 ? -1 : 1;
            Vector3 targetPos = transform.position + new Vector3(0, 0, z);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, PlayerData.Instance.moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
            PlayerData.Instance.GridPosition.y -= z;
            StartCoroutine(MoveToCell(To));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerData.Instance.playerMoveState == PlayerMoveState.Stand)
        // Only allow input during Player phase
        if (TurnManager.Instance == null || TurnManager.Instance.currentPhase != Phase.Player) return;


        if (Input.GetMouseButtonDown(0) && PlayerData.Instance.playerMoveState == PlayerMoveState.Stand)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
            {
                StartCoroutine(MoveToCell(hit.collider.gameObject));
            }
        }
    }
}
