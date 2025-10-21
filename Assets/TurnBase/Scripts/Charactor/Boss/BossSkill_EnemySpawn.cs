using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_EnemySpawn : MonoBehaviour
{
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    public float ActivationRange = 10f;
    public string BossRoomName = "BossRoom";
    public Transform PlayerTransform;

    [Tooltip("Reference to the script that handles the 70% HP phase change.")]
    [Header("Phase Change Reference")]
    public BossSkill_HealthThresholdSpawn HealthThresholdSkill;

    private BossData bossEnemyData;

    // --- Enemy Spawn Settings ---
    // ... (rest of existing fields) ...
    [Header("Enemy Spawn Settings")]
    public int MinSpawnCount = 3;
    public int MaxSpawnCount = 5;
    public float SkillCooldownTime = 2f;

    public string EnemyPrefabPath = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    // --- Private State ---
    private GameObject targetRoom;
    private List<GameObject> _spawnedMinions = new List<GameObject>(); // Tracks minions from THIS phase
    private bool _onInitialCooldown = true;
    private bool _isInitialTriggered = false;
    private bool _isPhaseTwoReady = false;

    void Start()
    {
        bossEnemyData = GetComponent<BossData>();
        HealthThresholdSkill = GetComponent<BossSkill_HealthThresholdSpawn>();

        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_EnemySpawn: Boss is missing the BossData component!");
        }

        if (PlayerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) PlayerTransform = playerObj.transform;
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
            Debug.LogError($"BossSkill_EnemySpawn: Room '{BossRoomName}' not found.");
        }

        yield return new WaitForSeconds(1f);
        _onInitialCooldown = false;

        StartCoroutine(SkillLoop());
    }

    private IEnumerator SkillLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(SkillCooldownTime);

            if (PlayerTransform == null || targetRoom == null || _onInitialCooldown)
            {
                continue;
            }

            // 1. PHASE TWO ACTIVATION CHECK
            if (!_isPhaseTwoReady && bossEnemyData != null && HealthThresholdSkill != null)
            {
                float activationHealth = bossEnemyData.HP * 0.7f;

                if (bossEnemyData.currentHP <= activationHealth)
                {
                    // 1a. NEW: Clear all enemies spawned by this script
                    ClearSpawnedMinions();

                    // 1b. Enable the new script, which disables THIS script in its OnEnable.
                    HealthThresholdSkill.enabled = true;
                    Debug.Log("Boss HP is below 70%. **Cleared Phase 1 minions** and enabling Phase Two skill.");

                    _isPhaseTwoReady = true;
                    yield break;
                }
            }

            // 2. ORIGINAL PHASE ONE LOGIC (Respawn Check)
            CleanupMinionList();

            float distanceToPlayer = Vector3.Distance(this.transform.position, PlayerTransform.position);
            int currentMinionCount = _spawnedMinions.Count;

            bool isPlayerInRange = distanceToPlayer <= ActivationRange;
            bool isRespawnNeeded = currentMinionCount <= 1;

            if (!_isInitialTriggered && isPlayerInRange)
            {
                _isInitialTriggered = true;
                ExecuteSkill();
            }
            else if (_isInitialTriggered && isRespawnNeeded)
            {
                Debug.Log($"Minion count is low ({currentMinionCount}). Triggering respawn skill.");
                ExecuteSkill();
            }
        }
    }

    /// <summary>
    /// Destroys all currently tracked minions and clears the list. (NEW FUNCTION)
    /// </summary>
    public void ClearSpawnedMinions()
    {
        CleanupMinionList(); // First, remove nulls to ensure we only target active enemies

        int clearedCount = 0;
        foreach (GameObject minion in _spawnedMinions)
        {
            if (minion != null)
            {
                // Optional: Also remove from GameManager global list if needed
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Enemys_alive.Remove(minion);
                }

                Destroy(minion);
                clearedCount++;
            }
        }
        _spawnedMinions.Clear();
        Debug.Log($"Phase 1 Cleanup: Destroyed {clearedCount} minions.");
    }


    // --- Existing Helper Methods (ExecuteSkill, CleanupMinionList, etc.) below ---

    private void CleanupMinionList()
    {
        _spawnedMinions.RemoveAll(minion => minion == null);
    }

    public void ExecuteSkill()
    {
        ApplySelfDamage();
        int enemiesToSpawn = Random.Range(MinSpawnCount, MaxSpawnCount + 1);
        GameObject enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null) { Debug.LogError($"Could not load prefab from '{EnemyPrefabPath}'"); return; }

        int successfullySpawned = 0;
        for (int i = 0; i < enemiesToSpawn && i < SpawnPositions.Count; i++)
        {
            EnemySpawnData spawnData = SpawnPositions[i];
            Vector2 roundedGrid = new Vector2(Mathf.Round(spawnData.GridPosition.x), Mathf.Round(spawnData.GridPosition.y));
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(roundedGrid, targetRoom);

            if (worldPos == Vector3.zero) continue;

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            _spawnedMinions.Add(enemy); // Add to the tracking list
            if (GameManager.Instance != null) GameManager.Instance.Enemys_alive.Add(enemy);

            SetupMinion(enemy, spawnData.GridPosition);
            successfullySpawned++;
        }

        if (TurnManager.Instance != null) TurnManager.Instance.EndTurn();
    }

    private void ApplySelfDamage()
    {
        if (bossEnemyData == null) return;
        int damageAmount = Mathf.CeilToInt(bossEnemyData.HP * 0.05f);
        bossEnemyData.TakeDamage(damageAmount);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ActivationRange);
    }
}