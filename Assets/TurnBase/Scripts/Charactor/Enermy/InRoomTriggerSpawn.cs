using UnityEngine;

public class InRoomTriggerSpawn : MonoBehaviour
{
    public EnemySpawner enemySpawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && enemySpawner != null)
        {
            enemySpawner.SpawnEnemies();
            Debug.Log("Player entered room trigger — spawning enemies.");
        }
    }
}
