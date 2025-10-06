using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("Grid position in the room (match BoardManager marker positions)")]
    public Vector2 GridPosition;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Room and Enemy Settings")]
    [Tooltip("Room name that matches the room GameObject name in BoardManager.ListRooms")]
    public string RoomName;

    [Tooltip("Enemy prefab path (from Resources)")]
    public string EnemyPrefabPath = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";

    [Tooltip("List of grid positions in the room where enemies will spawn")]
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();

    [Tooltip("Offset above the marker for enemy height")]
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);

    [Tooltip("Spawn enemies automatically on Start")]
    public bool SpawnOnStart = true;

    private GameObject targetRoom;

    void Start()
    {
        StartCoroutine(WaitForBoardManager());
    }

    private IEnumerator WaitForBoardManager()
    {
        // Wait until BoardManager.Instance is not null
        while (BoardManager.Instance == null)
            yield return null;

        targetRoom = BoardManager.Instance.GetRoom(RoomName);
        if (targetRoom == null)
        {
            Debug.LogError($"EnemySpawner: Room '{RoomName}' not found in BoardManager.");
            yield break;
        }

        if (SpawnOnStart)
        {
            SpawnEnemies();
        }
    }


    /// <summary>
    /// Spawns all enemies in SpawnPositions list.
    /// </summary>
    public void SpawnEnemies()
    {
        if (targetRoom == null)
        {
            Debug.LogError("EnemySpawner: No valid room to spawn enemies in.");
            return;
        }

        GameObject enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner: Could not load enemy prefab from path '{EnemyPrefabPath}'");
            return;
        }

        foreach (EnemySpawnData spawnData in SpawnPositions)
        {
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(spawnData.GridPosition, targetRoom);
            if (worldPos == Vector3.zero)
            {
                Debug.LogWarning($"EnemySpawner: No marker found at {spawnData.GridPosition} in {RoomName}");
                continue;
            }

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = $"Enemy_{spawnData.GridPosition.x}_{spawnData.GridPosition.y}";

            EnemyData enemyData = enemy.GetComponent<EnemyData>();
            if (enemyData != null)
            {
                enemyData.GridPosition = spawnData.GridPosition;
                enemyData.currentHP = enemyData.HP;
            }

            Debug.Log($"Spawned {enemy.name} at {spawnData.GridPosition} in {RoomName}");
        }
    }
}
