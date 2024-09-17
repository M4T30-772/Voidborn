using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private float introDuration = 7f; // Duration of the intro (in seconds)
    [SerializeField] private float forceLoadDelay = 10f; // Force load the main menu after this delay (in seconds)
    private static bool hasPlayedIntro = false; // Static flag to ensure intro only plays once

    void Start()
    {
        // Ensure IntroManager persists across scenes
        DontDestroyOnLoad(gameObject);

        // Start coroutines to handle intro playback and forced loading
        StartCoroutine(PlayIntroAndLoadMenu());
        StartCoroutine(ForceLoadMainMenuAfterDelay());
    }

    private IEnumerator PlayIntroAndLoadMenu()
    {
        // Wait for the intro duration
        yield return new WaitForSeconds(introDuration);
        
        // Mark that the intro has been played
        hasPlayedIntro = true;
        
        // Load the main menu
        LoadMainMenu();
    }

    private IEnumerator ForceLoadMainMenuAfterDelay()
    {
        // Wait for the force load delay
        yield return new WaitForSeconds(forceLoadDelay);
        
        // Ensure main menu is loaded even if intro has not finished
        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Load the main menu scene
        Destroy(gameObject); // Optionally destroy the IntroManager once it loads the main menu
    }
}
