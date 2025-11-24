using System.Collections;
using UnityEngine;

public class Tentacle : MonoBehaviour
{
    [Header("Tentacle Settings")]
    public Marker occupiedMarker;
    public int damageAmount = 50;
    public float activeDuration = 1.5f;

    [Header("Visuals & Components")]
    public MeshRenderer tentacleRenderer;
    public Collider damageCollider;

    void Awake()
    {
        // Initially, the tentacle's mesh and collider are disabled
        if (tentacleRenderer != null) tentacleRenderer.enabled = false;
        if (damageCollider != null) damageCollider.enabled = false;
    }
    private void Update()
    {
        if (GameManager.Instance != null && AppStateManager.appState == AppState.Hub)
        {
            DestroySelf();
            return;
        }
    }
    private void DestroySelf()
    {
        // 2. Destroy the boss object
        Destroy(gameObject);
    }
    public void Initialize(Marker markerToOccupy)
    {
        occupiedMarker = markerToOccupy;
        StartCoroutine(TentacleLifecycle());
    }

    private IEnumerator TentacleLifecycle()
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("TurnManager not found. Tentacle cannot function.");
            Destroy(gameObject);
            yield break;
        }

        // 1. PHASE 1: WARNING (Tile turns red)
        if (occupiedMarker != null)
        {
            occupiedMarker.SetMarkerColor(Color.red);
        }

        // ⭐ NEW: Record the current Turn Cycle Count
        int turnCycleAtSpawn = TurnManager.Instance.TurnCycleCount;

        Debug.Log($"Tentacle WARNING starts. Current Cycle: {turnCycleAtSpawn}. Waiting for next Player Turn.");

        // --- STEP 1: Wait for the next Player Phase to START (The dodge turn) ---

        // Wait until the TurnManager has completed a full cycle (Enemy Phase -> Player Phase).
        // If it spawns during the Player Phase, it waits until the next Player Phase (1 full cycle).
        // If it spawns during the Enemy Phase, it waits until the current Enemy Phase ends (less than 1 cycle).
        while (TurnManager.Instance.TurnCycleCount == turnCycleAtSpawn || TurnManager.Instance.currentPhase != Phase.Player)
        {
            // The first part of the condition handles waiting for the full cycle.
            // The second part ensures we pause specifically at the start of the Player's turn.
            yield return null;
        }

        // We are now guaranteed to be at the **START of the Player's Dodge Turn**.

        // --- STEP 2: Wait through the entire Player Dodge Turn ---

        // Wait until the phase switches back to Enemy (i.e., the player ends their turn).
        while (TurnManager.Instance.currentPhase == Phase.Player)
        {
            yield return null;
        }

        // We are now at the **START of the Enemy Phase**, right after the player's turn ended.

        // --- PHASE 2: TENTACLE APPEARS AND BECOMES ACTIVE (Enemy Phase) ---

        if (tentacleRenderer != null) tentacleRenderer.enabled = true;
        if (damageCollider != null) damageCollider.enabled = true;
        Debug.Log($"Tentacle is now ACTIVE during the Enemy Turn. Damage window: {activeDuration}s.");

        // Wait for the active duration (1.5 seconds)
        yield return new WaitForSeconds(activeDuration);
        SFXManager.Instance?.PlayOneShot(SFX.tentacleRise);

        // --- PHASE 3: TENTACLE DISAPPEARS AND DESTROYS ITSELF ---
        if (tentacleRenderer != null) tentacleRenderer.enabled = false;
        if (damageCollider != null) damageCollider.enabled = false;

        if (occupiedMarker != null)
        {
            occupiedMarker.RestoreOriginalColor();
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (damageCollider != null && damageCollider.enabled && other.CompareTag("Player"))
        {
            PlayerData playerData = other.GetComponent<PlayerData>();
            if (playerData != null)
            {
                playerData.TakeDamage(damageAmount);
                HPBarsHUDController.Update();
            }
            Debug.Log($"Tentacle dealt {damageAmount} damage to Player!");
            damageCollider.enabled = false;
        }
    }
}