using UnityEngine;

public static class CanvasManager
{
    public static GameObject Canvas0 { get; set; }
    public static GameObject Canvas1 { get; set; }
    public static GameObject Canvas2 { get; set; }
    public static GameObject Canvas3 { get; set; }
    public static GameObject Canvas4 { get; set; }

    public static void Init()
    {
        Canvas0 = GameObject.Find("Canvas0");
        Canvas1 = GameObject.Find("Canvas1");
        Canvas2 = GameObject.Find("Canvas2");
        Canvas3 = GameObject.Find("Canvas3");
        Canvas4 = GameObject.Find("Canvas4");
    }
}
