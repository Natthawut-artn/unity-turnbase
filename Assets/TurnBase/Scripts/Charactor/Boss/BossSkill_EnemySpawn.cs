using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkill_EnemySpawn : MonoBehaviour
{
    public Animator animator;
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    public float ActivationRange = 10f;
    public string BossRoomName = "BossRoom";
    

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

    private Transform PlayerTransform;

    
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
        while (true) // Loop indefinitely
        {
            yield return new WaitForSeconds(SkillCooldownTime); // Wait for cooldown

            // --- FIX: Dynamically find Player if reference is lost ---
            if (PlayerTransform == null)
            {
                // Attempt to find the player GameObject using its tag
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    // If found, update the reference
                    PlayerTransform = playerObj.transform;
                    Debug.Log("BossSkill_EnemySpawn re-acquired PlayerTransform.");
                }
                else
                {
                    // If STILL no player found (e.g., game over, scene transition)
                    Debug.LogWarning("BossSkill_EnemySpawn: No active player found. Pausing spawn checks.");
                    continue; // Skip the rest of this loop iteration and try again next cooldown
                }
            }
            // --- END FIX ---


            // Check other essential conditions (room ready, initial cooldown passed)
            if (targetRoom == null || _onInitialCooldown)
            {
                continue;
            }

            // --- PHASE TWO ACTIVATION CHECK ---
            // (Check if boss HP is below 70% and transition if needed)
            if (!_isPhaseTwoReady && bossEnemyData != null && HealthThresholdSkill != null)
            {
                float activationHealth = bossEnemyData.HP * 0.7f;
                if (bossEnemyData.currentHP <= activationHealth)
                {
                    ClearSpawnedMinions();          // Clear existing minions
                    HealthThresholdSkill.enabled = true; // Enable the Phase 2 script
                    Debug.Log("Boss HP below 70%. Transitioning to Phase Two.");
                    _isPhaseTwoReady = true;
                    StopCoroutine(SkillLoop()); // Stop this Phase 1 loop
                    yield break; // Exit this coroutine permanently
                }
            }

            // --- PHASE ONE RESPAWN LOGIC ---
            CleanupMinionList(); // Remove any destroyed minions from the tracking list

            // Check distance to the (now confirmed non-null) player
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
            int currentMinionCount = _spawnedMinions.Count; // Get count AFTER cleanup

            bool isPlayerInRange = distanceToPlayer <= ActivationRange;
            bool isRespawnNeeded = currentMinionCount <= 1; // Respawn if 0 or 1 minions remain

            // Initial spawn trigger (only happens once when player enters range)
            if (!_isInitialTriggered && isPlayerInRange)
            {
                _isInitialTriggered = true;
                ExecuteSkill(); // Spawn initial wave
            }
            // Respawn trigger (happens repeatedly if minions are killed)
            else if (_isInitialTriggered && isRespawnNeeded)
            {
                Debug.Log($"Minion count low ({currentMinionCount}). Triggering respawn.");
                ExecuteSkill(); // Spawn reinforcement wave
                
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
        SFXManager.Instance.PlayOneShot(SFX.SpawnFX);
        StartCoroutine(SpawnAnim());
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
        

    }

    private void ApplySelfDamage()
    {
        if (bossEnemyData == null) return;
        int damageAmount = Mathf.CeilToInt(bossEnemyData.HP * 0.05f);
        bossEnemyData.BossTakeDamage(damageAmount);
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
    IEnumerator SpawnAnim()
    {
        animator.SetTrigger("Skill1");
        yield return new WaitForSeconds(3.5f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ActivationRange);
    }
}