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
    private GameObject Player;
    private Vector3 offset;
    public Vector3 maxDistance; 
    public void SetPlayer(GameObject Player)
    {
        this.Player = Player;
        if (this.Player != null)
        {
            offset = transform.position - this.Player.transform.position;
        }
    }
    void Update()
    {
        if (Player != null)
        {
            Vector3 desiredPos = Player.transform.position + offset;

            // หาทิศทางจาก Player ไปตำแหน่งกล้องที่ต้องการ
            Vector3 dir = desiredPos - Player.transform.position;

            // จำกัดระยะห่างทีละแกน
            dir.x = Mathf.Clamp(dir.x, -maxDistance.x, maxDistance.x);
            dir.y = Mathf.Clamp(dir.y, -maxDistance.y, maxDistance.y);
            dir.z = Mathf.Clamp(dir.z, -maxDistance.z, maxDistance.z);

            // อัปเดตตำแหน่งกล้อง
            transform.position = Player.transform.position + dir;
        }
    }
}
