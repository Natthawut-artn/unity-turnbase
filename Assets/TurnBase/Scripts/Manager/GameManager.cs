using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        "Prefabs/Charactor/Player/Striker",
        "Prefabs/Charactor/Player/Protector",
        "Prefabs/Charactor/Player/Support"
    };
    private List<string> relicDatas_Path = new List<string>()
    {
        "RelicsData/RevenCrew"
    };
    public List<GameObject> Players_alive = new List<GameObject>();
    public List<GameObject> Enemys_alive = new List<GameObject>();

    public string Room;
    private const string Avatar_Path = "Prefabs/Charactor/Player/Avatar1";
    public Vector2 EnemySpawnPoint;
    private const string Enemy_Path = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public RelicData relic;

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

        for (int i = 0; i < Players_Path.Count - 2; i++)
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
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return null;
        HPBarsHUDController.Show();
        PlayersInfoHUDController.Show();
        PocketHUDController.Show();
    }

    public void DestroyAll()
    {
        foreach (GameObject player in Players_alive)
        {
            Destroy(player);
        }
        Players_alive.Clear();
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
        HPBarsHUDController.Hide();
        PlayerOptionHUDController.Hide();
        PlayersInfoHUDController.Hide();
        PocketHUDController.Hide();
        RelicPocketPopupController.Hide();
        PlayersInfoHUDController.Hide();
        StopGame();
        AppStateManager.ChangeAppState(AppState.Hub);
        StartGame();
    }

    public void CheckEnemys()
    {
        if (!Enemys_alive.Any())
        {
            TurnManager.Instance.StopTurnBaseSystem();
        }
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
            TurnManager.Instance.EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($"Now AppState is {AppStateManager.appState}");
        }

        if (Input.GetKeyDown(KeyCode.E) && AppStateManager.appState == AppState.State)
        {
            BackToTheHub();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Install relic");
            foreach (GameObject player in Players_alive)
            {
                RelicsStoage relicsStoage = player.GetComponent<RelicsStoage>();
                relicsStoage.InstallRelic(relic);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Remove relic");
            foreach (GameObject player in Players_alive)
            {
                RelicsStoage relicsStoage = player.GetComponent<RelicsStoage>();
                relicsStoage.RemoveRelic(relic);
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Get Relic");
            foreach (string relic in relicDatas_Path)
            {
                RelicData relicData = Resources.Load<RelicData>(relic);
                RelicStroageManager.RelicsInUserPocket.Add(relicData);
                RelicPocketPopupController.Update();
            }
        }
    }
}
