using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicsStoage : MonoBehaviour
{
    List<RelicData> RelicsInPocket = new List<RelicData>();
    private PlayerData playerData;

    void Start()
    {
        playerData = GetComponent<PlayerData>();
    }

    public void InstallRelic(RelicData Relic)
    {
        if (!RelicsInPocket.Contains(Relic))
        {
            RelicsInPocket.Add(Relic);
            Relic.InstallRelic(playerData);
        }
    }

    public void RemoveRelic(RelicData relic)
    {
        if (RelicsInPocket.Contains(relic))
        {
            relic.RemoveRelic(playerData);
            RelicsInPocket.Remove(relic);
        }
    }
}

