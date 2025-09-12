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

    public GameObject CurrentStage;
    public LayerMask MarkerLayer;
    public Vector2 PlayerSpawnPoint;
    public string Room;
    private const string Player_Path = "Prefabs/Charactor/Player/Striker";
    public Vector2 EnemySpawnPoint;
    private const string Enemy_Path = "Prefabs/Charactor/Enemy/Enemy";

    public void StartGame()
    {
        CurrentStage = Instantiate(Resources.Load<GameObject>("Prefabs/Stages/Sci-fi/Sci-fi 1"));

        if (CurrentStage != null)
        {
            SpawnPlayer();
        }
    }

    public void StopGame()
    {
        Destroy(CurrentStage);
    }

    public void SpawnPlayer()
    {
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlayerSpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainHero = Instantiate(Resources.Load<GameObject>(Player_Path), Pos, Quaternion.identity);
        PlayerData.Instance.GridPosition = PlayerSpawnPoint;

        MainCamera.Instance.SetPlayer(MainHero);

        BoardManager.Instance.CheckPlayerInWhatRoom();
    }

    public void SpawnEnemy()
    {
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(EnemySpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainEnemy = Instantiate(Resources.Load<GameObject>(Enemy_Path), Pos, Quaternion.identity);
        PlayerData.Instance.GridPosition = EnemySpawnPoint;
    }

    void Start()
    {
        StartGame();
        SpawnEnemy();  
        SwiftTurnHUDController.Show();
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
            Debug.Log($"Player Grid Position : {PlayerData.Instance.GridPosition}");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemy();    
        }
    }
}
