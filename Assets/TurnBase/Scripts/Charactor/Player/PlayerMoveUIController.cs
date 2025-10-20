using UnityEngine;
using TMPro;

public class PlayerMoveUIController : MonoBehaviour
{
    public static PlayerMoveUIController Instance;

    [Tooltip("The TextMeshProUGUI component for displaying the moves count.")]
    public TextMeshProUGUI moveCountText;

    // Reference to the active PlayerController
    private PlayerController playerController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (moveCountText == null)
        {
            Debug.LogError("PlayerMoveUIController needs the TextMeshProUGUI assigned!");
        }
        else
        {
            moveCountText.text = "Moves Left: --/--"; // Initial display
        }
    }

    // Called by the PlayerController on start or on new room spawn
    public void SetPlayer(PlayerController newPlayer)
    {
        playerController = newPlayer;
        if (playerController != null)
        {
            // Initializes the UI with correct values
            UpdateMoveUI();
        }
    }

    // Called by the PlayerController whenever the move count changes
    public void UpdateMoveUI()
    {
        if (playerController != null && moveCountText != null)
        {
            int maxMoves = playerController.maxTilesPerTurn;
            int remainingMoves = maxMoves - playerController.tilesMoved;

            moveCountText.text = $"Moves Left: {remainingMoves}/{maxMoves}";
        }
    }
}