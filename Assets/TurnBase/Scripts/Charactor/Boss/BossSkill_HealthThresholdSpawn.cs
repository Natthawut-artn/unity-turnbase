using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_HealthThresholdSpawn : MonoBehaviour
{
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    // Activation threshold is handled by the other script, but we keep the header for context.
    [Tooltip("The name of the room the boss is currently in.")]
    public string BossRoomName = "BossRoom";

    [Tooltip("Time delay between skill checks to prevent spamming.")]
    public float SkillCooldownTime = 2f; // Re-introduced cooldown for the respawn loop

    private BossData bossEnemyData;

    // --- Enemy Spawn Settings ---
    [Header("Enemy Spawn Settings")]
    [Tooltip("The number of enemies to spawn, chosen randomly between this min and max (inclusive).")]
    public int MinSpawnCount = 5;
    public int MaxSpawnCount = 8;

    public string EnemyPrefabPath = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    // --- Private State ---
    private GameObject targetRoom;
    private bool _hasTriggered = false;
    private List<GameObject> _spawnedMinions = new List<GameObject>(); // RE-INTRODUCED MINION TRACKING

    // This runs the moment the original script sets BossSkill_HealthThresholdSpawn.enabled = true;
    void OnEnable()
    {
        if (_hasTriggered) return;

        // 1. DISABLE PREVIOUS SKILL
        BossSkill_EnemySpawn previousSkill = GetComponent<BossSkill_EnemySpawn>();
        if (previousSkill != null && previousSkill.enabled)
        {
            previousSkill.enabled = false;
            Debug.Log("BossSkill_EnemySpawn has been **DISABLED** by OnEnable of Phase Two script.");
        }

        // 2. EXECUTE THE INITIAL PHASE CHANGE SPAWN
        ExecuteSkill(true); // 'true' for the initial large spawn
        _hasTriggered = true;

        // 3. START THE CONTINUOUS RESPAWN LOOP
        StartCoroutine(SkillLoop());
    }

    void Start()
    {
        bossEnemyData = GetComponent<BossData>();
        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_HealthThresholdSpawn: Boss is missing the BossData component!");
        }

        // We must start the setup coroutine, even if the script is disabled, 
        // to ensure targetRoom is ready when OnEnable fires later.
        StartCoroutine(WaitForBoardManagerAndSetup());
    }

    private IEnumerator WaitForBoardManagerAndSetup()
    {
        while (BoardManager.Instance == null)
            yield return null;

        targetRoom = BoardManager.Instance.GetRoom(BossRoomName);
        if (targetRoom == null)
        {
            Debug.LogError($"BossSkill_HealthThresholdSpawn: Room '{BossRoomName}' not found.");
        }
    }

    // *** NEW: The continuous loop that checks if enemies need to be respawned ***
    private IEnumerator SkillLoop()
    {
        // Give a brief delay after the initial massive spawn before checking again
        yield return new WaitForSeconds(SkillCooldownTime);

        while (true)
        {
            yield return new WaitForSeconds(SkillCooldownTime);

            if (targetRoom == null) continue;

            // 1. Clean up the list of destroyed minions
            CleanupMinionList();

            int currentMinionCount = _spawnedMinions.Count;

            // The condition to respawn is: if there are no enemies left (count is 0)
            bool isRespawnNeeded = currentMinionCount <= 0;

            if (isRespawnNeeded)
            {
                Debug.Log($"Minion count is zero ({currentMinionCount}). Triggering Phase Two respawn skill.");
                // 'false' indicates this is a respawn, not the initial phase-change trigger.
                ExecuteSkill(false);
            }
        }
    }

    /// <summary>
    /// Removes null entries from the tracking list (enemies that were killed).
    /// </summary>
    private void CleanupMinionList()
    {
        _spawnedMinions.RemoveAll(minion => minion == null);
    }

    /// <summary>
    /// Executes the enemy spawning skill, applying self-damage only on initial phase trigger.
    /// </summary>
    /// <param name="isInitialTrigger">True if this is the one-time phase activation.</param>
    public void ExecuteSkill(bool isInitialTrigger)
    {
       
       
            ApplySelfDamage();
       

        // Safety check to ensure targetRoom is ready
        if (targetRoom == null)
        {
            Debug.LogError("ExecuteSkill called but targetRoom is null. Skipping spawn.");
            return;
        }

        int enemiesToSpawn = Random.Range(MinSpawnCount, MaxSpawnCount + 1);
        GameObject enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);

        // ... (Rest of the instantiation logic) ...
        // ... (similar to the original script) ...

        int successfullySpawned = 0;
        for (int i = 0; i < enemiesToSpawn && i < SpawnPositions.Count; i++)
        {
            EnemySpawnData spawnData = SpawnPositions[i];
            Vector2 roundedGrid = new Vector2(Mathf.Round(spawnData.GridPosition.x), Mathf.Round(spawnData.GridPosition.y));
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(roundedGrid, targetRoom);

            if (worldPos == Vector3.zero) continue;

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // TRACK THE MINION
            _spawnedMinions.Add(enemy);
            if (GameManager.Instance != null) GameManager.Instance.Enemys_alive.Add(enemy);

            SetupMinion(enemy, spawnData.GridPosition);
            successfullySpawned++;
        }

        Debug.Log($"Phase Two spawned {successfullySpawned} enemies. (Initial Trigger: {isInitialTrigger})");

        
    }

    private void ApplySelfDamage()
    {
        if (bossEnemyData == null) return;
        int damageAmount = Mathf.CeilToInt(bossEnemyData.HP * 0.1f);
        bossEnemyData.TakeDamage(damageAmount);
        Debug.Log($"Boss took {damageAmount} damage for Phase Two activation.");
    }

    private void SetupMinion(GameObject enemy, Vector2 gridPosition)
    {
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.parentRoom = targetRoom.GetComponent<Room>();

        EnemyData enemyData = enemy.GetComponent<EnemyData>();
        if (enemyData != null)
        {
            enemyData.GridPosition = gridPosition;
            enemyData.currentHP = enemyData.HP;
        }
    }

    private void OnDestroy()
    {
        // 1. Ensure the list is cleaned up before checking
        CleanupMinionList();
        
        if (_spawnedMinions.Count > 0)
        {
            Debug.Log($"Boss destroyed. Clearing {_spawnedMinions.Count} active minions.");

            // 2. Iterate through the remaining minions and destroy them
            foreach (GameObject minion in _spawnedMinions)
            {
                if (minion != null)
                {
                    // Optionally, you might want a graceful death/destruction method on the minion instead of Destroy()
                    // For now, we use Destroy() to instantly remove them.
                    Destroy(minion);
                }
            }

            // 3. Clear the list after destruction is initiated
            _spawnedMinions.Clear();
        }

        
    }
}