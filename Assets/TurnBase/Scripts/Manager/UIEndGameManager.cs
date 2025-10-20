using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEndGameManager : MonoBehaviour
{
    public static UIEndGameManager Instance;

    [Tooltip("The Endgame Panel (must be assigned in Inspector)")]
    public GameObject endgamePanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure the manager persists across scene loads if needed
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
