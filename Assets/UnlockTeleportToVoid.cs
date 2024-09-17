using System.Collections;
using UnityEngine;

public class UnlockTeleportToVoid : MonoBehaviour
{
    [SerializeField] private GameObject particles;  // Prefab for particles
    [SerializeField] private GameObject canvasUI;   // Reference to the canvas UI to show
    private bool used;  // Flag to ensure this is only used once

    void Start()
    {
        // Check if teleportation has already been enabled
        if (PlayerController.Instance.teleportToVoid)
        {
            Destroy(gameObject);  // Destroy this object if teleportation is already enabled
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collides with this object and it hasn't been used yet
        if (collision.CompareTag("Player") && !used)
        {
            used = true;  // Mark as used to prevent re-triggering
            StartCoroutine(ShowParticlesAndEnableTeleport());
        }
    }

    private IEnumerator ShowParticlesAndEnableTeleport()
    {
        // Instantiate and destroy particles
        GameObject particleInstance = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(particleInstance, 0.5f);  // Destroy particles after 0.5 seconds

        // Wait for particles to be visible
        yield return new WaitForSeconds(0.5f);

        // Show UI message
        if (canvasUI != null)
        {
            canvasUI.SetActive(true);
            yield return new WaitForSeconds(4f);  // Show the UI for 4 seconds
            canvasUI.SetActive(false);
        }

        // Enable teleportation
        PlayerController.Instance.teleportToVoid = true;
        SaveData.Instance.SavePlayerData();  // Save player data

        // Destroy this object
        Destroy(gameObject);
    }
}
