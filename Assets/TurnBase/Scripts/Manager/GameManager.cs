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
    public List<Vector2> PlaterSpawnPos = new List<Vector2>();
    private List<string> Players_Path = new List<string>()
    {
        "Prefabs/Charactor/Player/Striker Variant",
        "Prefabs/Charactor/Player/Protector",
        "Prefabs/Charactor/Player/Support"
    };
    public List<GameObject> Players_alive = new List<GameObject>();

    public string Room;
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
            SpawnPlayers();
            StartCoroutine(TurnManager.Instance.PlayersPhase());
        }
    }

    public void StopGame()
    {
        DestroyAll();
        Destroy(CurrentStage);
    }

    public void SpawnPlayers()
    {
        if (AppStateManager.appState == AppState.Hub)
        {
            Player = Instantiate(Resources.Load<GameObject>(Avatar_Path));
            MainCamera.Instance.SetPlayer(Player.transform);
            return;
        }
        else
        {
            Destroy(Player);
        }

        for (int i = 0; i < Players_Path.Count; i++)
        {
            Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlaterSpawnPos[i], BoardManager.Instance.GetRoom(Room));
            Pos += new Vector3(0f, 1.49f, 0f);

            GameObject Player = Instantiate(Resources.Load<GameObject>(Players_Path[i]), Pos, Quaternion.identity);
            PlayerData playerData = Player.GetComponent<PlayerData>();
            playerData.GridPosition = PlaterSpawnPos[i];

            Players_alive.Add(Player);

            if (i == 0)
            {
                MainCamera.Instance.SetPlayer(Player.transform);
            }
        }

    }

    public void DestroyAll()
    {
        foreach (GameObject player in Players_alive)
        {
            Destroy(player);
        }
    }

    IEnumerator Wait()
    {
        yield return null;
        // HPBarHUDController.Show();
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
        // HPBarHUDController.Hide();
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
                    BoardManager.Instance.OpenAllDoors(roomScript.gameObject);
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
