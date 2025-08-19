using UnityEngine;

public class MainApp : MonoBehaviour
{
    public bool DestroyOnLoad = false;

    public static MainApp Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!DestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        CanvasManager.Init();
        var uiManager = gameObject.AddComponent<UIManager>();
        var loader = new UIListLoader(uiManager);
        loader.RegisterAll();
    }
}
