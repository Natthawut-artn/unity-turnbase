using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomState roomState = RoomState.UnPlaying;
    public List<Marker> ExitMarkers = new List<Marker>();
    public EventType eventType;
    public List<GameObject> Doors = new List<GameObject>();

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
