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

            Vector3 dir = desiredPos - Player.transform.position;

            dir.x = Mathf.Clamp(dir.x, -maxDistance.x, maxDistance.x);
            dir.y = Mathf.Clamp(dir.y, -maxDistance.y, maxDistance.y);
            dir.z = Mathf.Clamp(dir.z, -maxDistance.z, maxDistance.z);

            transform.position = Player.transform.position + dir;
        }
    }
}
