using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    void Awake()
    {
        Instance = this;
    }
    public LayerMask MarkerLayer;

    private IEnumerator MoveToCell(GameObject To)
    {
        if (transform.position.x == To.transform.position.x && transform.position.z == To.transform.position.z)
        {
            PlayerData.playerMoveState = PlayerMoveState.Stand;
            yield break;
        }

        if (transform.position.x != To.transform.position.x)
        {
            PlayerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(transform.position.x - To.transform.position.x, 0, 0).normalized;
            int x = Dir.x > 0 ? x = -1 : x = 1;
            Vector3 pos = transform.position;
            pos.x += x;
            transform.position = pos;
            PlayerData.GridPosition.y -= x;

            yield return new WaitForSeconds(0.25f);
            StartCoroutine(MoveToCell(To));
        }
        else if (transform.position.z != To.transform.position.z)
        {
            PlayerData.playerMoveState = PlayerMoveState.Walk;
            Vector3 Dir = new Vector3(0, 0, transform.position.z - To.transform.position.z).normalized;
            int z = Dir.z > 0 ? z = -1 : z = 1;
            Vector3 pos = transform.position;
            pos.z += z;
            transform.position = pos;
            PlayerData.GridPosition.x -= z;

            yield return new WaitForSeconds(0.25f);
            StartCoroutine(MoveToCell(To));
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && PlayerData.playerMoveState == PlayerMoveState.Stand)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer))
            {
                StartCoroutine(MoveToCell(hit.collider.gameObject));
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log($"Player Grid Position : {PlayerData.GridPosition}");    
        }
    }
}
