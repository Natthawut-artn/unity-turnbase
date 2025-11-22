using System.Collections.Generic;
using UnityEngine;

public static class BrokenRelicManager
{
    public static List<RelicData> BrokenRelicPocket = new List<RelicData>();
    public static List<List<RelicData>> BrokensStorage = new List<List<RelicData>>();


    public static string folderNormal = "RelicsData/Broken/Normal";
    public static string folderRare = "RelicsData/Broken/Rare";
    public static string folderLegent = "RelicsData/Broken/Legent";
    public static string folderAR = "RelicsData/Broken/AR";
    public static List<string> normalPaths = new List<string>();
    public static List<string> rarePaths = new List<string>();
    public static List<string> legentPaths = new List<string>();
    public static List<string> ARPaths = new List<string>();
    public static int NormalRate = 70;
    public static int RareRate = 15;
    public static int LegentRate = 10;
    public static int ARRate = 5;


    public static void Init()
    {
        Object[] normals = Resources.LoadAll(folderNormal, typeof(Object));
        Object[] rares = Resources.LoadAll(folderRare, typeof(Object));
        Object[] legents = Resources.LoadAll(folderLegent, typeof(Object));
        Object[] ars = Resources.LoadAll(folderAR, typeof(Object));

        foreach (Object asset in normals)
        {
            string path = folderNormal + "/" + asset.name;
            normalPaths.Add(path);
        }
        foreach (Object asset in rares)
        {
            string path = folderRare + "/" + asset.name;
            rarePaths.Add(path);
        }
        foreach (Object asset in legents)
        {
            string path = folderLegent + "/" + asset.name;
            legentPaths.Add(path);
        }
        foreach (Object asset in ars)
        {
            string path = folderAR + "/" + asset.name;
            ARPaths.Add(path);
        }

        for (int i = 0; i < 3 ;i++) 
        {
            List<RelicData> brokenrelics = new List<RelicData>();
            BrokensStorage.Add(brokenrelics);
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
        RelicData relic;
        int number = Random.Range(1, NormalRate + RareRate + LegentRate + ARRate);
        if (number <= NormalRate)
        {
            relic = RandomNormalRelic();
        }
        else if (number <= NormalRate + RareRate)
        {
            relic = RandomRareRelic();
        }
        else if(number <= NormalRate + RareRate + LegentRate)
        {
            relic = RandomLegentRelic();
        }
        else
        {
            relic = RandomARRelic();
        }

        if (relic == null)
        {
            Debug.LogError("RandomRelic() failed: relic is null!");
            return;
        }

        ReceiveBrokenRelic(relic);
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
    public static RelicData RandomARRelic()
    {
        if (ARPaths == null || ARPaths.Count == 0)
        {
            Debug.LogError("normalPaths is empty!");
            return null;
        }

        int number = Random.Range(0, ARPaths.Count);
        RelicData relic = Resources.Load<RelicData>(ARPaths[number]);

        if (relic == null)
            Debug.LogError($"Failed to load Normal Relic at path: {ARPaths[number]}");

        return relic;
    }

    public static void Destroy(int index)
    {
        BrokenRelicPocket.RemoveAt(index);
    }

    public static void InitInStage()
    {
        for (int i = 0; i < GameManager.Instance.Players_alive.Count ; i++)
        {
            RelicsStoage relicsStoage = GameManager.Instance.Players_alive[i].GetComponent<RelicsStoage>();
            List<RelicData> brokenRelic = BrokensStorage[i];
            for (int j = 0 ; j < brokenRelic.Count ; j++)
            {
                relicsStoage.InstallBrokenRelic(brokenRelic[j]);
            }
        }
    }

    public static void InstallInPlayer(string playername, RelicData brokenrelicData)
    {
        if(AppStateManager.appState == AppState.Hub)
        {
            if (playername.Contains("Striker"))
            {
                List<RelicData> brokenrelics = BrokensStorage[0];
                brokenrelics.Add(brokenrelicData);
            }
            else if (playername.Contains("Support"))
            {
                List<RelicData> brokenrelics = BrokensStorage[1];
                brokenrelics.Add(brokenrelicData);
            }
            if (playername.Contains("Protector"))
            {
                List<RelicData> brokenrelics = BrokensStorage[2];
                brokenrelics.Add(brokenrelicData);
            }
        }
    }
    
}