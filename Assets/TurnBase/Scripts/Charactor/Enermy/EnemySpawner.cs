using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("Grid position in the room (must match BoardManager marker positions)")]
    public Vector2 GridPosition;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Room and Enemy Settings")]
    public string RoomName;
    public string EnemyPrefabPath = "Prefabs/Charactor/Enemy/Goblin_Enemy Variant";
    public List<EnemySpawnData> SpawnPositions = new List<EnemySpawnData>();
    public Vector3 SpawnOffset = new Vector3(0f, 1.49f, 0f);
    public bool SpawnOnStart = false;

    private GameObject targetRoom;
    private bool hasSpawned = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForBoardManager());
    }

    private IEnumerator WaitForBoardManager()
    {
        while (BoardManager.Instance == null)
            yield return null;

        targetRoom = BoardManager.Instance.GetRoom(RoomName);
        if (targetRoom == null)
        {
            Debug.LogError($"EnemySpawner: Room '{RoomName}' not found in BoardManager.");
            yield break;
        }

        if (SpawnOnStart)
            SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (hasSpawned)
        {
            Debug.Log($"EnemySpawner: Enemies already spawned in {RoomName}");
            return;
        }

        if (targetRoom == null)
        {
            Debug.LogError("EnemySpawner: No valid room to spawn enemies in.");
            return;
        }

        GameObject enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner: Could not load enemy prefab from '{EnemyPrefabPath}'");
            return;
        }

        foreach (EnemySpawnData spawnData in SpawnPositions)
        {
            // Snap to nearest whole grid so minor decimal errors don�t cause misses
            Vector2 roundedGrid = new Vector2(Mathf.Round(spawnData.GridPosition.x), Mathf.Round(spawnData.GridPosition.y));
            Vector3 worldPos = BoardManager.Instance.GetMarkerWorldPosition(roundedGrid, targetRoom);

            if (worldPos == Vector3.zero)
            {
                Debug.LogWarning($"EnemySpawner: No marker found at {roundedGrid} in {RoomName}");
                continue;
            }

            Vector3 spawnPos = worldPos + SpawnOffset;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = $"Enemy_{roundedGrid.x}_{roundedGrid.y}";
            GameManager.Instance.Enemys_alive.Add(enemy);

           
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.parentRoom = targetRoom.GetComponent<Room>();
            }

            EnemyData enemyData = enemy.GetComponent<EnemyData>();
            if (enemyData != null)
            {
                enemyData.GridPosition = roundedGrid;
                enemyData.currentHP = enemyData.HP;
            }

            Debug.Log($"Spawned {enemy.name} at {roundedGrid} in {RoomName}");
        }

        hasSpawned = true;

        TurnManager.Instance.isInTurnSystem = true;
        TurnManager.Instance.EndTurn();
    }
}
