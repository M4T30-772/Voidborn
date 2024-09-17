using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip forestMusic;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip townMusic;
    [SerializeField] private AudioClip castleMusic;
    [SerializeField] private AudioClip greenMusic;
    [SerializeField] private AudioClip caveMusic;
    [SerializeField] private AudioClip graveyardMusic;
    [SerializeField] private AudioClip voidMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        if (FindObjectsOfType<MusicManager>().Length > 1)
        {
            Destroy(gameObject); // Ensure only one MusicManager exists
        }
        else
        {
            DontDestroyOnLoad(gameObject); // Persist across scenes
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true; // Ensure the music loops
            PlayMusicForCurrentScene();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Castle"))
        {
            audioSource.clip = castleMusic;
        }
        else if (sceneName.StartsWith("Green"))
        {
            audioSource.clip = greenMusic;
        }
        else if (sceneName.StartsWith("Cave"))
        {
            audioSource.clip = caveMusic;
        }
        else if (sceneName.StartsWith("Graveyard"))
        {
            audioSource.clip = graveyardMusic;
        }
        else if (sceneName.StartsWith("Void"))
        {
            audioSource.clip = voidMusic;
        }
        else if (sceneName.StartsWith("Forest"))
        {
            audioSource.clip = forestMusic;
        }
        else if (sceneName.StartsWith("Battle"))
        {
            audioSource.clip = battleMusic;
        }
        else if (sceneName.StartsWith("Town"))
        {
            audioSource.clip = townMusic;
        }
        else
        {
            audioSource.clip = null;
        }

        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
