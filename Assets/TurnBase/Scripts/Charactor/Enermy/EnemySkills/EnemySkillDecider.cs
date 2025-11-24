using UnityEngine;
using System.Collections;

public class EnemySkillDecider : MonoBehaviour
{
    private EnemyData enemyData;
    private ToxicFragment toxicFragment;
    private FrostveilPrison frostveilPrison;
    private SilentFang silentFang;
    private Venomous venomous;
    private PowerSlam powerSlam;
    private Earthshatter earthshatter;
    
    // NEW: Reference for the FrontalSmash skill
    private EdgeBurst Edgeburst;

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

        // NEW: Initialize FrontalSmash reference
        Edgeburst = GetComponent<EdgeBurst>();

        
    }

    /// <summary>
    /// Helper to check if ANY player in the game currently has a specific status flag set.
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

        // --- 1. FRONTAL SMASH CHECK (EnemyType: Miniboss - Highest Priority) ---
        // This is a direct attack and should execute if a player is in range.
        if (EnemyType == "Zenos" && Edgeburst != null)
        {
            if (Edgeburst.FindNearestPlayer() != null) // Check if any player is in range (using skill's internal range)
            {
                // This skill does not require SP deduction from the Global Pool. It's a special ability.
                yield return StartCoroutine(Edgeburst.CheckAndExecuteSkill());

                SkillExecutedThisTurn = true; // Signal success to EnemyAI
                yield break; // End turn because the skill was successful
                
            }
        }
        
        // --- 2. EARTHSHATTER CHECK (EnemyType: Golem) ---
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

        // --- 3. POWER SLAM CHECK (EnemyType: Minotaur) ---
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

        // --- 4. UNTYPED FALLBACK CHECKS ---
        if (string.IsNullOrEmpty(EnemyType))
        {
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