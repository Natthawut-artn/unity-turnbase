using UnityEngine;

public class EnemySkillPoint : MonoBehaviour
{
    public static EnemySkillPoint Instance;

    [Tooltip("The shared resource pool all enemies deduct from.")]
    public int currentSkillPointPool = 10;

    // REMOVED: public int pointCost = 5; // Cost is now defined on ToxicFragment.cs

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Attempts to deduct the cost from the global skill point pool.
    /// </summary>
    /// <param name="cost">The amount to spend.</param>
    /// <returns>True if points were successfully spent, false otherwise (insufficient points).</returns>
    public bool TrySpendGlobalSkillPoints(int cost)
    {
        if (currentSkillPointPool >= cost)
        {
            currentSkillPointPool -= cost;
            Debug.Log($"Global SP Pool spent: {cost}. Remaining: {currentSkillPointPool}");
            return true;
        }
        Debug.Log($"Global SP Pool insufficient. Needed {cost}, available {currentSkillPointPool}");
        return false;
    }
}
