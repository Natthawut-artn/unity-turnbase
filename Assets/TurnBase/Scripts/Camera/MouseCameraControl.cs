using System.ComponentModel;
using UnityEngine;

public class MouseCameraControl : MonoBehaviour
{
    public static MouseCameraControl Instance;
    public float panSpeed = 20f;
    private Vector3 dragOrigin;
    private MainCamera mainCameraScript; // Reference to the main camera script
    private Plane groundPlane; // A virtual plane to measure movement against
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        mainCameraScript = GetComponent<MainCamera>();
        if (mainCameraScript == null)
        {
            Debug.LogError("MainCamera script not found on the camera object!");
        }
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void Update()
    {
        // Stop following the player while the mouse button is down
        if (Input.GetMouseButtonDown(1))
        {
            if (mainCameraScript != null) mainCameraScript.IsFollowingPlayer = false;

            // Cast a ray into the scene to get the initial point on the plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (groundPlane.Raycast(ray, out entry))
            {
                dragOrigin = ray.GetPoint(entry);
            }
        }

        // Panning logic while the button is held
        if (Input.GetMouseButton(1))
        {
            // Cast a new ray to the current mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (groundPlane.Raycast(ray, out entry))
            {
                Vector3 currentGroundPoint = ray.GetPoint(entry);
                // Calculate the difference and move the camera
                Vector3 moveDifference = dragOrigin - currentGroundPoint;
                transform.position += moveDifference * panSpeed * Time.deltaTime; // Use += for continuous movement
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SnapToPlayer();
        }
    }
    
    public void SnapToPlayer()
    {
        if (mainCameraScript != null)
        {
            mainCameraScript.IsFollowingPlayer = true;
        }
    }
}
