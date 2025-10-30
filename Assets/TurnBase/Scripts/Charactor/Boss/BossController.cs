using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add any other necessary 'using' statements
// using UnityEngine.AI; // Example

public class BossController : MonoBehaviour
{
    public string EnemyType;
    public CharactorInheritance charactorInheritance;
    public static BossController Instance;
    // Tracks occupied grid positions (X,Z) - COPIED FROM ENEMYAI
    private Vector2 currentGridPos;

    // Public reference to the room - ALREADY PRESENT
    public Room parentRoom;

    // Assuming a default step size for grid calculation
    public float stepSize = 2f;

    public SwapForm swapForm;
    public BossData bossData;
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
        // This Instance check seems redundant, but keeping as is.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (parentRoom == null)
        {
            parentRoom = GetComponentInParent<Room>();
        }
    }

    void Start()
    {
        if (parentRoom == null)
        {
            Debug.LogError("BossController: 'parentRoom' is NULL. Cannot register boss on grid.", this);
            return;
        }
        
        GameManager.IsBossSwapping = false;
        currentGridPos = GetGridPosition(transform.position);
        SetMarkerOccupancy(currentGridPos, true);
    }

    void Update()
    {
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

        if (EnemyType == "The Mother" && bossData.currentHP <= 0)
        {
            GameManager.Instance.GetBrokenRelic();
            TurnManager.Instance?.StopTurnBaseSystem();
            DestroySelf();
        }
    }

    // --- REMOVED THE INCORRECT 'override void Die()' METHOD ---
    // The Die() logic must be placed in the class that inherits 
    // from CharactorInheritance (e.g., BossData.cs or EnemyData.cs)


    // ⭐ CLEANUP FUNCTION (NOW PUBLIC)
    // This can now be called by BossData.Die()
    public void DestroySelf()
    {
        // 1. Clear grid occupancy BEFORE destroying the GameObject
        SetMarkerOccupancy(currentGridPos, false);
        Destroy(gameObject);
    }

    // Safety measure: if another script destroys the boss directly
    private void OnDestroy()
    {
        if (currentGridPos != Vector2.zero)
        {
            SetMarkerOccupancy(currentGridPos, false);
        }
    }
}

