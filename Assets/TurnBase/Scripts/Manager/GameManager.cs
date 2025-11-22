using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System.IO;

public enum Theme
{
    Scifi,
    Medieval,
    Victoria
}

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
    
    public static bool IsBossSwapping = false;
    public GameObject Player;
    public GameObject CurrentStage;
    public LayerMask MarkerLayer;
    public LayerMask PlayerLayer;
    public LayerMask EnemyLayer;
    public List<Vector2> PlaterSpawnPos = new List<Vector2>();
    private List<string> Players_Path = new List<string>()
    {
        "Prefabs/Charactor/Player/Striker",
        "Prefabs/Charactor/Player/Support",
        "Prefabs/Charactor/Player/Protector"
    };
    private List<string> relicDatas_Path = new List<string>()
    {
        "RelicsData/RevenCrew"
    };
    public List<GameObject> Players_alive = new List<GameObject>();
    public List<GameObject> Enemys_alive = new List<GameObject>();
    public GameObject lstPlayerDie = null;

    public string CurrentRoom;
    private const string Avatar_Path = "Prefabs/Charactor/Player/Avatar1";
    public Vector2 EnemySpawnPoint;
    private const string Enemy_Path = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public RelicData relic;
    private List<GameObject> Friends = new List<GameObject>();
    private const string BGManager_Path = "Prefabs/Board/SFXManager";
    private const string VFXManager_Path = "Prefabs/Board/VFXManager";
    private Coroutine StartGameCoroutine;

    // Base Def Debuff status
    public int Venom = 100;
    public int Fire = 100;
    public int BloodFlowing = 140;
    public int Frost = 100;
    public int Death = 200;


    public void StartGame(Theme theme = Theme.Scifi)
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
            string Path = "Prefabs/Stages";
            if (theme == Theme.Scifi)
            {
                Path += "/Sci-fi/Sci-fi ";
            }
            else if (theme == Theme.Medieval)
            {
                Path += "/Medieval/Medieval ";
            }
            else if (theme == Theme.Victoria)
            {
                Path += "/Victoria/Victoria ";
            }
            int random = Random.Range( 1 , 3);
            Path += random.ToString();
            CurrentStage = Instantiate(Resources.Load<GameObject>(Path));
        }

        if (CurrentStage != null)
        {
            SpawnPlayers();
            if (AppStateManager.appState == AppState.State)
            {
                if (StartGameCoroutine == null)
                {
                    StartGameCoroutine = StartCoroutine(StartTheGame());
                }
                SFXManager.Instance.Play(SFX.Stages);
                SkillPointsManager.ReceiveSkillPoint(3);
            }
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
            Friends.Clear();
        }

        for (int i = 0; i < Players_Path.Count; i++)
        {
            Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(PlaterSpawnPos[i], BoardManager.Instance.GetRoom(CurrentRoom));
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
        TurnManager.Instance.StopTurnBaseSystem();
    }

    IEnumerator StartTheGame()
    {
        yield return new WaitForSeconds(3f);
        ReleasePlayers();
    }

    IEnumerator Wait()
    {
        yield return null;
        HPBarsHUDController.Show();
        PlayersInfoHUDController.Show();
        PocketHUDController.Show();
        GameObject MainPlayer = Players_alive[0];
        GameObject LeftPos = MainPlayer.transform.Find("Left").gameObject;
        GameObject RightPos = MainPlayer.transform.Find("Right").gameObject;
        Friends.Add(LeftPos);
        Friends.Add(RightPos);
        BrokenRelicManager.InitInStage();
    }

    public void DestroyAll()
    {
        foreach (GameObject player in Players_alive)
        {
            Destroy(player);
        }
        Players_alive.Clear();
        foreach (GameObject enemy in Enemys_alive)
        {
            Destroy(enemy);
        }
        Enemys_alive.Clear();
    }

    public void SpawnEnemy()
    {
        Vector3 Pos = BoardManager.Instance.GetMarkerWorldPosition(EnemySpawnPoint, BoardManager.Instance.GetRoom(CurrentRoom));
        Pos += new Vector3(0f, 1.49f, 0f);

        GameObject MainEnemy = Instantiate(Resources.Load<GameObject>(Enemy_Path), Pos, Quaternion.identity);
        EnemyData enemyData = MainEnemy.GetComponent<EnemyData>();
        enemyData.GridPosition = EnemySpawnPoint;
    }

    public void BackToTheHub()
    {
        TurnManager.Instance.StopTurnBaseSystem();

        HPBarsHUDController.Hide();
        PlayerOptionHUDController.Hide();
        PlayersInfoHUDController.Hide();
        PlayersInfoPopupController.Hide();
        PocketHUDController.Hide();
        RelicPocketPopupController.Hide();
        PlayersInfoHUDController.Hide();
        ServiceLocator.Get<RelicInstPopup>().Hide();
        ServiceLocator.Get<RelicUnInstPopup>().Hide();
        ServiceLocator.Get<SkillPointsHUD>().Hide();
        ServiceLocator.Get<TurnHUD>().Hide();

        if (StartGameCoroutine != null)
        {
            StopCoroutine(StartGameCoroutine);
            StartGameCoroutine = null;
        }

        StopGame();
        AppStateManager.ChangeAppState(AppState.Hub);
        StartGame();
        SFXManager.Instance.Play(SFX.Hub);
        SkillPointsManager.ResetSkillPonit();
        RelicStroageManager.ResetRelics();
        PocketHUDController.Show();
    }

    public void CheckEnemys()
    {
        if (!Enemys_alive.Any() && CurrentRoom == "Room9")
        {
            TurnManager.Instance.StopTurnBaseSystem();
        }
        else if (!Enemys_alive.Any())
        {
            TurnManager.Instance.StopTurnBaseSystem();
            RelicStroageManager.RandomRelic();
            PlayerOptionHUDController.Hide();
            ServiceLocator.Get<SkillPointsHUD>().Hide();
        }
    }

    public IEnumerator TeleportFriends()
    {
        GameObject MainPlayer = MainCamera.Instance.Player.gameObject;
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        int playerCount = Players_alive.Count;
        int friendCount = Friends.Count;

        for (int i = 1; i < playerCount; i++)
        {
            if (i - 1 >= friendCount) break;

            GameObject player = Players_alive[i];
            Vector3 newPos = Friends[i - 1].transform.position;
            newPos.x = RoundToHalf(newPos.x);
            newPos.z = RoundToHalf(newPos.z);

            var cc = player.GetComponent<PlayerController>();
            var rb = player.GetComponent<Rigidbody>();

            if (cc != null)
            {
                cc.enabled = false;
                player.transform.position = newPos;
            }
            else if (rb != null)
            {
                rb.position = newPos;
                rb.velocity = Vector3.zero;
            }
            else
            {
                player.transform.position = newPos;
            }

            player.transform.rotation = MainPlayer.transform.rotation;

            Debug.Log($"Teleport {player.name} to {newPos}");
        }
    }

    private float RoundToHalf(float value)
    {
        float intPart = Mathf.Floor(value);   
        float result = intPart + 0.5f;       
        return result;
    }


    public void ReleasePlayers()
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

    public void GetBrokenRelic()
    {
        HPBarsHUDController.Hide();
        PocketHUDController.Hide();
        PlayersInfoHUDController.Hide();
        StartCoroutine(GetReward());
    }

    IEnumerator GetReward()
    {
        yield return new WaitForSeconds(1f);
        BrokenRelicManager.RandomBrokenRelic();
        ServiceLocator.Get<ResultPopup>().Show();
    }

    void Start()
    {
        StartGame();
        SettingHUDController.Show();
        GameObject SFXGameObjetc = Instantiate(Resources.Load<GameObject>(BGManager_Path));
        GameObject VFXGameObjetc = Instantiate(Resources.Load<GameObject>(VFXManager_Path));
        SFXManager.Instance.Play(SFX.Hub);
        RelicStroageManager.Init();
        BrokenRelicManager.Init();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ReleasePlayers();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SkillPointsManager.ReceiveSkillPoint(1);
            ServiceLocator.Get<SkillPointsHUD>().Update();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($"Skill point : {SkillPointsManager.currentSkillPoint}");
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
            RelicStroageManager.RandomRelic();
            RelicPocketPopupController.Update();
        }
    }
}
