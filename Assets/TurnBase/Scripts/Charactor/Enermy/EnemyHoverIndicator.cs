using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyHoverIndicator : MonoBehaviour
{
    // The visual indicator for hovering (separate from the selection indicator)
    [Tooltip("The child GameObject to enable/disable when hovering. This should be a UNIQUE visual, separate from any selection or attack indicator.")]
    public GameObject hoverVisual;

    // NEW: We will filter the raycast to only hit objects on the "Enemy" layer.
    private LayerMask enemyLayerMask;

    private PlayerAttack playerAttack;
    private bool isCurrentlyHovering = false;
    private Camera mainCamera;

    void Start()
    {
        // Find the LayerMask for the "Enemy" layer
        // CRITICAL: Ensure you have created a layer named "Enemy" in the Unity Editor.
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

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerAttack = playerGO.GetComponent<PlayerAttack>();
            if (playerAttack == null)
            {
                Debug.LogError(gameObject.name + ": Hover Indicator: Could not find PlayerAttack component on the GameObject tagged 'Player'.");
            }
        }
        else
        {
            Debug.LogError(gameObject.name + ": Hover Indicator: Could not find any GameObject with the tag 'Player'.");
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
        if (hoverVisual == null || playerAttack == null || TurnManager.Instance == null)
        {
            return;
        }

        // The two conditions required to show the hover visual:
        bool isTargeting = playerAttack.isTargetingMode;
        bool isPlayerTurn = TurnManager.Instance.currentPhase == Phase.Player;

        bool canShowVisual = isTargeting && isPlayerTurn;

        if (active && canShowVisual)
        {
            hoverVisual.SetActive(true);
            // We can now remove the diagnostic logs since the layer mask should fix the issue
            // Debug.Log(gameObject.name + " Raycast Success: Visual ON. Targeting: " + isTargeting + ", Player Turn: " + isPlayerTurn);
        }
        else
        {
            hoverVisual.SetActive(false);
        }
    }

    // Manual Raycasting to detect hover state (more reliable than OnMouseEnter)
    void Update()
    {
        if (playerAttack == null || mainCamera == null) return;

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

        // 2. Perform the manual Raycast check - **NOW USING LAYERMASK**
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
                    // Debug.Log("RAYCAST HIT: " + gameObject.name + ". Attempting to show visual.");
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
        if (hoverVisual != null && hoverVisual.activeSelf && !playerAttack.isTargetingMode)
        {
            hoverVisual.SetActive(false);
            isCurrentlyHovering = false; // Reset hover state if mode is canceled
        }
    }
}
