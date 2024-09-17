using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public SceneFader sceneFader;
    public static UIManager Instance;

    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject deathScreenText;
    [SerializeField] private GameObject showControls;
    [SerializeField] private Button closeControlsButton; // Reference to the close button

    public GameObject inventory;
    public GameObject mapHandler;

    [Header("Boss Health Bar")]
    [SerializeField] private Image bossHealthBarFill;
    [SerializeField] private GameObject bossHealthBarPanel;
    [SerializeField] private Text bossNameText;

    private bool controlsShown = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        sceneFader = GetComponentInChildren<SceneFader>();
        deathScreen.SetActive(false);
        deathScreenText.SetActive(false);
        bossHealthBarPanel.SetActive(false);
        showControls.SetActive(false);

        // Add listener to the close button
        if (closeControlsButton != null)
        {
            closeControlsButton.onClick.AddListener(CloseControls);
        }
    }

    private void Start()
    {
        if (!controlsShown)
        {
            StartCoroutine(ShowControlsOnce());
        }
    }

    private IEnumerator ShowControlsOnce()
    {
        controlsShown = true;
        showControls.SetActive(true);
        yield return null; // Wait for a frame, but do not use WaitForSeconds
    }

    public void ResetControlsShown()
    {
        controlsShown = false;
    }

    public void ShowDeathScreen()
    {
        StartCoroutine(HandleDeathScreen());
    }

    private IEnumerator HandleDeathScreen()
    {
        yield return new WaitForSeconds(0.1f);
        yield return sceneFader.Fade(SceneFader.FadeDirection.In);

        deathScreen.SetActive(true);
        deathScreenText.SetActive(true);

        yield return new WaitForSeconds(2f);

        GameManager.Instance.RespawnPlayer();
    }

    public IEnumerator DeactivateDeathScreen()
    {
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(false);
        deathScreenText.SetActive(false);
        yield return sceneFader.Fade(SceneFader.FadeDirection.Out);
    }

    public void SetBossHealthBarVisible(bool isVisible)
    {
        bossHealthBarPanel.SetActive(isVisible);
    }

    public void UpdateBossHealthBar(float currentHealth, float maxHealth)
    {
        if (bossHealthBarPanel.activeSelf)
        {
            float healthPercentage = currentHealth / maxHealth;
            bossHealthBarFill.fillAmount = healthPercentage;
        }
    }

    public void SetBossName(string name)
    {
        if (bossNameText != null)
        {
            bossNameText.text = name;
        }
    }

    // Method to close the "ShowControls" panel
    public void CloseControls()
    {
        showControls.SetActive(false);
    }
}
