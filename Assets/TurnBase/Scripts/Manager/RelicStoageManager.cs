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
        Object[] Normalassets = Resources.LoadAll(folderNormal, typeof(Object));
        Object[] Rareassets = Resources.LoadAll(folderRare, typeof(Object));
        Object[] Legentassets = Resources.LoadAll(folderLegent, typeof(Object));

        foreach (var asset in Normalassets)
        {
            string path = folderNormal + "/" + asset.name;
            normalPaths.Add(path);
        }
        foreach (var asset in Rareassets)
        {
            string path = folderRare + "/" + asset.name;
            rarePaths.Add(path);
        }
        foreach (var asset in Legentassets)
        {
            string path = folderLegent + "/" + asset.name;
            legentPaths.Add(path);
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
        RelicData relic;
        int number = Random.Range(1, NormalRate + RareRate + LegentRate);
        if (number <= NormalRate)
        {
            relic = RandomNormalRelic();
        }
        else if (number <= NormalRate + RareRate)
        {
            relic = RandomRareRelic();
        }
        else
        {
            relic = RandomLegentRelic();
        }

        if (relic == null)
        {
            Debug.LogError("RandomRelic() failed: relic is null!");
            return;
        }

        ReceiveRelic(relic);
        string text = $"You got new Relic : {relic.RelicName}";
        Sprite sprite = relic.RelicSprite;
        AnnouncerPopupController.Show(text,sprite);
    }

    public static RelicData RandomNormalRelic()
    {
        if (normalPaths == null || normalPaths.Count == 0)
        {
            Debug.LogError("normalPaths is empty!");
            return null;
        }

        int number = Random.Range(0, normalPaths.Count);
        RelicData relic = Resources.Load<RelicData>(normalPaths[number]);

        if (relic == null)
            Debug.LogError($"Failed to load Normal Relic at path: {normalPaths[number]}");
        
        return relic;
    }
    public static RelicData RandomRareRelic()
    {
        if (rarePaths == null || rarePaths.Count == 0)
        {
            Debug.LogError("normalPaths is empty!");
            return null;
        }

        int number = Random.Range(0, rarePaths.Count);
        RelicData relic = Resources.Load<RelicData>(rarePaths[number]);

        if (relic == null)
            Debug.LogError($"Failed to load Normal Relic at path: {rarePaths[number]}");
        
        return relic;
    }
    public static RelicData RandomLegentRelic()
    {
        if (legentPaths == null || legentPaths.Count == 0)
        {
            Debug.LogError("normalPaths is empty!");
            return null;
        }

        int number = Random.Range(0, legentPaths.Count);
        RelicData relic = Resources.Load<RelicData>(legentPaths[number]);

        if (relic == null)
            Debug.LogError($"Failed to load Normal Relic at path: {legentPaths[number]}");

        return relic;
    }
    
}