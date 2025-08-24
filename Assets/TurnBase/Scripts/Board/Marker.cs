using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Vector2 GridPosition;
    public bool isExit = false;
    public Bridge connectedBridge;   // เชื่อมกับสะพานไหน
    public string connectedRoomName; // ปลายทางคือห้องไหน
}
