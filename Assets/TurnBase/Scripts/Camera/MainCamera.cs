using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public static MainCamera Instance;
    void Awake()
    {
        Instance = this;
    }
    public Transform Player;
    private Vector3 offset;
    public LayerMask obstacleLayers;
    private Dictionary<Renderer, FadeObject> fadingObjects = new();
    private Vector3 lastCameraPosition;
    private Vector3 lastPlayerPosition;
    public Camera cam;
    public float fadeDuration = 0.3f;
    public float targetAlpha = 0.2f;
    public float followSpeed = 5f;   

    public void SetPlayer(Transform Player)
    {
        this.Player = Player;
    }

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (Player != null)
        {
            offset = transform.position - Player.position;
            lastCameraPosition = transform.position;
            lastPlayerPosition = Player.position;
            DoRaycasts();
        }
    }

    void LateUpdate()
    {
        if (Player == null) return;

        if (transform.position != lastCameraPosition || Player.position != lastPlayerPosition)
        {
            lastCameraPosition = transform.position;
            lastPlayerPosition = Player.position;
            DoRaycasts();
        }

        Vector3 desiredPos = Player.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }

    void DoRaycasts()
    {
        // รีเซ็ตสถานะวัตถุทั้งหมดให้ Fade กลับ (ยกเลิกการบัง)
        foreach (var entry in fadingObjects.Values)
        {
            entry.MarkAsNotHitThisFrame();
        }

        // --- ยิง Ray ไปยัง Player ---
        Vector3 dirToPlayer = Player.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;
        Ray rayToPlayer = new Ray(transform.position, dirToPlayer.normalized);
        ProcessRay(rayToPlayer, distToPlayer, Color.red);

        // --- ยิง Ray ไปยังมุมล่างขวา ---
        Ray rayToCorner = cam.ViewportPointToRay(new Vector3(1, 0, 0));
        float maxDistance = cam.farClipPlane;
        ProcessRay(rayToCorner, maxDistance, Color.green);

        // --- ให้ทุกวัตถุที่ไม่ได้โดน Ray ค่อยๆ fade กลับ ---
        foreach (var entry in fadingObjects.Values)
        {
            entry.ApplyFade();
        }
    }

    void ProcessRay(Ray ray, float distance, Color debugColor)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleLayers);
        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                if (!fadingObjects.ContainsKey(rend))
                {
                    fadingObjects[rend] = new FadeObject(rend, fadeDuration, targetAlpha);
                }
                fadingObjects[rend].MarkAsHitThisFrame();
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * distance, debugColor, 0.1f);
    }
}
