using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarSprite;
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }
    public void UpdateEnemyHealthBar(int maxHealth, int currentHealth)
    {
        if (maxHealth <= 0)
        {
            healthBarSprite.fillAmount = 0;
            return;
        }

        healthBarSprite.fillAmount = (float)currentHealth / (float)maxHealth;
    }

    private void Update()
    {
        // Face the camera
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}

