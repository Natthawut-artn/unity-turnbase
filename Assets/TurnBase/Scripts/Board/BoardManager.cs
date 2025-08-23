using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomState
{
    Playing,
    UnPlaying,
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
            GameObject Room = gameObject.transform.GetChild(i).gameObject;
            ListRooms.Add(Room);
        }
    }

    private const string MarkerPath = "Prefabs/Board/Marker";
    public List<GameObject> ListRooms = new List<GameObject>();
    public List<Vector2Int> SizeRoomsData = new List<Vector2Int>();
    public LayerMask MarkerLayer;
    private MeshRenderer currentMarker;

    public void InitMarkersForEveryRooms()
    {
        for (int i = 0; i < ListRooms.Count; i++)
        {
            int rows = SizeRoomsData[i].y;
            int cols = SizeRoomsData[i].x;
            SpawnMarkers(rows, cols, ListRooms[i]);
        }
    }

    public void SpawnMarkers(int rows, int cols, GameObject Room)
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
            }
        }
    }

    public Vector3 GetMarkerWorldPosition(Vector2 GridPosition)
    {
        for (int i = 0; i < ListRooms.Count; i++)
        {
            Room room = ListRooms[i].GetComponent<Room>();
            if (room.roomState == RoomState.Playing)
            {
                for (int j = 0; j < ListRooms[i].transform.childCount; j++)
                {
                    Marker markerScript = ListRooms[i].GetComponentInChildren<Marker>();
                    if (GridPosition == markerScript.GridPosition)
                    {
                        return markerScript.transform.position;
                    }
                }    
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
            if (room.roomState == RoomState.Playing)
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

    void Start()
    {
        InitMarkersForEveryRooms();
    }

    void Update()
    {
        ShowMarkerMouseHover();
    }
}
