using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_EnemySpawn : MonoBehaviour
{
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    [Tooltip("The range (in tiles/units) the player must be in for the skill to initially trigger.")]
    public float ActivationRange = 10f;
    [Tooltip("The name of the room the boss is currently in.")]
    public string BossRoomName = "BossRoom";
    [Tooltip("Reference to the player's transform.")]
    public Transform PlayerTransform;

    [Tooltip("The boss component handling HP and damage. Should be attached to the same GameObject.")]
    private BossData bossEnemyData; // Assumed component on the boss

    // --- Enemy Spawn Settings ---
    [Header("Enemy Spawn Settings")]
    [Tooltip("The number of enemies to spawn, chosen randomly between this min and max (inclusive).")]
    public int MinSpawnCount = 3;
    public int MaxSpawnCount = 5;
    public float SkillCooldownTime = 2f; // Time delay between skill checks to prevent spamming

    public string EnemyPrefabPath = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    // --- Private State ---
    private GameObject targetRoom;
    private List<GameObject> _spawnedMinions = new List<GameObject>();
    private bool _isSkillReady = false;
    private bool _onInitialCooldown = true; // Use this to prevent spawning immediately
    private bool _isInitialTriggered = false; // Tracks if the player has entered the range at least once

    void Start()
    {
        // Get the boss's own health data component
        bossEnemyData = GetComponent<BossData>();
        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_EnemySpawn: Boss is missing the EnemyData component! Self-damage will not work.");
        }

        // Safety check: Try to find the player if not set
        if (PlayerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                PlayerTransform = playerObj.transform;
            }
        }

        // Wait for BoardManager to be ready and get the room reference
        StartCoroutine(WaitForBoardManagerAndSetup());
    }

    private IEnumerator WaitForBoardManagerAndSetup()
    {
        while (BoardManager.Instance == null)
            yield return null;

        targetRoom = BoardManager.Instance.GetRoom(BossRoomName);
        if (targetRoom == null)
        {
            Debug.LogError($"BossSkill_EnemySpawn: Room '{BossRoomName}' not found in BoardManager. Boss skill won't work.");
        }

        // Start the continuous skill loop after setup
        StartCoroutine(SkillLoop());
    }

    /// <summary>
    /// The main coroutine that continuously manages the skill activation and respawn condition.
    /// </summary>
    private IEnumerator SkillLoop()
    {
        // Initial wait to ensure all managers are set up before starting checks
        yield return new WaitForSeconds(1f);
        _onInitialCooldown = false;

        while (true)
        {
            yield return new WaitForSeconds(SkillCooldownTime);

            if (PlayerTransform == null || targetRoom == null || _onInitialCooldown)
            {
                continue;
            }

            // 1. Clean up the list of destroyed minions
            CleanupMinionList();

            // 2. Check Activation Condition (Initial Trigger OR Respawn Condition)
            float distanceToPlayer = Vector3.Distance(this.transform.position, PlayerTransform.position);
            int currentMinionCount = _spawnedMinions.Count;

            bool isPlayerInRange = distanceToPlayer <= ActivationRange;
            bool isRespawnNeeded = currentMinionCount <= 1;

            if (!_isInitialTriggered && isPlayerInRange)
            {
                // Condition A: Initial skill trigger when player enters range
                _isInitialTriggered = true;
                ExecuteSkill();
            }
            else if (_isInitialTriggered && isRespawnNeeded)
            {
                // Condition B: Respawn check after initial trigger
                Debug.Log($"Minion count is low ({currentMinionCount}). Triggering respawn skill.");
                ExecuteSkill();
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
    /// Executes the enemy spawning skill, applying self-damage to the boss.
    /// </summary>
    public void ExecuteSkill()
    {
        // Apply 5% self-damage first
        ApplySelfDamage();

        // Determine random spawn count (Max is exclusive in this version of Random.Range for integers)
        int enemiesToSpawn = Random.Range(MinSpawnCount, MaxSpawnCount + 1);

        GameObject enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null)
        {
            Debug.LogError($"BossSkill_EnemySpawn: Could not load enemy prefab from '{EnemyPrefabPath}'");
            return;
        }

        int successfullySpawned = 0;

        // Loop up to the number of enemies we want to spawn
        for (int i = 0; i < enemiesToSpawn && i < SpawnPositions.Count; i++)
        {
            EnemySpawnData spawnData = SpawnPositions[i];

            // Snap to nearest whole grid
            Vector2 roundedGrid = new Vector2(Mathf.Round(spawnData.GridPosition.x), Mathf.Round(spawnData.GridPosition.y));
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(roundedGrid, targetRoom);

            if (worldPos == Vector3.zero)
            {
                Debug.LogWarning($"BossSkill_EnemySpawn: No marker found at {roundedGrid} at index {i}. Skipping.");
                continue;
            }

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = $"BossSpawnedMinion_{roundedGrid.x}_{roundedGrid.y}";

            // Track the newly spawned minion locally and globally
            _spawnedMinions.Add(enemy);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Enemys_alive.Add(enemy);
            }

            // --- Post-Spawn Setup (Based on your reference) ---
            // Note: Use 'i' to ensure unique grid positions are used if available in the list
            SetupMinion(enemy, spawnData.GridPosition);
            successfullySpawned++;
        }

        Debug.Log($"Boss spawned {successfullySpawned} enemies and took self-damage.");

        // Optional: Trigger a turn system update 
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.isInTurnSystem = true;
            TurnManager.Instance.EndTurn();
        }
    }

    /// <summary>
    /// Applies 5% damage to the boss based on its current health.
    /// </summary>
    private void ApplySelfDamage()
    {
        if (bossEnemyData == null) return;

        // Calculate 5% of the current health and round up to the nearest integer (for integer damage systems).
        // Mathf.CeilToInt ensures the damage is a whole number and the boss takes at least 1 damage.
        int damageAmount = Mathf.CeilToInt(bossEnemyData.currentHP * 0.05f);

        // Assuming EnemyData has a method like TakeDamage(int amount)
        bossEnemyData.TakeDamage(damageAmount);

        // Update Debug log to reflect the integer damage
        Debug.Log($"Boss took {damageAmount} damage (5% of current HP, rounded up) from using spawn skill.");
    }

    /// <summary>
    /// Sets up the EnemyAI and EnemyData components on the newly spawned minion.
    /// </summary>
    private void SetupMinion(GameObject enemy, Vector2 gridPosition)
    {
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.parentRoom = targetRoom.GetComponent<Room>();
        }

        EnemyData enemyData = enemy.GetComponent<EnemyData>();
        if (enemyData != null)
        {
            enemyData.GridPosition = gridPosition;
            enemyData.currentHP = enemyData.HP;
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize the activation range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ActivationRange);
    }
}
