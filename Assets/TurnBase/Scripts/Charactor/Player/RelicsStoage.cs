using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicsStoage : MonoBehaviour
{
    public List<RelicData> RelicsInPocket = new List<RelicData>();
    private PlayerData playerData;

    void Start()
    {
        playerData = GetComponent<PlayerData>();
    }

    public void InstallRelic(RelicData Relic)
    {
        if (!RelicsInPocket.Contains(Relic) && RelicsInPocket.Count < 4)
        {
            RelicsInPocket.Add(Relic);
            Relic.InstallRelic(playerData);
            RelicStroageManager.RelicsInUserPocket.Remove(Relic);
        }
    }

    public void RemoveRelic(RelicData relic)
    {
        if (RelicsInPocket.Contains(relic))
        {
            relic.RemoveRelic(playerData);
            RelicsInPocket.Remove(relic);
            RelicStroageManager.RelicsInUserPocket.Add(relic);
        }
    }
}

