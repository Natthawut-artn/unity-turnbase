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
    public void SetPlayer(Transform Player)
    {
        this.Player = Player;
    }
    void Start()
    {
        if (Player != null)
        {
            offset = transform.position - Player.position;
        }
    }

    void LateUpdate()
    {
        if (Player == null) return;

        transform.position = Player.position + offset;
    }
}
