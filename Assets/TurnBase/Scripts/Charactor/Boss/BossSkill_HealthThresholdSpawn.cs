using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_HealthThresholdSpawn : MonoBehaviour
{
    public Animator animator;
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    [Tooltip("The name of the room the boss is currently in.")]
    public string BossRoomName = "BossRoom";

    [Tooltip("Time delay between skill checks to prevent spamming.")]
    public float SkillCooldownTime = 2f;

    private BossData bossEnemyData;

    // --- Enemy Spawn Settings ---
    [Header("Enemy Spawn Settings")]
    [Tooltip("The number of enemies to spawn, chosen randomly between this min and max (inclusive).")]
    public int MinSpawnCount = 5;
    public int MaxSpawnCount = 8;

    [Tooltip("List of possible enemy prefab resource paths (e.g., 'Prefabs/Charactor/Enemy/Slime').")]
    public List<string> EnemyPrefabPaths = new List<string>(); // CHANGED to list
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    // --- Private State ---
    private GameObject targetRoom;
    private bool _hasTriggered = false;
    private List<GameObject> _spawnedMinions = new List<GameObject>();

    void OnEnable()
    {
        if (_hasTriggered) return;

        BossSkill_EnemySpawn previousSkill = GetComponent<BossSkill_EnemySpawn>();
        if (previousSkill != null && previousSkill.enabled)
        {
            previousSkill.enabled = false;
            Debug.Log("BossSkill_EnemySpawn has been **DISABLED** by OnEnable of Phase Two script.");
        }

        ExecuteSkill(true);
        _hasTriggered = true;

        StartCoroutine(SkillLoop());
    }

    void Start()
    {
        bossEnemyData = GetComponent<BossData>();
        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_HealthThresholdSpawn: Boss is missing the BossData component!");
        }

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

    private IEnumerator SkillLoop()
    {
        yield return new WaitForSeconds(SkillCooldownTime);

        while (true)
        {
            yield return new WaitForSeconds(SkillCooldownTime);

            if (targetRoom == null) continue;

            CleanupMinionList();

            int currentMinionCount = _spawnedMinions.Count;

            bool isRespawnNeeded = currentMinionCount <= 0;

            if (isRespawnNeeded)
            {
                Debug.Log($"Minion count is zero ({currentMinionCount}). Triggering Phase Two respawn skill.");
                ExecuteSkill(false);
            }
        }
    }

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
        if (EnemyPrefabPaths == null || EnemyPrefabPaths.Count == 0)
        {
            Debug.LogError("EnemyPrefabPaths list is empty! Cannot spawn minions.");
            return;
        }

        StartCoroutine(SpawnAnim());
        ApplySelfDamage();
        

        if (targetRoom == null)
        {
            Debug.LogError("ExecuteSkill called but targetRoom is null. Skipping spawn.");
            return;
        }

        int enemiesToSpawn = Random.Range(MinSpawnCount, MaxSpawnCount + 1);
        int successfullySpawned = 0;

        for (int i = 0; i < enemiesToSpawn && i < SpawnPositions.Count; i++)
        {
            // --- FIX: Randomly select a prefab path ---
            string randomPath = EnemyPrefabPaths[Random.Range(0, EnemyPrefabPaths.Count)];
            GameObject enemyPrefab = Resources.Load<GameObject>(randomPath);

            if (enemyPrefab == null)
            {
                Debug.LogError($"Could not load prefab from '{randomPath}'. Skipping spawn for this slot.");
                continue;
            }
            // --- END FIX ---

            EnemySpawnData spawnData = SpawnPositions[i];
            Vector2 roundedGrid = new Vector2(Mathf.Round(spawnData.GridPosition.x), Mathf.Round(spawnData.GridPosition.y));
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(roundedGrid, targetRoom);

            if (worldPos == Vector3.zero) continue;

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

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
        bossEnemyData.BossTakeDamage(damageAmount);
        Debug.Log($"Boss took {damageAmount} damage for Phase Two activation.");
    }

    private void SetupMinion(GameObject enemy, Vector2 gridPosition)
    {
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null) ai.parentRoom = targetRoom.GetComponent<Room>();

        // FIX: The Minion Type Name (for the decider) must now be derived from the prefab's identity
        // Since we don't have a MinionTypeName field, we use the simple name of the prefab for now.
        // You may need to refine this based on your actual enemy naming/tagging system.
        EnemySkillDecider skillDecider = enemy.GetComponent<EnemySkillDecider>();
        if (skillDecider != null)
        {
            // We strip any clone markers (" (Clone)") and use the prefab's base name as the type
            string prefabName = enemy.name.Replace("(Clone)", "").Trim();
            // Often prefabs are named like "Goblin_Enemy", but the type is just "Goblin".
            // You might need a more custom lookup here, but using the full prefab name is a start.
            skillDecider.EnemyType = prefabName;
            Debug.Log($"Assigned Minion Type '{prefabName}' to {enemy.name}");
        }

        EnemyData enemyData = enemy.GetComponent<EnemyData>();
        if (enemyData != null)
        {
            enemyData.GridPosition = gridPosition;
            enemyData.currentHP = enemyData.HP;
        }
    }

    private void OnDestroy()
    {
        CleanupMinionList();

        if (_spawnedMinions.Count > 0)
        {
            Debug.Log($"Boss destroyed. Clearing {_spawnedMinions.Count} active minions.");

            foreach (GameObject minion in _spawnedMinions)
            {
                if (minion != null)
                {
                    Destroy(minion);
                }
            }
            _spawnedMinions.Clear();
        }
    }
    IEnumerator SpawnAnim()
    {
        animator.SetTrigger("Skill2");
        yield return new WaitForSeconds(3.5f);
    }
}
