using System.Collections.Generic;
using UnityEngine;

public static class RelicStroageManager
{
    public static string folderNormal = "RelicsData/Normal";
    public static string folderRare = "RelicsData/Rare";
    public static string folderLegent = "RelicsData/Legent";
    public static List<string> normalPaths = new List<string>();
    public static List<string> rarePaths = new List<string>();
    public static List<string> legentPaths = new List<string>();
    public static int NormalRate = 60;
    public static int RareRate = 30;
    public static int LegentRate = 10;
    public static void Init()
    {
        Object[] assets = Resources.LoadAll(folderNormal, typeof(Object));

        foreach (var asset in assets)
        {
            string path = folderNormal + "/" + asset.name;
            normalPaths.Add(path);
            // Debug.Log($"Loaded assets from {path}");
        }
        foreach (var asset in assets)
        {
            string path = folderRare + "/" + asset.name;
            rarePaths.Add(path);
            // Debug.Log($"Loaded assets from {path}");
        }
        foreach (var asset in assets)
        {
            string path = folderLegent + "/" + asset.name;
            legentPaths.Add(path);
            // Debug.Log($"Loaded assets from {path}");
        }
    }
    public static List<RelicData> RelicsInUserPocket = new List<RelicData>();
    public static void ReceiveRelic(RelicData relic)
    {
        if(RelicsInUserPocket.Count >= 12){ return; }
        RelicsInUserPocket.Add(relic);
    }
    public static void ResetRelics()
    {
        RelicsInUserPocket.RemoveAll(Relic => RelicsInUserPocket.Contains(Relic));
    }

    public static void RandomRelic()
    {
        RelicData relic = null;
        int number = Random.Range(1, NormalRate + RareRate + LegentRate);
        if (number <= NormalRate)
        {
            relic = RandomNormalRelic();
        }
        else if (number <= NormalRate + RareRate)
        {
            relic = RandomRareRelic();
        }
        else if (number <= NormalRate + RareRate + LegentRate)
        {
            relic = RandomLegentRelic();
        }

        ReceiveRelic(relic);
        string text = $"You got new Relic : {relic.RelicName}";
        Sprite sprite = relic.RelicSprite;
        AnnouncerPopupController.Show(text,sprite);
    }

    public static RelicData RandomNormalRelic()
    {
        int number = Random.Range(0, normalPaths.Count);
        RelicData relic = Resources.Load<RelicData>(normalPaths[number]);
        return relic;
    }
    public static RelicData RandomRareRelic()
    {
        int number = Random.Range(0, rarePaths.Count);
        RelicData relic = Resources.Load<RelicData>(rarePaths[number]);
        return relic;
    }
    public static RelicData RandomLegentRelic()
    {
        int number = Random.Range(0, legentPaths.Count);
        RelicData relic = Resources.Load<RelicData>(legentPaths[number]);
        return relic;
    }
    
}