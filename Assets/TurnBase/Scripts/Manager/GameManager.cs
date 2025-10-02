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

    public GameObject Player;
    public GameObject CurrentStage;
    public LayerMask MarkerLayer;
    public Vector2 PlayerSpawnPoint;
    public string Room;
    private const string Player_Path = "Prefabs/Charactor/Player/Striker";
    private const string Avatar_Path = "Prefabs/Charactor/Player/Avatar";
    public Vector2 EnemySpawnPoint;
    private const string Enemy_Path = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";

    public void StartGame()
    {
        if (CurrentStage != null)
        {
            StopGame();
        }

        if (AppStateManager.appState == AppState.Hub)
        {
            CurrentStage = Instantiate(Resources.Load<GameObject>("Prefabs/Stages/Hub"));
        }
        else
        {
            CurrentStage = Instantiate(Resources.Load<GameObject>("Prefabs/Stages/Sci-fi/Sci-fi 1"));
        }

        if (CurrentStage != null)
        {
            SpawnPlayer();
        }
    }

    public void StopGame()
    {
        Destroy(Player);
        Destroy(CurrentStage);
    }

    public void SpawnPlayer()
    {
        if (AppStateManager.appState == AppState.Hub)
        {
            Player = Instantiate(Resources.Load<GameObject>(Avatar_Path));
        }
        else
        {
            Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlayerSpawnPoint, BoardManager.Instance.GetRoom(Room));
            Pos += new Vector3(0f, 1.49f, 0f);

            Player = Instantiate(Resources.Load<GameObject>(Player_Path), Pos, Quaternion.identity);
            PlayerData playerData = Player.GetComponent<PlayerData>();
            playerData.GridPosition = PlayerSpawnPoint;

            StartCoroutine(Wait());
        }
        MainCamera.Instance.SetPlayer(Player.transform);
    }

    IEnumerator Wait()
    {
        yield return null;
        HPBarHUDController.Show();
    }

    public void SpawnEnemy()
    {
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(EnemySpawnPoint, BoardManager.Instance.GetRoom(Room));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainEnemy = Instantiate(Resources.Load<GameObject>(Enemy_Path), Pos, Quaternion.identity);
        EnemyData enemyData = MainEnemy.GetComponent<EnemyData>();
        enemyData.GridPosition = EnemySpawnPoint;
    }

    public void BackToTheHub()
    {
        HPBarHUDController.Hide();
        StopGame();
        AppStateManager.ChangeAppState(AppState.Hub);
        StartGame();
    }

    void Start()
    {
        StartGame();
        SettingHUDController.Show();
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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnEnemy();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($"Now AppState is {AppStateManager.appState}");
        }

        if (Input.GetKeyDown(KeyCode.E) && AppStateManager.appState == AppState.State)
        {
            BackToTheHub();    
        }
    }
}
