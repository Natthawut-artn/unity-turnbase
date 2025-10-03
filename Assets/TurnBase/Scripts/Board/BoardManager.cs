using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum RoomState
{
    Playing,
    UnPlaying,
    CanExit,
    Clear,
    Visit
}

[System.Serializable]
public class ExitGridData
{
    public int RoomIndex;
    public List<Vector2Int> Positions;
}

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject gameObj = gameObject.transform.GetChild(i).gameObject;
            if (gameObj.GetComponent<Room>())
            {
                ListRooms.Add(gameObj);
            }
        }

        GameObject BridgesParent = gameObject.transform.Find("Bridges").gameObject;
        for (int i = 0; i < BridgesParent.transform.childCount; i++)
        {
            GameObject bridge = BridgesParent.transform.GetChild(i).gameObject;
            if (bridge.GetComponent<Bridge>())
            {
                ListBridges.Add(bridge);
            }
        }

        InitMarkersForEveryRooms();
        LinkBridgesWithRooms();

        eventList = System.Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();
    }

    private const string MarkerPath = "Prefabs/Board/Marker";
    public List<GameObject> ListRooms = new List<GameObject>();
    public List<GameObject> ListBridges = new List<GameObject>();
    public List<ExitGridData> ExitGridPositions = new List<ExitGridData>();
    public List<Vector2Int> SizeRoomsData = new List<Vector2Int>();
    private List<EventType> eventList = new List<EventType>();
    public LayerMask MarkerLayer;
    private MeshRenderer currentMarker;
    public bool isTransferring = false;


    private void LinkBridgesWithRooms()
    {
        foreach (GameObject bridgeObj in ListBridges)
        {
            Bridge bridge = bridgeObj.GetComponent<Bridge>();

            GameObject room1 = GetRoom(bridge.ConnectWith1);
            GameObject room2 = GetRoom(bridge.ConnectWith2);

            if (room1 == null || room2 == null)
            {
                Debug.LogError($"Bridge {bridge.name} cannot find connected rooms!");
                continue;
            }

            Room room1Script = room1.GetComponent<Room>();
            Room room2Script = room2.GetComponent<Room>();

            // หา exit marker ของห้อง1 ตามตำแหน่งที่กำหนด
            Marker marker1 = room1.GetComponentsInChildren<Marker>()
                .FirstOrDefault(m => m.isExit && m.GridPosition == bridge.ExitPosRoom1);

            if (marker1 != null && marker1.ConnectedBridge == null)
            {
                marker1.ConnectedBridge = bridge;
                if (!room1Script.ExitMarkers.Contains(marker1))
                    room1Script.ExitMarkers.Add(marker1);

                bridge.ExitMarkerRoom1 = marker1;
            }
            else
            {
                Debug.LogWarning($"Bridge {bridge.name} could not find ExitMarker at {bridge.ExitPosRoom1} in room {room1.name}");
            }

            // หา exit marker ของห้อง2 ตามตำแหน่งที่กำหนด
            Marker marker2 = room2.GetComponentsInChildren<Marker>()
                .FirstOrDefault(m => m.isExit && m.GridPosition == bridge.ExitPosRoom2);

            if (marker2 != null && marker2.ConnectedBridge == null)
            {
                marker2.ConnectedBridge = bridge;
                if (!room2Script.ExitMarkers.Contains(marker2))
                    room2Script.ExitMarkers.Add(marker2);

                bridge.ExitMarkerRoom2 = marker2;
            }
            else
            {
                Debug.LogWarning($"Bridge {bridge.name} could not find ExitMarker at {bridge.ExitPosRoom2} in room {room2.name}");
            }
        }
    }


    public void InitMarkersForEveryRooms()
    {
        for (int i = 0; i < ListRooms.Count; i++)
        {
            int rows = SizeRoomsData[i].y;
            int cols = SizeRoomsData[i].x;
            SpawnMarkers(rows, cols, ListRooms[i], i);
        }
    }

    public void SpawnMarkers(int rows, int cols, GameObject Room, int roomIndex)
    {
        int spacing = 1;
        Room roomScript = Room.GetComponent<Room>();
        ExitGridData exitData = ExitGridPositions.Find(e => e.RoomIndex == roomIndex);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = (c - (cols - 1) / 2f) * spacing * -1;
                float y = -0.3f;
                float z = (r - (rows - 1) / 2f) * spacing * -1;

                Vector3 pos = new Vector3(x, y, z) + Room.transform.position;
                GameObject marker = Instantiate(Resources.Load<GameObject>(MarkerPath), pos, Quaternion.identity, Room.transform);
                marker.name = $"Marker R{r + 1} C{c + 1} in Room{roomIndex + 1}";

                Vector2Int markerPos = new Vector2Int(c + 1, r + 1);
                Marker markerScript = marker.GetComponent<Marker>();
                markerScript.GridPosition = markerPos;

                if (exitData != null && exitData.Positions.Contains(markerPos))
                {
                    markerScript.isExit = true;
                    roomScript.ExitMarkers.Add(markerScript);
                }
            }
        }
    }

    public GameObject GetRoom(string RoomName)
    {
        foreach (GameObject Room in ListRooms)
        {
            if (Room.name == RoomName)
            {
                return Room;
            }
        }
        return null;
    }

    public Vector3 GetMarkerWorldPosition(Vector2 GridPosition, GameObject Room)
    {
        for (int j = 0; j < Room.transform.childCount; j++)
        {
            Marker markerScript = Room.GetComponentInChildren<Marker>();
            if (GridPosition == markerScript.GridPosition)
            {
                return markerScript.transform.position;
            }
        }
        return Vector3.zero;
    }

    public Vector2 GetMarkerGameObject(GameObject MarkerGameObject)
    {
        Marker markerScript = MarkerGameObject.GetComponent<Marker>();
        return markerScript.GridPosition;
    }

    public void ShowMarkerMouseHover()
    {
        for (int i = 0; i < ListRooms.Count; i++)
        {
            Room room = ListRooms[i].GetComponent<Room>();
            if (room.roomState == RoomState.Playing || room.roomState == RoomState.CanExit)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer) && TurnManager.Instance.currentPhase == Phase.Player)
                {
                    MeshRenderer newMarker = hit.collider.GetComponent<MeshRenderer>();
                    if (newMarker != currentMarker)
                    {
                        if (currentMarker != null)
                            currentMarker.enabled = false;

                        currentMarker = newMarker;
                        currentMarker.enabled = true;
                    }

                    MeshRenderer MarkerView = hit.collider.gameObject.GetComponent<MeshRenderer>();
                    MarkerView.enabled = true;
                }
                else
                {
                    if (currentMarker != null)
                    {
                        currentMarker.enabled = true;
                        // currentMarker = null;
                    }
                }
            }
        }
    }

    public void CheckPlayerOnExit(GameObject marker)
    {
        foreach (GameObject room in ListRooms)
        {
            Room roomScript = room.GetComponent<Room>();
            if (roomScript.roomState == RoomState.CanExit)
            {
                OpenAllDoors(room);
            }
            if (roomScript.roomState == RoomState.CanExit || roomScript.roomState == RoomState.Clear)
            {
                Marker markerScript = marker.GetComponent<Marker>();
                if (markerScript.isExit)
                {
                    Debug.Log("Player on Exit");
                    TranferPlayerToOtherRoom(marker);
                    return;
                }
            }
        }
    }

    public void TranferPlayerToOtherRoom(GameObject markerObj)
    {
        if (isTransferring) return; // ป้องกัน loop
        isTransferring = true;

        Marker markerScript = markerObj.GetComponent<Marker>();
        if (!markerScript.isExit || markerScript.ConnectedBridge == null) { isTransferring = false; return; }

        Bridge bridge = markerScript.ConnectedBridge;
        Marker otherExit = bridge.GetOtherExit(markerScript);
        if (otherExit == null) { isTransferring = false; return; }

        StartCoroutine(PlayerController.Instance.MoveToCell(otherExit.gameObject));

        Debug.Log($"Player transfered to room {bridge.GetOtherRoom(markerObj.transform.parent.name)}");
    }

    public void OpenAllDoors(GameObject room)
    {
        if (room == null) return;

        string thisRoomName = room.name;

        // 1. เปิดเฉพาะ door ที่เชื่อมกับห้องอื่น (ผ่าน bridge)
        Room roomComponent = room.GetComponent<Room>();
        if (roomComponent != null)
        {
            foreach (GameObject doorObj in roomComponent.Doors)
            {
                if (doorObj == null) continue;
                Door door = doorObj.GetComponent<Door>();
                if (door == null) continue;

                // เช็คว่าประตูนี้มี Bridge เชื่อมอยู่จริงไหม
                if (HasBridgeBetween(thisRoomName, door.ConnectToRoom))
                {
                    door.Open();

                    // 2. หา room ฝั่งตรงข้าม แล้วเปิดประตูที่เชื่อมกลับมา
                    GameObject otherRoom = GetRoom(door.ConnectToRoom);
                    if (otherRoom != null)
                    {
                        Room otherRoomComp = otherRoom.GetComponent<Room>();
                        if (otherRoomComp != null)
                        {
                            foreach (GameObject otherDoorObj in otherRoomComp.Doors)
                            {
                                Door otherDoor = otherDoorObj.GetComponent<Door>();
                                if (otherDoor == null) continue;

                                // เปิดเฉพาะ door ที่เชื่อมกลับมาห้องต้นทาง
                                if (otherDoor.ConnectToRoom == thisRoomName)
                                {
                                    otherDoor.Open();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// เช็คว่ามี Bridge เชื่อมระหว่างสองห้องไหม
    /// </summary>
    private bool HasBridgeBetween(string roomA, string roomB)
    {
        foreach (GameObject bridgeObj in ListBridges)
        {
            Bridge bridge = bridgeObj.GetComponent<Bridge>();
            if (bridge == null) continue;

            if ((bridge.ConnectWith1 == roomA && bridge.ConnectWith2 == roomB) ||
                (bridge.ConnectWith1 == roomB && bridge.ConnectWith2 == roomA))
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
    }

    void Update()
    {
        ShowMarkerMouseHover();
    }
}
