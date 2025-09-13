using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{
    public Slider Healthbar;
    

    void Update()
    {
        Healthbar.value = PlayerHealth.Instance.currentHealth;
        Healthbar.maxValue = PlayerHealth.Instance.maxHealth;
    }
}
