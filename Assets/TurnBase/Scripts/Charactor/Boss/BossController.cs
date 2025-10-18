using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add any other necessary 'using' statements (e.g., for AppState, RoomState)
// using Unity.VisualScripting; // Example
// using UnityEngine.AI;      // Example

public class BossController : MonoBehaviour
{
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
        // Keep optional parent search here, but don't use the reference yet.
        if (parentRoom == null)
        {
            parentRoom = GetComponentInParent<Room>();
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
        /* // Example check, assuming a component named BossData handles HP
        if (GetComponent<BossData>()?.currentHP <= 0)
        {
             DestroySelf();
             return;
        }
        */
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