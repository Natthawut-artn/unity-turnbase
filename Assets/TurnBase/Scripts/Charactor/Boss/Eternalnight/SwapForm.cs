using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning a Phase 2 Boss upon the death of this (Phase 1) boss.
/// It retrieves the "CrimsonDevourerStacks" from the BossPhaseManager
/// and applies them as a permanent ATK buff to the newly spawned boss.
/// </summary>
public class SwapForm : MonoBehaviour
{
    [Header("Phase 2 Settings")]
    [Tooltip("The resource path to the Phase 2 Boss prefab (e.g., 'Prefabs/Bosses/Boss_Phase2').")]
    public string PhaseTwoBossPrefabPath;

    // --- NEW FIELD ---
    [Tooltip("The EnemyType string for the Phase 2 boss (e.g., 'Boss_P2', 'VampireLord'). This MUST match the check in EnemySkillDecider.")]
    public string PhaseTwoEnemyType;
    // --- END NEW FIELD ---

    [Tooltip("The specific grid coordinate (X, Z) where the Phase 2 Boss will spawn.")]
    public Vector2 TargetGridPosition;

    [Tooltip("The vertical offset to apply to the spawn position to ensure the boss is above the grid.")]
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    private bool hasSwapped = false;
    private EnemyAI enemyAI; // Reference to this (P1) boss's AI

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogError($"SwapForm Error: {gameObject.name} is missing an EnemyAI component! Cannot determine spawn room.");
        }
    }

    /// <summary>
    /// This is called by the BossData.Die() method right before the Phase 1 Boss is destroyed.
    /// </summary>
    public void ExecutePhaseChange()
    {
        if (hasSwapped) return;
        if (BossPhaseManager.Instance == null)
        {
            Debug.LogError("SwapForm Error: BossPhaseManager.Instance is null!");
            return;
        }
        if (enemyAI == null || enemyAI.parentRoom == null)
        {
            Debug.LogError("SwapForm Error: EnemyAI or ParentRoom reference is missing!");
            return;
        }
        if (string.IsNullOrEmpty(PhaseTwoEnemyType))
        {
            Debug.LogWarning($"SwapForm Warning: PhaseTwoEnemyType is not set in the Inspector for {gameObject.name}. The P2 boss may not use skills.");
        }

        // Load Prefab
        GameObject prefabToSpawn = Resources.Load<GameObject>(PhaseTwoBossPrefabPath);
        if (prefabToSpawn == null)
        {
            Debug.LogError($"SwapForm Error: Could not load P2 prefab from path: {PhaseTwoBossPrefabPath}");
            return;
        }

        hasSwapped = true;

        // Determine Spawn Location
        GameObject targetRoomGO = enemyAI.parentRoom.gameObject;
        Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(TargetGridPosition, targetRoomGO);
        if (worldPos == Vector3.zero)
        {
            worldPos = transform.position; // Fallback to P1 boss location
        }
        Vector3 spawnPos = worldPos + SpawnOffset;
        Quaternion spawnRot = Quaternion.identity;

        // Spawn P2
        GameObject phase2GO = Instantiate(prefabToSpawn, spawnPos, spawnRot);
        phase2GO.name = $"{PhaseTwoEnemyType}_{TargetGridPosition.x}_{TargetGridPosition.y}"; // Sets name based on type
        Debug.Log($"<color=red>PHASE 2:</color> Spawned {phase2GO.name} at grid {TargetGridPosition}");

        // Add to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Enemys_alive.Add(phase2GO);
        }

        // Get P2 Stats
        CharactorInheritance phase2Stats = phase2GO.GetComponent<CharactorInheritance>();
        if (phase2Stats == null)
        {
            Debug.LogError($"SwapForm Error: {phase2GO.name} is missing a CharactorInheritance component!");
            return;
        }

        // Apply Crimson Stack Buff
        int stacks = BossPhaseManager.Instance.CrimsonDevourerStacks;
        float bonusPercentage = BossPhaseManager.Instance.GetDevourerDamageBonus();
        int baseAtk = phase2Stats.ATK;
        int damageBonus = Mathf.RoundToInt(baseAtk * bonusPercentage);
        phase2Stats.ATK += damageBonus;
        phase2Stats.currentATK += damageBonus;
        Debug.Log($"<color=red>PHASE 2 BUFF:</color> {phase2GO.name} gained +{damageBonus} ATK from {stacks} stacks!");

        // --- Setup P2 Components ---

        // 1. Set AI Room
        EnemyAI phase2AI = phase2GO.GetComponent<EnemyAI>();
        if (phase2AI != null)
        {
            phase2AI.parentRoom = enemyAI.parentRoom;
        }

        // 2. Set BossController Room
        BossController phase2Controller = phase2GO.GetComponent<BossController>();
        if (phase2Controller != null)
        {
            phase2Controller.parentRoom = enemyAI.parentRoom;
        }

        // --- 3. SET ENEMY TYPE (THE FIX) ---
        EnemySkillDecider phase2Decider = phase2GO.GetComponent<EnemySkillDecider>();
        if (phase2Decider != null)
        {
            phase2Decider.EnemyType = PhaseTwoEnemyType;
            Debug.Log($"<color=red>PHASE 2 SETUP:</color> Set EnemyType to '{PhaseTwoEnemyType}'.");
        }
        // --- END FIX ---

        // 4. Mark Grid as Occupied
        GameObject markerGO = BoardManager.Instance.GetMarkerGameObjectAtGrid(TargetGridPosition, targetRoomGO);
        if (markerGO != null)
        {
            Marker marker = markerGO.GetComponent<Marker>();
            if (marker != null)
            {
                marker.IsOccupied = true;
            }
        }
    }
}

