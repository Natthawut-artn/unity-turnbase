using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add any other necessary 'using' statements (e.g., for AppState, RoomState)
// using Unity.VisualScripting; // Example
// using UnityEngine.AI;      // Example

public class BossController : MonoBehaviour
{
    [Tooltip("The UI Panel to show when the boss is defeated. Must be assigned in the Inspector.")]
    public GameObject endgamePanel;
    public static BossController Instance;
    private bool isDefeated = false;
    // Tracks occupied grid positions (X,Z) - COPIED FROM ENEMYAI
    private Vector2 currentGridPos;

    // Public reference to the room - ALREADY PRESENT
    public Room parentRoom;

    // Assuming a default step size for grid calculation (adjust if your boss uses a different size)
    public float stepSize = 2f;

    // Assuming your Boss has a component to track health, like EnemyData or BossData
    // public BossData bossData; // Example reference

    // ----------------------------------------------------------------------
    // GRID UTILITY METHODS (Copied and adapted from EnemyAI)
    // ----------------------------------------------------------------------

   
    private Vector2 GetGridPosition(Vector3 worldPos)
    {
        return new Vector2(Mathf.Round(worldPos.x), Mathf.Round(worldPos.z));
    }

    private void SetMarkerOccupancy(Vector2 gridPos, bool isOccupied)
    {
        // Must check for nulls since BoardManager is static and Room is a reference
        if (parentRoom == null || BoardManager.Instance == null) return;

        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, parentRoom.gameObject);
        if (markerGO != null)
        {
            Marker marker = markerGO.GetComponent<Marker>();
            if (marker != null)
            {
                marker.IsOccupied = isOccupied;
            }
        }
    }

    // ----------------------------------------------------------------------
    // LIFECYCLE METHODS
    // ----------------------------------------------------------------------

    void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Keep optional parent search here, but don't use the reference yet.
        if (parentRoom == null)
        {
            parentRoom = GetComponentInParent<Room>();
        }
        if (endgamePanel == null)
        {
            // ⭐ Make sure your panel is *in the scene* when this Awake runs.
            // If the panel is a child of something that gets destroyed/created,
            // or if it's in a different scene, this will fail.
            endgamePanel = GameObject.FindWithTag("EndgamePanel");
            if (endgamePanel == null)
            {
                Debug.LogError("BossController: Could not find GameObject with tag 'EndgamePanel'. Panel will not show!", this);
            }
        }
    }

    void Start()
    {
       
        // ⭐ CRITICAL: The grid logic MUST be in Start() for spawned objects.
        if (parentRoom == null)
        {
            Debug.LogError("BossController: 'parentRoom' is NULL. Cannot register boss on grid.", this);
            return;
        }

        // Initialize grid position and mark occupancy now that parentRoom is set.
        currentGridPos = GetGridPosition(transform.position);
        SetMarkerOccupancy(currentGridPos, true);
    }
    // Update is called once per frame
    void Update()
    {
        if (GetComponent<BossData>()?.currentHP <= 0 && !isDefeated)
        {
            BossDefeated();
            return;
        }
        // 1. Destroy if room is unplaying
        if (parentRoom == null || parentRoom.roomState == RoomState.UnPlaying)
        {
            DestroySelf();
            return;
        }

        // 2. Destroy if player goes back to hub
        if (GameManager.Instance != null && AppStateManager.appState == AppState.Hub)
        {
            DestroySelf();
            return;
        }

        // 3. ⭐ Boss Death Check (You need to implement the HP/death check here)
        // Example check, assuming a component named BossData handles HP
        if (GetComponent<BossData>()?.currentHP <= 0)
        {
            BossController bossController = GetComponent<BossController>();
            GameManager.Instance.Enemys_alive.Remove(bossController.gameObject);
            TurnManager.Instance?.StopTurnBaseSystem();
            DestroySelf();
             return;
        }
        
    }
    private void BossDefeated()
    {
        if (isDefeated) return;
        isDefeated = true; // Set the flag immediately

        // 1. Stop the turn system and remove from active lists immediately
        GameManager.Instance.Enemys_alive.Remove(gameObject); // Use gameObject instead of GetComponent<BossController>().gameObject
        TurnManager.Instance?.StopTurnBaseSystem();

        // 2. Start the coroutine to handle panel display and timed destruction
        StartCoroutine(ShowEndgamePanelAndDestroy());
    }
    private IEnumerator ShowEndgamePanelAndDestroy()
    {
        GameObject panel = UIEndGameManager.Instance?.endgamePanel;

        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("Endgame Panel Activated via UIManager.");
        }
        else
        {
            Debug.LogError("UIManager or Endgame Panel reference is missing!");
        }

        // 2. WAIT
        float panelDisplayTime = 3.0f;
        yield return new WaitForSeconds(panelDisplayTime);

        // 3. FINAL CLEANUP and DESTRUCTION
        SetMarkerOccupancy(currentGridPos, false);

        Destroy(gameObject);
    }
    // ⭐ THE FIXED CLEANUP FUNCTION
    private void DestroySelf()
    {
        // 1. Clear grid occupancy BEFORE destroying the GameObject
        SetMarkerOccupancy(currentGridPos, false);
        TurnManager.Instance.isInTurnSystem = false;

        // 2. Destroy the boss object
        Destroy(gameObject);
    }

    // Safety measure: if another script destroys the boss directly
    private void OnDestroy()
    {
        
        // This ensures the grid is cleared even if another script calls Destroy(gameObject) directly
        if (currentGridPos != Vector2.zero)
        {
            SetMarkerOccupancy(currentGridPos, false);
        }
    }
}