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
    public Vector2 EnemySpawnPoint;
    private const string Enemy_Path = "Prefabs/Charactor/Enemy/Enemy";

    IEnumerator SpawnPlayer()
    {
        yield return null;
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlayerSpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainHero = Instantiate(Resources.Load<GameObject>(Player_Path), Pos, Quaternion.identity);
        PlayerData.GridPosition = PlayerSpawnPoint;

        MainCamera.Instance.SetPlayer(MainHero);

        BoardManager.Instance.CheckPlayerInWhatRoom();
    }

    IEnumerator SpawnEnemy()
    {
        yield return null;
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(EnemySpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainEnemy = Instantiate(Resources.Load<GameObject>(Enemy_Path), Pos, Quaternion.identity);
        PlayerData.GridPosition = EnemySpawnPoint;

        
    }

    void Start()
    {
        StartCoroutine(SpawnPlayer());
        // StartCoroutine(SpawnEnemy());
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
