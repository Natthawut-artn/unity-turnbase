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
    public GameObject CurrentPlayer;
    private void Awake()
    {
        Instance = this;
    }

    public void EndTurn()
    {
        if (currentPhase == Phase.Player)
        {
            StartCoroutine(PlayersPhase());
        }
        else
        {
            StartCoroutine(EnemyPhase());
        }
    }

    public IEnumerator EnemyPhase()
    {
        currentPhase = Phase.Enemy;

        Debug.Log("Enemy Phase started");

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            yield return StartCoroutine(enemy.TakeTurn());
        }

        Debug.Log("Enemy Phase ended, switching to Player Phase");

        EndTurn();
    }

    public IEnumerator PlayersPhase()
    {
        currentPhase = Phase.Player;

        List<GameObject> players = GameManager.Instance.Players_alive;

        foreach (GameObject player in players)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            controller.enabled = false;
            attack.enabled = false;
        }

        foreach (GameObject player in players)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            controller.enabled = true;
            attack.enabled = true;
            yield return StartCoroutine(controller.TakeTurn());
            controller.enabled = false;
            attack.enabled = false;
        }

        yield return null;

        EndTurn();
    }
}