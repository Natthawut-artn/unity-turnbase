using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Dictionary<Vector2, Vector3> MarkerPositions = new Dictionary<Vector2, Vector3>();
    public RoomState roomState = RoomState.UnPlaying;
}
