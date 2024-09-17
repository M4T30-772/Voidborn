using System.Collections;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            Debug.Log("Player hit spikes. Starting respawn sequence.");
            StartCoroutine(RespawnPlayer());
        }
    }

    IEnumerator RespawnPlayer()
    {
        Debug.Log("Entering RespawnPlayer coroutine.");

        // Update player state
        PlayerController.Instance.pState.cutscene = true;
        PlayerController.Instance.pState.invincible = true;
        PlayerController.Instance.rb.velocity = Vector2.zero;
        Time.timeScale = 1;

        Debug.Log("Player state updated. Time.timeScale set to 1.");

        // Perform fade in
        yield return StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.In));
        Debug.Log("Fade In completed.");

        // Ensure GameManager has the correct respawn point
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("GameManager instance is null!");
        }

        // Perform fade out
        yield return StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        PlayerController.Instance.TakeDamage(1);
        Debug.Log("Fade Out completed.");

        // Reset player state
        PlayerController.Instance.pState.cutscene = false;
        PlayerController.Instance.pState.invincible = false;
        Time.timeScale = 1;
        Debug.Log("Respawn sequence completed. Time.timeScale reset to 1.");
    }
}
