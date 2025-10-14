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
    public bool isInTurnSystem = false;
    private Coroutine enemyPhaseCoroutine;
    private Coroutine playersPhaseCoroutine;
    private void Awake()
    {
        Instance = this;
    }

    public void StopTurnBaseSystem()
    {
        if (enemyPhaseCoroutine != null)
        {
            StopCoroutine(enemyPhaseCoroutine);
            enemyPhaseCoroutine = null;
        }

        if (playersPhaseCoroutine != null)
        {
            StopCoroutine(playersPhaseCoroutine);
            playersPhaseCoroutine = null;
        }

        isInTurnSystem = false;
        currentPhase = Phase.Player;
        PlayerOptionHUDController.Hide();
        List<GameObject> players = GameManager.Instance.Players_alive;

        foreach (GameObject player in players)
        {
            Debug.Log(player.name);
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            controller.enabled = true;
            attack.enabled = true;
        }

        Debug.Log("Stop turnbase");
    }

    public void EndTurn()
    {
        if (!isInTurnSystem) return;
        if (currentPhase == Phase.Player)
        {
            playersPhaseCoroutine = StartCoroutine(PlayersPhase());
            
        }
        else
        {
            enemyPhaseCoroutine = StartCoroutine(EnemyPhase());
        }
    }

    public IEnumerator EnemyPhase()
    {
        Debug.Log("Enemy Phase started");

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        foreach (EnemyAI enemy in enemies)
        {
            yield return StartCoroutine(enemy.TakeTurn());
        }

        Debug.Log("Enemy Phase ended, switching to Player Phase");

        currentPhase = Phase.Player;

        EndTurn();
    }

    public IEnumerator PlayersPhase()
    {
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

        currentPhase = Phase.Enemy;
        EndTurn();
    }
}