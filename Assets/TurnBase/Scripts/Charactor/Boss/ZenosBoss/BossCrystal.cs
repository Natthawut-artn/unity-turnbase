using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCrystal : MonoBehaviour
{
    public EnemyData CrystalData;

    public void Start()
    {
        CrystalData = GetComponent<EnemyData>();
    }

    public void Update()
    {
        if(CrystalData.currentHP <= 10)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        // 1. Find the target enemy by Tag.
        // **IMPORTANT**: Make sure your Boss GameObject (Zenos) is tagged correctly in the Unity Inspector!
        // Replace "Boss" with the actual tag you are using if it's different.
        GameObject zenosGO = GameObject.FindWithTag("Boss");

        // 2. Safety Check: Make sure Zenos was found.
        if (zenosGO == null)
        {
            Debug.LogWarning("BossCrystal destroyed, but could not find GameObject tagged 'Boss'.");
            return; // Exit if Boss isn't found
        }

        // 3. Get the EnemyData component from Zenos.
        // Assuming Zenos uses EnemyData or BossData which inherits from it.
        EnemyData zenosData = zenosGO.GetComponent<EnemyData>();
        // If Zenos uses BossData specifically, you might use:
        // BossData zenosBossData = zenosGO.GetComponent<BossData>();

        // 4. Safety Check: Make sure Zenos has the correct data component.
        if (zenosData == null /* && zenosBossData == null */) // Adjust check based on component used
        {
            Debug.LogWarning("BossCrystal destroyed, but the 'Boss' GameObject is missing the EnemyData/BossData component.");
            return;
        }

        // 5. Apply the damage. Use the correct data reference.
        int damageAmount = 10000;
        if (zenosData != null)
        {
            zenosData.TakeDamage(damageAmount);
        }
        // else if (zenosBossData != null)
        // {
        //     zenosBossData.TakeDamage(damageAmount); // Assuming BossData has TakeDamage
        // }

        Debug.Log($"BossCrystal destroyed! Dealt {damageAmount} damage to {zenosGO.name}.");

        // Optional: Trigger explosion effect, sound, etc.
    }
}

