using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicsStoage : MonoBehaviour
{
    public List<RelicData> RelicsInPocket = new List<RelicData>();
    public List<RelicData> BrokenRelicsInPocket = new List<RelicData>();
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

    public void InstallBrokenRelic(RelicData Relic)
    {
        if (BrokenRelicsInPocket.Count < 4)
        {
            BrokenRelicsInPocket.Add(Relic);
            Relic.InstallRelic(playerData);
            BrokenRelicManager.BrokenRelicPocket.Remove(Relic);
        }
    }

    public void RemoveBrokenRelic(RelicData relic)
    {
        if (RelicsInPocket.Contains(relic))
        {
            relic.RemoveRelic(playerData);
            BrokenRelicsInPocket.Remove(relic);
            BrokenRelicManager.BrokenRelicPocket.Add(relic);
        }
    }
}

