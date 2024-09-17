using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string transitionTo; // Represents the scene to transition to
    [SerializeField] private Transform startPoint; // Defines the player's entry point in the scene
    [SerializeField] private Vector2 exitDirection; // Specifies the direction for the player's exit
    [SerializeField] private float exitTime; // Determines the time it takes for the player to exit the scene transition

    // Start is called before the first frame update
    private void Start()
    {
        // Check if transitioning back from the same scene
        if (GameManager.Instance.transitionedFromScene == transitionTo)
        {
            // Position the player at the designated start point
            PlayerController.Instance.transform.position = startPoint.position;

            // Initiate player walk-in animation if applicable
            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDirection, exitTime));
        }
        
        // Initiate scene fade-out
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            // Record the current scene as the transitioned-from scene
            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;

            // Load the specified scene
            SceneManager.LoadScene(transitionTo);

            // Disable player controls during scene transition
            PlayerController.Instance.pState.cutscene = true;
            PlayerController.Instance.pState.invincible = true;

            // Initiate scene fade-in and load new scene
            StartCoroutine(UIManager.Instance.sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, transitionTo));
        }
    }
}
