using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Include this for UI Text
using System.Collections; // Include this for IEnumerator
public class Bench : MonoBehaviour
{
    public Text pressFText; // Reference to the "Press F to save" UI Text
    public Text gameSavedText; // Reference to the "Game saved" UI Text

    public bool interacted;
    private bool saving;

    private void Start()
    {
        if (pressFText != null)
        {
            pressFText.gameObject.SetActive(false); // Hide pressFText at start
        }
        if (gameSavedText != null)
        {
            gameSavedText.gameObject.SetActive(false); // Hide gameSavedText at start
        }
        Debug.Log("Bench object started.");
    }

private void Update()
{
    if (interacted && Input.GetKeyDown(KeyCode.F))
    {
        Debug.Log("F key pressed.");
        SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.benchPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        SaveData.Instance.SaveBench();
        SaveData.Instance.SavePlayerData();
        
    if (interacted && Input.GetKeyDown(KeyCode.F))
    {
        Debug.Log("F key pressed.");
        
        // Existing save logic
        
        // Hide pressFText
        if (pressFText != null)
        {
            Debug.Log("Deactivating pressFText");
            pressFText.gameObject.SetActive(false); // Hide pressFText
        }
        
        // Existing gameSavedText logic
    }
        
        // Show and hide gameSavedText
        if (gameSavedText != null)
        {
            gameSavedText.gameObject.SetActive(true); // Show gameSavedText
            StartCoroutine(HideGameSavedTextAfterDelay(2f)); // Hide after delay
        }
        
        saving = false; // Reset saving state
    }
}

    private void OnTriggerStay2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            interacted = true;
            if (pressFText != null)
            {
                pressFText.gameObject.SetActive(true); // Show pressFText
            }
            Debug.Log("Player is interacting with the bench.");
        }
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            interacted = false;
            if (pressFText != null)
            {
                pressFText.gameObject.SetActive(false); // Hide pressFText
            }
            if (gameSavedText != null)
            {
                gameSavedText.gameObject.SetActive(false); // Hide gameSavedText if player exits
            }
            Debug.Log("Player exited the bench interaction zone.");
        }
    }

    private IEnumerator HideGameSavedTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameSavedText != null)
        {
            gameSavedText.gameObject.SetActive(false); // Hide gameSavedText after delay
        }
    }
}
