using System.Collections.Generic;
using UnityEngine;

public static class BrokenRelicManager
{
    public static List<RelicData> BrokenRelicPocket = new List<RelicData>();
    public static string folder = "RelicsData/Broken";
    public static List<string> list_path = new List<string>();

    public static void Init()
    {
        Object[] objects = Resources.LoadAll(folder, (typeof(Object)));

        foreach (Object asset in objects)
        {
            string path = folder + "/" + asset.name;
            list_path.Add(path);
        }
    }
    public static void ReceiveBrokenRelic(RelicData brokenRelic)
    {
        if (BrokenRelicPocket.Count >= 30) { return; }
        BrokenRelicPocket.Add(brokenRelic);
    }
    public static void RemoveRelics(RelicData brokenRelic)
    {
        BrokenRelicPocket.Remove(brokenRelic);
    }

    public static void RandomBrokenRelic()
    {
        if (BrokenRelicPocket.Count >= 30) { return; }

        int num = Random.Range(0, list_path.Count);

        RelicData brokenRelic = Resources.Load<RelicData>(list_path[num]);

        ReceiveBrokenRelic(brokenRelic);
    }
}