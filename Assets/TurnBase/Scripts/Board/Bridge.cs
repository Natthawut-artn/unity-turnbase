using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public string ConnectWith1;
    public string ConnectWith2;
    public Vector2Int ExitPosRoom1;   // กำหนดใน Inspector
    public Vector2Int ExitPosRoom2;   // กำหนดใน Inspector

    [Header("Assigned Exit Markers (Auto)")]
    public Marker ExitMarkerRoom1;
    public Marker ExitMarkerRoom2;

    public string GetOtherRoom(string currentRoomName)
    {
        if (currentRoomName == ConnectWith1) return ConnectWith2;
        if (currentRoomName == ConnectWith2) return ConnectWith1;
        return null;
    }

    public Marker GetOtherExit(Marker currentMarker)
    {
        if (currentMarker == ExitMarkerRoom1) return ExitMarkerRoom2;
        if (currentMarker == ExitMarkerRoom2) return ExitMarkerRoom1;
        return null;
    }
}
