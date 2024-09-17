using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector2 platformingRespawnPoint;
    public string transitionedFromScene;

    [SerializeField] private Bench bench;
    [SerializeField] private FadeUI pauseMenu;
    [SerializeField] private float fadeTime;
    public bool gameIsPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate GameManager found. Destroying this instance.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameManager instance created and set.");
        }

        SaveData.Instance.Initialize();
        SaveScene();
        bench = FindObjectOfType<Bench>();
        Debug.Log("Bench found: " + (bench != null));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("GameManager instance destroyed.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        RespawnPoint respawnPoint = FindObjectOfType<RespawnPoint>();
        if (respawnPoint != null)
        {
            platformingRespawnPoint = respawnPoint.GetPosition();
            Debug.Log("Respawn point for scene '" + scene.name + "' set to: " + platformingRespawnPoint);
        }
        else
        {
            Debug.LogWarning("No RespawnPoint found in scene '" + scene.name + "'");
        }
    }

    public void RespawnPlayer()
    {
        Debug.Log("Respawning player to position: " + platformingRespawnPoint);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.transform.position = platformingRespawnPoint;
            PlayerController.Instance.Respawn();
            Debug.Log("Player respawned.");
        }
        else
        {
            Debug.LogWarning("PlayerController instance is null. Cannot respawn player.");
        }
    }

    public void ResetGameData()
    {
        Debug.Log("Resetting game data.");
        SaveData.Instance.ResetData();
        Debug.Log("Game data has been reset.");
        Debug.Log($"Weapon Damage: {PlayerController.Instance.damage}, Money: {SaveData.Instance.money}");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Scene reloaded.");
    }

    public void UnpauseGame()
    {
        Debug.Log("Unpausing game.");
        Time.timeScale = 1;
        gameIsPaused = false;
        if (pauseMenu != null)
        {
            pauseMenu.FadeUIOut(fadeTime);
            Debug.Log("Faded out pause menu.");
        }
        else
        {
            Debug.LogWarning("PauseMenu reference is null. Cannot fade out.");
        }
    }

    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
        Debug.Log("Saved current scene: " + currentSceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameIsPaused)
            {
                Debug.Log("Pausing game.");
                pauseMenu.FadeUIIn(fadeTime);
                Time.timeScale = 1;
                gameIsPaused = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Resetting game data.");
            ResetGameData();
        }
    }
}
