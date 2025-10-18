using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPBarController : MonoBehaviour
{
    // --- NEW: STATIC INSTANCE for easy access from the spawner ---
    public static BossHPBarController Instance;
    // -------------------------------------------------------------

    [Header("UI Components")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Boss Reference")]
    public GameObject bossObject;

    // NOTE: Changed to CharactorInheritance since BossData inherits from it
    private CharactorInheritance bossData;

    // Used to reference the entire UI panel for easy hiding/showing
    private GameObject bossHPBarPanel;



    void Awake()
    {
        // Set the static instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Find the main UI panel this script is attached to, to hide it initially
        bossHPBarPanel = this.gameObject; // Assumes the script is on the panel itself.
        bossHPBarPanel.SetActive(false); // Hides the panel immediately.
    }

    void Start()
    {
        // NOTE: We no longer call FindWithTag here, as the spawner will call SetBossObject
    }

    /// <summary>
    /// Called by the Spawner script when the boss is instantiated.
    /// </summary>
    public void SetBossObject(GameObject newBossObject)
    {
        bossObject = newBossObject;

        // Ensure you get the base data script (CharactorInheritance or BossData)
        bossData = bossObject.GetComponent<CharactorInheritance>();

        if (bossData != null)
        {
            InitializeHPBar();
            // Show the HP bar once the boss is successfully found
            bossHPBarPanel.SetActive(true);
            Debug.Log("Boss HP Bar initialized and displayed.");
        }
        else
        {
            Debug.LogError("The spawned object does not have the CharactorInheritance component.");
            bossHPBarPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Check if the boss is still alive and the data is available
        if (bossData != null)
        {
            UpdateHPBar();
        }
        else if (bossHPBarPanel.activeSelf) // Check if the entire panel is active
        {
            // Hide the HP bar if the boss has died
            bossHPBarPanel.SetActive(false);
        }
    }

    // ... (InitializeHPBar, UpdateHPBar, and UpdateHPText methods remain the same) ...
    private void InitializeHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = bossData.HP;
            hpSlider.value = bossData.currentHP;
        }

        UpdateHPText();
    }

    private void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = Mathf.Lerp(hpSlider.value, bossData.currentHP, Time.deltaTime * 5f);
        }

        UpdateHPText();
    }

    private void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = $"{bossData.currentHP} / {bossData.HP}";
        }
    }
}