using UnityEngine;

public class EnemySelection : MonoBehaviour
{
    // Optional: Add a material or indicator for when the enemy is selected
    // public GameObject selectionIndicator; 

    // Make sure your enemy has a Collider component (e.g., BoxCollider or SphereCollider)

    private void OnMouseDown()
    {
        // Only allow selection during the player's turn
        if (TurnManager.Instance.currentPhase == Phase.Player)
        {
            // Deselect the old target first
            PlayerAttack.DeselectEnemy();

            // Set this enemy as the new selected target
            PlayerAttack.selectedEnemyTarget = transform;

            Debug.Log(gameObject.name + " selected as target!");

            // Optional: Add visual feedback
            // SetSelectedVisual(true);
        }
    }

    /*
    // Optional: Implement this if you use visual feedback
    public void SetSelectedVisual(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }
    */
}