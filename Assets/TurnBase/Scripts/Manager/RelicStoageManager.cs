using System.Collections.Generic;
using UnityEngine;

public static class RelicStroageManager
{
    public static List<RelicData> RelicsInUserPocket = new List<RelicData>();
    public static void ReceiveRelic(RelicData relic)
    {
        RelicsInUserPocket.Add(relic);
    }
    public static void ResetRelics()
    {
        RelicsInUserPocket.RemoveAll(Relic => RelicsInUserPocket.Contains(Relic));
    }
}