using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Required for .ToList()

public class BerserkerCall : MonoBehaviour
{
    // --- Configuration ---
    [Header("Buff Values")]
    [Tooltip("The flat ATK bonus to add.")]
    public int attackBuff = 30;
    [Tooltip("The Crit Rate bonus to add.")]
    public int critRateBuff = 20;
    [Tooltip("The Crit Damage bonus to add.")]
    public int critDmgBuff = 40;

    [Tooltip("The number of crystals that must be destroyed to trigger the buff.")]
    public int crystalsToDestroy = 2;

    // --- Component References ---
    private EnemyData enemyData; // Reference to the Boss's stats
    public Animator animator;
    // --- State Tracking ---
    [Tooltip("A list of all BossCrystals this script is tracking.")]
    private List<BossCrystal> trackedCrystals;
    private int initialCrystalCount = 0;
    private bool hasBeenBuffed = false; // Flag to ensure buff only applies once
    
    void Start()
    {
        // 1. Get the Boss's stats component (must be on the same GameObject)
        enemyData = GetComponent<EnemyData>();
        if (enemyData == null)
        {
            Debug.LogError($"BerserkerCall: Cannot find EnemyData component on {gameObject.name}! Disabling script.");
            enabled = false;
            return;
        }

        // 2. Find all BossCrystal objects in the scene
        // This is the most reliable way to find all instances of that script
        trackedCrystals = FindObjectsOfType<BossCrystal>().ToList();
        initialCrystalCount = trackedCrystals.Count;

        if (initialCrystalCount == 0)
        {
            Debug.LogWarning("BerserkerCall: No objects with BossCrystal script found. This buff will never trigger.");
            enabled = false;
        }
        else
        {
            Debug.Log($"BerserkerCall: Tracking {initialCrystalCount} BossCrystals.");
        }
    }

    void Update()
    {
        // 3. If buff is already applied, do nothing.
        if (hasBeenBuffed || trackedCrystals == null)
        {
            return;
        }

        // 4. Check how many crystals are left
        // This line efficiently removes any 'null' entries from the list 
        // (which happens when a GameObject is destroyed)
        trackedCrystals.RemoveAll(crystal => crystal == null);

        int currentCrystalCount = trackedCrystals.Count;
        int destroyedCount = initialCrystalCount - currentCrystalCount;

        // 5. Check the trigger condition (at least 2 destroyed)
        if (destroyedCount >= crystalsToDestroy)
        {
            SFXManager.Instance?.PlayOneShot(SFX.BerserkerCall);
            StartCoroutine(BerserkerAnim());
            ApplyBerserkerBuff();
        }
    }

    private void ApplyBerserkerBuff()
    {
       
        // 6. Apply the buff to the Boss's stats
        if (enemyData != null)
        {
            enemyData.currentATK += attackBuff;
            enemyData.currentCritRate += critRateBuff;
            enemyData.currentCritDGM += critDmgBuff;

            hasBeenBuffed = true; // Set the flag so this only runs ONCE

            Debug.Log($"<color=red>BERSERKER CALL ACTIVATED:</color> {gameObject.name} gained +{attackBuff} ATK, +{critRateBuff}% Crit Rate, +{critDmgBuff}% Crit Dmg.");

            // Optional: Play a particle effect or sound on the boss
            // e.g., Instantiate(berserkerEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator BerserkerAnim()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Skill4");
        }
        yield return new WaitForSeconds(2.5f);
    }
}
