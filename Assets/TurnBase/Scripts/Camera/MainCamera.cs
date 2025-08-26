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
            transform.position = Player.transform.position + offset;
        }
    }
}
