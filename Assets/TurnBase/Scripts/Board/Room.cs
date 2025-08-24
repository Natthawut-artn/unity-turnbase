using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomState roomState = RoomState.UnPlaying;
    public List<Marker> exitMarkers = new List<Marker>();
}
