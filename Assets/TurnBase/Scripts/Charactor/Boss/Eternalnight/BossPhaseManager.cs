using UnityEngine;

/// <summary>
/// A persistent Singleton to store buff stacks and other data 
/// that needs to be passed between boss phases or different scenes.
/// </summary>
public class BossPhaseManager : MonoBehaviour
{
    public static BossPhaseManager Instance;

    // The global stack count for the Crimson Devourer buff
    public int CrimsonDevourerStacks = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Keep this manager alive across scene loads
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds one stack to the global counter.
    /// </summary>
    public void AddDevourerStack()
    {
        CrimsonDevourerStacks++;
        Debug.Log($"<color=purple>CRIMSON DEVOURER STACK ADDED.</color> Total Stacks: {CrimsonDevourerStacks}");
    }

    /// <summary>
    /// Retrieves the total damage bonus percentage (2% per stack).
    /// </summary>
    /// <returns>A float multiplier (e.g., 10 stacks = 0.20)</returns>
    public float GetDevourerDamageBonus()
    {
        // 1 stack = 2% bonus (0.02f)
        return CrimsonDevourerStacks * 0.02f;
    }

    /// <summary>
    /// Resets the stacks, e.g., for a new game or full wipe.
    /// </summary>
    public void ResetStacks()
    {
        CrimsonDevourerStacks = 0;
    }
}
