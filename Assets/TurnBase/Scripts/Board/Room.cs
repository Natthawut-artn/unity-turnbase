using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomState roomState = RoomState.UnPlaying;
    public List<Marker> ExitMarkers = new List<Marker>();
    public RoomColor roomColor;

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
