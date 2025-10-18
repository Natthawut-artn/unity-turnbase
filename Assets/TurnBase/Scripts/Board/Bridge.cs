using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{

    public string ConnectWith1;
    public string ConnectWith2;

    public List<Vector2Int> ExitPosRoom1 = new List<Vector2Int>();
    public List<Vector2Int> ExitPosRoom2 = new List<Vector2Int>();

    [Header("Assigned Exit Markers (Auto)")]
    public List<Marker> ExitMarkersRoom1 = new List<Marker>();
    public List<Marker> ExitMarkersRoom2 = new List<Marker>();
    private Dictionary<Marker, Marker> markerPairs = new Dictionary<Marker, Marker>();

    public string GetOtherRoom(string currentRoomName)
    {
        if (currentRoomName == ConnectWith1) return ConnectWith2;
        if (currentRoomName == ConnectWith2) return ConnectWith1;
        return null;
    }

    public List<Marker> GetOtherExits(Marker currentMarker)
    {
        if (ExitMarkersRoom1.Contains(currentMarker)) return ExitMarkersRoom2;
        if (ExitMarkersRoom2.Contains(currentMarker)) return ExitMarkersRoom1;
        return null;
    }

    public Marker GetOtherExit(Marker currentMarker)
    {
        if (markerPairs.TryGetValue(currentMarker, out Marker other))
            return other;
        return null;
    }
    
    public void AddMarkerPair(Marker m1, Marker m2)
    {
        if (m1 == null || m2 == null) return;

        if (!markerPairs.ContainsKey(m1))
            markerPairs[m1] = m2;

        if (!markerPairs.ContainsKey(m2))
            markerPairs[m2] = m1;
    }
}
