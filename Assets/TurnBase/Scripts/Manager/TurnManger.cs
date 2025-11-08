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
    public bool isTurnBuff = false;
    public int TurnBuffAmount = 0;

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
        ServiceLocator.Get<SkillPointsHUD>().Hide();
        List<GameObject> enemies = GameManager.Instance.Enemys_alive;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (i >= enemies.Count) continue; // List size might have changed
            GameObject enemyGO = enemies[i];
            if (enemyGO == null) continue;

            EnemyAI enemyAI = enemyGO.GetComponent<EnemyAI>();
            EnemyData enemyData = enemyGO.GetComponent<EnemyData>();
            if (enemyAI == null || enemyData == null) continue;
            
            ApocalypeBloom bloomSkill = enemyGO.GetComponent<ApocalypeBloom>(); // Get component
            if (bloomSkill != null)
            {
                bloomSkill.DecrementCooldown(); // <-- DECREMENT CALL HERE
            }
            // check frost debuff
            if (enemyData.isOnFrost)
            {
                enemyData.Frost();
            }
            else
            {
                yield return StartCoroutine(enemyAI.TakeTurn());
            }
            if (enemyGO == null)
            {
                Debug.LogWarning("Enemy was destroyed during TakeTurn(). Skipping rest of its phase.");
                continue; // Go to the next enemy in the loop
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

        for (int i = 0; i < GameManager.Instance.Players_alive.Count; i++)
        {
            GameObject player = GameManager.Instance.Players_alive[i];
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
        if (isTurnBuff)
        {
            TurnBuffAmount -= 1;
            if (TurnBuffAmount <= 0)
            {
                isTurnBuff = false;
                SkillPointsManager.maxSkillPoint -= 2;
            }
        }

        yield return null;


        yield return StartCoroutine(TriggerBossTurnEndSkills());

        currentPhase = Phase.Enemy;
        EndTurn();
    }

    
    private IEnumerator TriggerBossTurnEndSkills()
    {
        
        CrimsonDevourer[] devourerSkills = FindObjectsOfType<CrimsonDevourer>();
        foreach (CrimsonDevourer devourSkill in devourerSkills)
        {
            if (devourSkill != null && devourSkill.enabled && devourSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = devourSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, player count, SP) internally
                    yield return StartCoroutine(devourSkill.CheckAndExecuteSkill());
                }
            }
        }
        
        BloodveilSaber[] saberSkills = FindObjectsOfType<BloodveilSaber>();
        foreach (BloodveilSaber saberSkill in saberSkills)
        {
            if (saberSkill != null && saberSkill.enabled && saberSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = saberSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, player count, SP) internally
                    yield return StartCoroutine(saberSkill.CheckAndExecuteSkill());
                }
            }
        }
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
        BloodClawDominion[] clawSkills = FindObjectsOfType<BloodClawDominion>();
        foreach (BloodClawDominion clawSkill in clawSkills)
        {
            if (clawSkill != null && clawSkill.enabled && clawSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = clawSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, player count, SP) internally
                    yield return StartCoroutine(clawSkill.CheckAndExecuteSkill());
                }
            }
        }
        CrimsonBeam[] beamSkills = FindObjectsOfType<CrimsonBeam>();
        foreach (CrimsonBeam beamSkill in beamSkills)
        {
            if (beamSkill != null && beamSkill.enabled && beamSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = beamSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, player count, SP) internally
                    yield return StartCoroutine(beamSkill.CheckAndExecuteSkill());
                }
            }
        }
        ScreamoftheVampireLord[] screamSkills = FindObjectsOfType<ScreamoftheVampireLord>();
        foreach (ScreamoftheVampireLord screamSkill in screamSkills)
        {
            if (screamSkill != null && screamSkill.enabled && screamSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = screamSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, SP) internally
                    yield return StartCoroutine(screamSkill.CheckAndExecuteSkill());
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
        WorldBreaker[] breakerSkills = FindObjectsOfType<WorldBreaker>();
        foreach (WorldBreaker breakerSkill in breakerSkills)
        {
            if (breakerSkill != null && breakerSkill.enabled && breakerSkill.gameObject.activeInHierarchy)
            {
                CharactorInheritance stats = breakerSkill.GetComponent<CharactorInheritance>();
                if (stats != null && stats.currentHP > 0)
                {
                    // ExecuteSkill checks all conditions (range, player count, SP) internally
                    yield return StartCoroutine(breakerSkill.CheckAndExecuteSkill());
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
        yield return null;
    }
}