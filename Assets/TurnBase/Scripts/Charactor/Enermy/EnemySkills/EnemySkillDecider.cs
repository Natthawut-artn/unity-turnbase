using System.Collections;
using UnityEngine;

/// <summary>
/// Handles decision logic for unique skills based on the enemy type set by the spawner.
/// </summary>
public class EnemySkillDecider : MonoBehaviour
{
    private EnemyData enemyData;
    private ToxicFragment toxicFragment;
    private FrostveilPrison frostveilPrison;
    private SilentFang silentFang;
    private Venomous venomous;
    private PowerSlam powerSlam;
    private Earthshatter earthshatter;

    // NEW: Reference for the CultistEmber skill
    private CultistEmber cultistEmber;

    [HideInInspector] public string EnemyType;

    // FLAG: Signals to the EnemyAI that a skill was successfully executed and the turn should end.
    [HideInInspector] public bool SkillExecutedThisTurn = false;

    void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        toxicFragment = GetComponent<ToxicFragment>();
        frostveilPrison = GetComponent<FrostveilPrison>();
        silentFang = GetComponent<SilentFang>();
        venomous = GetComponent<Venomous>();

        powerSlam = GetComponent<PowerSlam>();
        earthshatter = GetComponent<Earthshatter>();

        cultistEmber = GetComponent<CultistEmber>(); // Initialize Cultist Ember
    }

    /// <summary>
    /// Helper to check if ANY player in the game currently has a specific status flag set.
    /// (Required to prevent wasting status effects)
    /// </summary>
    private bool IsAnyPlayerUnderStatus(string statusName)
    {
        PlayerData[] allPlayers = FindObjectsOfType<PlayerData>();

        foreach (PlayerData player in allPlayers)
        {
            // Uses reflection to safely access boolean status flags by string name.
            System.Reflection.FieldInfo field = player.GetType().GetField(statusName);
            if (field != null && field.FieldType == typeof(bool))
            {
                if ((bool)field.GetValue(player))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks the enemy's type and attempts to use the appropriate skill if conditions are met.
    /// </summary>
    public IEnumerator DecideAndExecuteSkill()
    {
        SkillExecutedThisTurn = false;

        if (enemyData == null || EnemySkillPoint.Instance == null)
        {
            yield break;
        }

        // Check if this unit is the Boss and skip all minion logic
        if (enemyData.heroclass == HeroClass.Boss)
        {
            yield break;
        }

        // --- 1. EARTHSHATTER CHECK (EnemyType: Golem) ---
        if (EnemyType == "Golem" && earthshatter != null)
        {
            int skillCost = earthshatter.skillCost;

            if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
            {
                if (earthshatter.IsPlayerInAttackRange())
                {
                    yield return StartCoroutine(earthshatter.ExecuteSkill());

                    SkillExecutedThisTurn = true;
                    yield break;
                }
            }
        }


        // --- 2. POWER SLAM CHECK (EnemyType: Minotaur) ---
        if (EnemyType == "Minotaur" && powerSlam != null)
        {
            int skillCost = powerSlam.skillCost;

            if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
            {
                if (powerSlam.IsPlayerInAttackRange())
                {
                    yield return StartCoroutine(powerSlam.ExecuteSkill());

                    SkillExecutedThisTurn = true;
                    yield break;
                }
            }
        }

        // --- 3. CULTIST EMBER CHECK (EnemyType: Cultist - New Status Check) ---
        if (EnemyType == "Cultist" && cultistEmber != null)
        {
            if (!IsAnyPlayerUnderStatus("isOnBurning")) // Prevent wasting status
            {
                int skillCost = cultistEmber.skillCost;

                if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                {
                    if (cultistEmber.IsPlayerInAttackRange())
                    {
                        yield return StartCoroutine(cultistEmber.ExecuteSkill());

                        SkillExecutedThisTurn = true;
                        yield break;
                    }
                }
            }
        }


        // --- 4. UNTYPED FALLBACK CHECKS ---
        if (string.IsNullOrEmpty(EnemyType))
        {
            if(earthshatter != null)
            {
                // 4a. Check for Earthshatter as the primary default skill (AOE Damage)
                int skillCost = earthshatter.skillCost;
                if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                {
                    if (earthshatter.IsPlayerInAttackRange())
                    {
                        yield return StartCoroutine(earthshatter.ExecuteSkill());
                        SkillExecutedThisTurn = true;
                        yield break;
                    }
                }
            }
            // 4a. Check for PowerSlam as the primary default skill (Direct Damage)
            if (powerSlam != null)
            {
                int skillCost = powerSlam.skillCost;

                if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                {
                    if (powerSlam.IsPlayerInAttackRange())
                    {
                        yield return StartCoroutine(powerSlam.ExecuteSkill());
                        SkillExecutedThisTurn = true;
                        yield break;
                    }
                }
            }

            // 4b. Check for Venomous as a secondary default skill (Status Damage)
            if (venomous != null)
            {
                if (!IsAnyPlayerUnderStatus("isOnVenom"))
                {
                    int skillCost = venomous.skillCost;

                    if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                    {
                        if (venomous.IsPlayerInAttackRange())
                        {
                            yield return StartCoroutine(venomous.ExecuteSkill());
                            SkillExecutedThisTurn = true;
                            yield break;
                        }
                    }
                }
            }
        }


        // --- 5. NAMED STATUS CHECKS (Assassin, Mage, Slime) ---

        // SILENT FANG CHECK (EnemyType: Mimic)
        if (EnemyType == "Mimic" && silentFang != null)
        {
            if (!IsAnyPlayerUnderStatus("isOnBloodFlowing"))
            {
                int skillCost = silentFang.skillCost;

                if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                {
                    if (silentFang.IsPlayerInAttackRange())
                    {
                        yield return StartCoroutine(silentFang.ExecuteSkill());
                        SkillExecutedThisTurn = true;
                        yield break;
                    }
                }
            }
        }

        // FROSTVEIL PRISON CHECK (EnemyType: Mage)
        if (EnemyType == "Mage" && frostveilPrison != null)
        {
            if (!IsAnyPlayerUnderStatus("isOnFrost"))
            {
                int skillCost = frostveilPrison.skillCost;

                if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
                {
                    if (frostveilPrison.IsPlayerInAttackRange())
                    {
                        yield return StartCoroutine(frostveilPrison.ExecuteSkill());
                        SkillExecutedThisTurn = true;
                        yield break;
                    }
                }
            }
        }

        // TOXIC FRAGMENT CHECK (EnemyType: Slime)
        if (EnemyType == "Slime" && toxicFragment != null)
        {
            int skillCost = toxicFragment.skillCost;

            if (EnemySkillPoint.Instance.currentSkillPointPool >= skillCost)
            {
                if (toxicFragment.IsPlayerInAttackRange())
                {
                    yield return StartCoroutine(toxicFragment.ExecuteSkill());

                    SkillExecutedThisTurn = true;
                    yield break;
                }
            }
        }
    }
}
