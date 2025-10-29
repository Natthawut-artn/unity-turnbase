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

    public int TurnCycleCount { get; private set; } = 0;
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
        ServiceLocator.Get<SkillPointsHUD>().Hide();
        ServiceLocator.Get<TurnHUD>().Hide();
        List<GameObject> players = GameManager.Instance.Players_alive;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerController controller = players[i].GetComponent<PlayerController>();
            PlayerAttack attack = players[i].GetComponent<PlayerAttack>();
            if (i == 0)
            {
                controller.enabled = true;
                attack.enabled = true;
                MainCamera.Instance.SetPlayer(players[i].transform);
            }
            else
            {
                controller.enabled = false;
                attack.enabled = false;
            }
        }

        SFXManager.Instance.Play(SFX.Stages);
        GameManager.Instance.ReleasePlayers();

        Debug.Log("Stop turnbase");
    }

    public void EndTurn()
    {
        if (!isInTurnSystem) return;
        SFXManager.Instance.Play(SFX.Fight);
        if (currentPhase == Phase.Player)
        {
            playersPhaseCoroutine = StartCoroutine(PlayersPhase());
            ServiceLocator.Get<TurnHUD>().Show();
        }
        else
        {
            enemyPhaseCoroutine = StartCoroutine(EnemyPhase());
            ServiceLocator.Get<TurnHUD>().Show();
        }
    }

    public IEnumerator EnemyPhase()
    {
        Debug.Log("Enemy Phase started");

        // --- OLD CODE (Causes Crash) ---
        // EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        // foreach (EnemyAI enemy in enemies) 
        // {
        //     EnemyData enemyData = enemy.GetComponent<EnemyData>();
        // --- END OLD CODE ---

        // --- NEW CODE (Defensive Loop) ---
        List<GameObject> enemies = GameManager.Instance.Enemys_alive;
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemyGO = enemies[i];
            // CRITICAL: Check if the enemy object has been destroyed mid-turn
            if (enemyGO == null) continue;

            ApocalypeBloom bloomSkill = enemyGO.GetComponent<ApocalypeBloom>(); // Get component
            if (bloomSkill != null)
            {
                bloomSkill.DecrementCooldown(); // <-- DECREMENT CALL HERE
            }

            EnemyAI enemy = enemyGO.GetComponent<EnemyAI>();
            EnemyData enemyData = enemyGO.GetComponent<EnemyData>();

            // Double check components and data before proceeding
            if (enemy == null || enemyData == null) continue;
            // --- End NEW CODE ---
      
            // check frost debuff
            if (enemyData.isOnFrost)
            {
                enemyData.Frost();
            }
            else
            {
                yield return StartCoroutine(enemy.TakeTurn());
            }

            // check venom debuff
            if (enemyData.isOnVenom)
            {
                enemyData.Venom();
            }
            if (enemyData.isOnBurning)
            {
                enemyData.Burning();
            }
        }

        Debug.Log("Enemy Phase ended, switching to Player Phase");

        TurnCycleCount++;

        
        currentPhase = Phase.Player;

        EndTurn();
    }

    public IEnumerator PlayersPhase()
    {
        SkillPointsManager.ReceiveSkillPoint(1);

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
            PlayerData playerData = player.GetComponent<PlayerData>();
            controller.enabled = true;
            attack.enabled = true;

            if (playerData.isOnFrost)
            {
                playerData.Frost();
            }
            else
            {
                yield return StartCoroutine(controller.TakeTurn());
            }

            controller.enabled = false;
            attack.enabled = false;

            // check venom debuff
            if (playerData.isOnVenom)
            {
                playerData.Venom();
            }
            if (playerData.isOnBurning)
            {
                playerData.Burning();
            }
        }

        yield return null;


        yield return StartCoroutine(TriggerBossTurnEndSkills());

        currentPhase = Phase.Enemy;
        EndTurn();
    }

    private IEnumerator TriggerBossTurnEndSkills()
    {
        ApocalypeBloom[] bloomSkills = FindObjectsOfType<ApocalypeBloom>();
        foreach (ApocalypeBloom bloomSkill in bloomSkills)
        {
            if (bloomSkill != null && bloomSkill.enabled && bloomSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = bloomSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks cooldown internally
                    yield return StartCoroutine(bloomSkill.ExecuteSkill()); // <-- EXECUTE CALL HERE and WAIT
                }
            }
        }


        MultipleSlash[] slashSkills = FindObjectsOfType<MultipleSlash>();

        foreach (MultipleSlash slashSkill in slashSkills)
        {
            // Check if the enemy is alive and the skill component exists
            if (slashSkill != null && slashSkill.enabled && slashSkill.gameObject.activeInHierarchy)
            {
                // Get the EnemyData/CharactorInheritance to check HP
                CharactorInheritance stats = slashSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // Start the skill's check coroutine. 
                    // The skill itself handles range checking and execution.
                    yield return StartCoroutine(slashSkill.CheckAndExecuteSkill());
                    Debug.Log($"TurnManager triggered CheckAndExecuteSkill for {slashSkill.gameObject.name}");
                }
            }
        }
        // Find the boss (assuming it has BossData)
        BossData bossData = FindObjectOfType<BossData>();

        if (bossData != null)
        {
            // Now, we specifically check for the TENTACLE skill
            BossSkill_TentacleSpawn tentacleSkill = bossData.GetComponent<BossSkill_TentacleSpawn>();

            if (tentacleSkill != null)
            {
                // This call advances the 2-turn cooldown and checks HP.
                tentacleSkill.CheckSkillTrigger();
            }
        }
    }
}