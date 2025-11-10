using UnityEngine;
using System.Collections.Generic; // Required for using List<>

[RequireComponent(typeof(Room))]
public class ExitIndicatorController : MonoBehaviour
{
    public Room room; // Reference to the Room script on this GameObject
    public RoomState previousState; // To track when the state changes

    // --- NEW ---
    // Assign your indicator GameObjects here in the Inspector.

    /// <summary>
    /// All visuals (arrows, etc.) that point to the *next* room.
    /// </summary>
    public List<GameObject> nextRoomIndicators = new List<GameObject>();

    /// <summary>
    /// All visuals (arrows, etc.) that point back to the *previous* room.
    /// </summary>
    public List<GameObject> prevRoomIndicators = new List<GameObject>();


    void Start()
    {
        // Get the Room component on this same GameObject
        room = GetComponent<Room>();

        if (room == null)
        {
            Debug.LogError("ExitIndicatorController requires a Room component on the same GameObject.", this);
            enabled = false; // Disable this script if no Room is found
            return;
        }

        // Initialize the state
        previousState = room.roomState;

        // Ensure all indicators are hidden when the game starts
        SetExitIndicators(false);
    }

    void Update()
    {
        // Check every frame if the state in the Room script has changed
        if (room.roomState != previousState)
        {
            // The state has changed! Call our logic function.
            HandleStateChange(previousState, room.roomState);

            // Update the previousState so we can detect the *next* change
            previousState = room.roomState;
        }
    }

    /// <summary>
    /// This function is called by Update() only when a state change is detected.
    /// </summary>
    private void HandleStateChange(RoomState oldState, RoomState newState)
    {
        // --- SHOW INDICATORS ---
        // When we ENTER the CanExit state
        if (newState == RoomState.CanExit)
        {
            SetExitIndicators(true);
        }
        // --- HIDE INDICATORS ---
        // When we LEAVE the CanExit state (e.g., changing from CanExit to Clear)
        else if (oldState == RoomState.CanExit)
        {
            SetExitIndicators(false);
        }
    }

    /// <summary>
    /// Turns all the exit indicators on or off.
    /// This now uses its own public lists of GameObjects.
    /// </summary>
    private void SetExitIndicators(bool show)
    {
        // Activate/Deactivate all "Next Room" indicators
        foreach (GameObject indicator in nextRoomIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(show);
            }
        }

        // Activate/Deactivate all "Previous Room" indicators
        foreach (GameObject indicator in prevRoomIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(show);
            }
        }
    }
}