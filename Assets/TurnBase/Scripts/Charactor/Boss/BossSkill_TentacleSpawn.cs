using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BossSkill_TentacleSpawn : MonoBehaviour
{
    // --- Skill Activation & Boss Setup ---
    [Header("Skill Activation & Boss Setup")]
    [Tooltip("The name of the room the boss is currently in.")]
    public string BossRoomName = "BossRoom";

    [Tooltip("The boss's HP threshold (as a percentage) to enable the skill.")]
    public float HpThreshold = 0.50f; // 50%

    [Tooltip("The number of player turns to wait between spawns.")]
    public int PlayerTurnCooldown = 2; // This is the cooldown you requested (2 player turns)

    private BossData bossEnemyData;

    // --- Tentacle Spawn Settings ---
    [Header("Tentacle Spawn Settings")]
    [Tooltip("The prefab path for the Tentacle object.")]
    public string TentaclePrefabPath = "Prefabs/Charactor/Enemy/Tentacle_Variant";

    [Tooltip("The maximum number of tentacles to spawn in one activation.")]
    public int MaxTentaclesToSpawn = 3;

    [Tooltip("Offset applied to the world position from the grid cell center.")]
    public Vector3 SpawnOffset = new Vector3(0f, 0f, 0f);

    // --- Private State ---
    private Room targetRoomComponent;
    private int _turnsSinceLastSpawn = 0; // Tracks the cooldown
    private bool _hasSetup = false;
    private bool _isSpawning = false;

    void Start()
    {
        bossEnemyData = GetComponent<BossData>();

        if (bossEnemyData == null)
        {
            Debug.LogError("BossSkill_TentacleSpawn: Boss is missing the BossData component!");
        }

        // Start the room setup process
        StartCoroutine(WaitForBoardManagerAndSetup());
    }

    private IEnumerator WaitForBoardManagerAndSetup()
    {
        while (BoardManager.Instance == null)
            yield return null;

        GameObject targetRoomGO = BoardManager.Instance.GetRoom(BossRoomName);
        if (targetRoomGO != null)
        {
            targetRoomComponent = targetRoomGO.GetComponent<Room>();
        }

        if (targetRoomComponent == null)
        {
            Debug.LogError($"BossSkill_TentacleSpawn: Room '{BossRoomName}' or its Room component not found.");
        }
        else
        {
            _hasSetup = true;
        }
    }

    /// <summary>
    /// **CALLED BY TURNMANAGER** at the end of the Player's turn.
    /// This is the only method that manages HP check and cooldown.
    /// </summary>
    public void CheckSkillTrigger()
    {
        if (!_hasSetup || _isSpawning || bossEnemyData == null || !enabled) return; // ONLY run if the script is enabled

        // 1. Check HP Threshold (50% or lower)
        float currentHPPercent = (float)bossEnemyData.currentHP / bossEnemyData.HP;
        if (currentHPPercent > HpThreshold)
        {
            // Reset turns if we go ABOVE the threshold (in case of healing), but usually just wait.
            // If the boss is at 70% (when this script enables), we wait until 50%
            Debug.Log($"Tentacle Skill Waiting. Boss HP: {currentHPPercent:P0}. Needs to be below {HpThreshold:P0}.");
            _turnsSinceLastSpawn = 0; // Prevent cooldown from building up prematurely
            return;
        }

        // --- HP IS 50% OR LOWER, START COOLDOWN COUNT ---

        // 2. Check Cooldown (2 Player Turns)
        _turnsSinceLastSpawn++;
        if (_turnsSinceLastSpawn < PlayerTurnCooldown)
        {
            Debug.Log($"Tentacle Spawn Cooldown: {_turnsSinceLastSpawn}/{PlayerTurnCooldown} player turns played.");
            return;
        }

        // 3. Execute Spawn
        Debug.Log("Boss HP is low and cooldown is complete. Triggering Tentacle Spawn.");
        ExecuteSkill();
        _turnsSinceLastSpawn = 0; // Reset cooldown
    }

    public void ExecuteSkill()
    {
        if (targetRoomComponent == null)
        {
            Debug.LogError("ExecuteSkill called but targetRoomComponent is null. Skipping spawn.");
            return;
        }

        _isSpawning = true;
        GameObject tentaclePrefab = Resources.Load<GameObject>(TentaclePrefabPath);
        // ... (Error checks for prefab and targetRoom) ...
        if (tentaclePrefab == null) { _isSpawning = false; return; }

        // --- NEW LOGIC: Filter UNOCUUPIED Markers ---

        // 1. Get all Marker components in the room
        Marker[] allMarkers = targetRoomComponent.GetComponentsInChildren<Marker>();

        // 2. Filter for UNOCUUPIED markers and get their Vector2 grid position
        List<Vector2> availableSpawnSpots = allMarkers
            .Where(m => m.IsOccupied == false)
            .Select(m => m.GridPosition)
            .ToList();

        // --- END NEW LOGIC ---

        if (availableSpawnSpots.Count == 0)
        {
            Debug.LogWarning("No unoccupied grid positions available for tentacle spawn.");
            _isSpawning = false;
            return;
        }

        // Calculate 25% of the room's total grid positions (using the AVAILABLE count)
        int totalAvailableCells = availableSpawnSpots.Count;
        int maxSpawnAreaCount = Mathf.CeilToInt(totalAvailableCells * 0.75f);
        int spawnCount = Mathf.Min(MaxTentaclesToSpawn, maxSpawnAreaCount);

        // 3. Randomly select from the *available* list
        List<Vector2> potentialSpawnSpots = new List<Vector2>(availableSpawnSpots);
        List<Vector2> selectedSpawnPositions = new List<Vector2>();

        for (int i = 0; i < spawnCount; i++)
        {
            if (potentialSpawnSpots.Count == 0) break;

            int randomIndex = Random.Range(0, potentialSpawnSpots.Count);
            Vector2 gridPos = potentialSpawnSpots[randomIndex];

            selectedSpawnPositions.Add(gridPos);
            potentialSpawnSpots.RemoveAt(randomIndex);
        }

        int successfullySpawned = 0;
        foreach (Vector2 gridPos in selectedSpawnPositions)
        {
            // Find the Marker GameObject (it must exist and be unoccupied since it came from the filtered list)
            GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(gridPos, targetRoomComponent.gameObject);

            // ... (Instantiation and initialization logic remains the same) ...
            if (markerGO == null) continue;

            Marker markerComponent = markerGO.GetComponent<Marker>();
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(gridPos, targetRoomComponent.gameObject);

            if (worldPos == Vector3.zero) continue;

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject tentacle = Instantiate(tentaclePrefab, spawnPos, Quaternion.identity);

            Tentacle tentacleScript = tentacle.GetComponent<Tentacle>();
            if (tentacleScript != null)
            {
                tentacleScript.Initialize(markerComponent);
                // OPTIONAL: Mark the tile as occupied by the tentacle/hazard during its lifecycle
                // This is generally handled within the Tentacle.cs lifecycle if you want other things
                // (like player movement) to avoid the tile while the tentacle is warning/active.

                successfullySpawned++;
            }
            else
            {
                Debug.LogError("Tentacle prefab is missing the Tentacle script!");
                Destroy(tentacle);
            }
        }

        Debug.Log($"Successfully spawned {successfullySpawned} tentacles. (Target: {spawnCount})");
        _isSpawning = false;
    }
}