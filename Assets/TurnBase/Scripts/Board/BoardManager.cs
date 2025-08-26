using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum RoomState
{
    Playing,
    UnPlaying,
    CanExit,
    Clear
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
            else if (gameObj.GetComponent<Bridge>())
            {
                ListBridges.Add(gameObj);
            }
        }

        InitMarkersForEveryRooms();
        LinkBridgesWithRooms();
    }

    private const string MarkerPath = "Prefabs/Board/Marker";
    public List<GameObject> ListRooms = new List<GameObject>();
    public List<GameObject> ListBridges = new List<GameObject>();
    public List<Vector2Int> ExitGridPosition = new List<Vector2Int>();
    public List<Vector2Int> SizeRoomsData = new List<Vector2Int>();
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

            // หา exit marker ของห้อง1
            foreach (Marker marker in room1.GetComponentsInChildren<Marker>())
            {
                if (marker.isExit && !marker.ConnectedBridge)
                {
                    marker.ConnectedBridge = bridge;
                    room1Script.ExitMarkers.Add(marker);
                    bridge.ExitMarkerRoom1 = marker;
                    break;
                }
            }

            // หา exit marker ของห้อง2
            foreach (Marker marker in room2.GetComponentsInChildren<Marker>())
            {
                if (marker.isExit && !marker.ConnectedBridge)
                {
                    marker.ConnectedBridge = bridge;
                    room2Script.ExitMarkers.Add(marker);
                    bridge.ExitMarkerRoom2 = marker;
                    break;
                }
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

    public void SpawnMarkers(int rows, int cols, GameObject Room , int Index)
    {
        int spacing = 1;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = (c - (cols - 1) / 2f) * spacing * -1;
                float y = -0.49f;
                float z = (r - (rows - 1) / 2f) * spacing * -1;

                Vector3 pos = new Vector3(x, y, z) + Room.transform.position;
                GameObject marker = Instantiate(Resources.Load<GameObject>(MarkerPath), pos, Quaternion.identity, Room.transform);
                marker.name = $"Marker R{r + 1} C{c + 1}";

                Vector2 MarkerPositionKey = new Vector2(c + 1, r + 1);
                Marker markerScript = marker.GetComponent<Marker>();
                markerScript.GridPosition = MarkerPositionKey;

                if (MarkerPositionKey == ExitGridPosition[Index])
                {
                    markerScript.isExit = true;    
                    Room roomScript = Room.GetComponent<Room>();
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

    public Vector3 GetMarkerWorldPosition(Vector2 GridPosition , GameObject Room)
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

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, MarkerLayer) && PlayerData.playerMoveState == PlayerMoveState.Stand)
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

    public void CheckPlayerInWhatRoom()
    {
        for (int i = 0; i < ListRooms.Count; i++)
        {
            Collider col = ListRooms[i].GetComponent<Collider>();    
            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents, Quaternion.identity);

            bool playerFound = false;

            foreach (Collider hit in hits)
            {
                if (hit.gameObject.CompareTag("Player"))
                {
                    playerFound = true;
                    break; // เจอแล้วไม่ต้องวนต่อ
                }
            }

            Room room = ListRooms[i].GetComponent<Room>();
            if (playerFound)
            {
                if (room.roomState == RoomState.UnPlaying)
                {
                    room.roomState = RoomState.Playing;
                }
            }
            else
            {
                if (room.roomState == RoomState.CanExit)
                {
                    room.roomState = RoomState.Clear;
                }
            }
        }
    }

    public void CheckPlayerOnExit(GameObject marker)
    {
        foreach (GameObject room in ListRooms)
        {
            Room roomScript = room.GetComponent<Room>();
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

        PlayerData.playerMoveState = PlayerMoveState.Walk;
        StartCoroutine(PlayerController.Instance.MoveToCell(otherExit.gameObject));

        Debug.Log($"Player transfered to room {bridge.GetOtherRoom(markerObj.transform.parent.name)}");
    }


    void Start()
    {
    }

    void Update()
    {
        ShowMarkerMouseHover();
    }
}
