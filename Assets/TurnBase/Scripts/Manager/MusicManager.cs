using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public GameObject BGmusic;
    public GameObject bosslowhealth;
    private BossData bossData;

    // Flag to prevent continuous searching once boss is found
    private bool bossSearchAttempted = false;

    void Start()
    {
        // Assuming BGmusic is the default/ambient music and should start active
        if (BGmusic != null)
        {
            BGmusic.SetActive(true);
        }
        if (bosslowhealth != null)
        {
            bosslowhealth.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 1. ATTEMPT TO FIND BOSS (Only runs if bossData is null)
        if (bossData == null)
        {
            // Try to find the Boss GameObject
            GameObject bossObject = GameObject.FindGameObjectWithTag("Boss");

            if (bossObject != null)
            {
                // Assign BossData if found
                bossData = bossObject.GetComponent<BossData>();
            }

            // If we still can't find it, the block below is safely skipped
            if (bossData == null)
            {
                // We're done for this frame if the boss or component isn't found
                return;
            }
        }

        // 2. CHECK BOSS HEALTH (Only runs if bossData is GUARANTEED not null)
        // This is where your previous error occurred (line 22). It's now safe.
        int currentHealth = bossData.currentHP;

        if (currentHealth <= 500) // Changed to <= 500 for a more reliable threshold trigger
        {
            if (BGmusic != null && BGmusic.activeSelf)
            {
                BGmusic.SetActive(false);
            }
            if (bosslowhealth != null && !bosslowhealth.activeSelf)
            {
                bosslowhealth.SetActive(true);
            }
        }
        else
        {
            // OPTIONAL: Revert to BG music if boss health goes above 500 (e.g., if healed)
            if (BGmusic != null && !BGmusic.activeSelf)
            {
                BGmusic.SetActive(true);
                bosslowhealth.SetActive(false);
            }
        }
    }
}