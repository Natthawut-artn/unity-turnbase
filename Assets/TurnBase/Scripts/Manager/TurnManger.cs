using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Phase
{
    Player,
    Enemy
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public Phase currentPhase = Phase.Player;

    private void Awake()
    {
        Instance = this;
    }

    public void EndTurn()
    {
        if (currentPhase == Phase.Player)
        {
            currentPhase = Phase.Enemy;
            StartCoroutine(EnemyPhase());
        }
    }

    private IEnumerator EnemyPhase()
    {
        Debug.Log("Enemy Phase started");

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            yield return StartCoroutine(enemy.TakeTurn());
        }

        Debug.Log("Enemy Phase ended, switching to Player Phase");
        currentPhase = Phase.Player;
    }
}