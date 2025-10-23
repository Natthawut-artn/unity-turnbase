using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyHoverIndicator : MonoBehaviour
{
    // The visual indicator for hovering (separate from the selection indicator)
    [Tooltip("The child GameObject to enable/disable when hovering. This should be a UNIQUE visual, separate from any selection or attack indicator.")]
    public GameObject hoverVisual;

    private LayerMask enemyLayerMask;

    // Cached reference for the currently targeting PlayerAttack component to avoid constant lookups.
    private PlayerAttack cachedAttacker;

    private bool isCurrentlyHovering = false;
    private Camera mainCamera;

    // Performance optimization: FindObjectsOfType is expensive. Only run it every N frames.
    private int frameCounter = 0;
    // Check for active attacker once every 10 frames (~6 times per second). Adjust this number higher (e.g., 20) if lag persists.
    private const int ATTACKER_UPDATE_FREQUENCY = 10;

    void Start()
    {
        // Find the LayerMask for the "Enemy" layer
        enemyLayerMask = LayerMask.GetMask("Enemy");
        if (enemyLayerMask == 0)
        {
            Debug.LogError(gameObject.name + ": CRITICAL ERROR! Please create a Layer named 'Enemy' in the Unity Editor and assign it to this enemy object.");
        }

        mainCamera = Camera.main; // Cache the main camera reference
        if (mainCamera == null)
        {
            Debug.LogError(gameObject.name + ": Hover Indicator: Could not find Main Camera.");
        }

        if (hoverVisual == null)
        {
            Debug.LogError(gameObject.name + ": 'hoverVisual' is NOT assigned in the Inspector!");
        }
        else
        {
            hoverVisual.SetActive(false);
        }
    }

    private void SetHoverVisual(bool active)
    {
        // Use the cachedAttacker
        if (hoverVisual == null || cachedAttacker == null || TurnManager.Instance == null)
        {
            hoverVisual.SetActive(false);
            return;
        }

        // The two conditions required to show the hover visual:
        bool isTargeting = cachedAttacker.isTargetingMode;
        bool isPlayerTurn = TurnManager.Instance.currentPhase == Phase.Player;

        bool canShowVisual = active && isTargeting && isPlayerTurn;

        hoverVisual.SetActive(canShowVisual);
    }

    // New method to perform the expensive attacker lookup, only called periodically
    private void UpdateAttackerCache()
    {
        if (TurnManager.Instance.currentPhase != Phase.Player)
        {
            // If it's not the player phase, clear the cache.
            cachedAttacker = null;
            return;
        }

        // Find the currently targeting PlayerAttack component.
        // This is the expensive call we want to limit.
        PlayerAttack[] allPlayerAttacks = FindObjectsOfType<PlayerAttack>();
        cachedAttacker = null;

        foreach (PlayerAttack pa in allPlayerAttacks)
        {
            if (pa.isTargetingMode)
            {
                cachedAttacker = pa;
                break;
            }
        }

        // Safety fallback: If no one is targeting but it's the player phase, 
        // cache the first player to handle hover detection logic checks.
        if (cachedAttacker == null && allPlayerAttacks.Length > 0)
        {
            cachedAttacker = allPlayerAttacks[0];
        }
    }

    // Manual Raycasting to detect hover state (more reliable than OnMouseEnter)
    void Update()
    {
        if (mainCamera == null || TurnManager.Instance == null) return;

        // --- 0. Performance Optimization: Update cached attacker periodically ---
        frameCounter++;
        if (frameCounter >= ATTACKER_UPDATE_FREQUENCY)
        {
            UpdateAttackerCache();
            frameCounter = 0;
        }

        // --- Check if we have an active attacker to proceed ---
        if (cachedAttacker == null)
        {
            if (isCurrentlyHovering)
            {
                isCurrentlyHovering = false;
                hoverVisual.SetActive(false);
            }
            return;
        }

        // 1. Check if the pointer is currently over a UI element (skip 3D interaction)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (isCurrentlyHovering)
            {
                isCurrentlyHovering = false;
                SetHoverVisual(false);
            }
            return;
        }

        // 2. Perform the manual Raycast check
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // The raycast will now ONLY detect objects on the "Enemy" layer!
        if (Physics.Raycast(ray, out hit, 1000f, enemyLayerMask))
        {
            // The object hit MUST be an enemy because of the LayerMask.
            if (hit.collider.gameObject == gameObject)
            {
                if (!isCurrentlyHovering)
                {
                    isCurrentlyHovering = true;
                    SetHoverVisual(true);
                }
            }
            else // Ray hit a different enemy object (due to the layer mask)
            {
                // This script only cares if it hit *this* specific enemy
                if (isCurrentlyHovering)
                {
                    isCurrentlyHovering = false;
                    SetHoverVisual(false);
                }
            }
        }
        else // The ray hit nothing on the Enemy layer
        {
            if (isCurrentlyHovering)
            {
                isCurrentlyHovering = false;
                SetHoverVisual(false);
            }
        }

        // Safety check to ensure the visual is disabled if targeting mode ends
        if (hoverVisual != null && hoverVisual.activeSelf && !cachedAttacker.isTargetingMode)
        {
            hoverVisual.SetActive(false);
            isCurrentlyHovering = false; // Reset hover state if mode is canceled
        }
    }
}
