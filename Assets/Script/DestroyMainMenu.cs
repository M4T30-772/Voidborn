using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyMainMenu : MonoBehaviour
{
    private void Awake()
    {
        // Ensure this object persists across scenes
        DontDestroyOnLoad(gameObject);
        
        // Register to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the newly loaded scene is the main menu
        if (scene.buildIndex == 0)
        {
            // Destroy this GameObject if the scene is the main menu
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unregister from the sceneLoaded event when this object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
