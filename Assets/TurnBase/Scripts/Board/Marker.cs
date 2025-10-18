using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Vector2 GridPosition;
    public bool isExit = false;
    public Bridge ConnectedBridge;

    private Renderer _renderer;
    private Color _originalColor;
    public bool IsOccupied { get; set; } = false;
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    public void SetMarkerColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }

    public void RestoreOriginalColor()
    {
        if (_renderer != null)
        {
            _renderer.material.color = _originalColor;
        }
    }
}
