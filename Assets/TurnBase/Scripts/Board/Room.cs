using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomState roomState = RoomState.UnPlaying;
    public List<Marker> ExitMarkers = new List<Marker>();
    public EventType eventType;
    public List<GameObject> Doors = new List<GameObject>();

    public List<Vector2> AllGridPositions
    {
        get
        {
            // Use GetComponentsInChildren to find all Marker scripts attached to the room or its children.
            // Select the GridPosition from each Marker (now a Vector2).
            return GetComponentsInChildren<Marker>()
                .Select(m => m.GridPosition)
                .ToList();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (roomState == RoomState.UnPlaying)
            {
                roomState = RoomState.Playing;
            }
            else if (roomState == RoomState.Clear)
            {
                roomState = RoomState.CanExit;
            }
        }

        if (other.gameObject.tag == "Door")
        {
            Doors.Add(other.gameObject);    
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (roomState == RoomState.CanExit)
            {
                roomState = RoomState.Clear;
            }
        }
    }
}
