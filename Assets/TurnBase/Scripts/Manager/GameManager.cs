using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public LayerMask MarkerLayer;
    public Vector2 PlayerSpawnPoint;
    public string Room;
    private const string Player_Path = "Prefabs/Charactor/Player/Player";

    IEnumerator SpawnPlayer()
    {
        yield return null;
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlayerSpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainHero = Instantiate(Resources.Load<GameObject>(Player_Path), Pos, Quaternion.identity);
        PlayerData.GridPosition = PlayerSpawnPoint;

        BoardManager.Instance.CheckPlayerInWhatRoom();
    }

    void Start()
    {
        StartCoroutine(SpawnPlayer());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (GameObject room in BoardManager.Instance.ListRooms)
            {
                Room roomScript = room.GetComponent<Room>();
                if (roomScript.roomState == RoomState.Playing)
                {
                    roomScript.roomState = RoomState.CanExit;
                    Debug.Log($"Now {roomScript.gameObject.name} is {roomScript.roomState}");
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log($"Player Grid Position : {PlayerData.GridPosition}");    
        }
    }
}
