using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MenuTimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private GameObject mainMenu;   // Reference to your main menu GameObject
    [SerializeField] private Button skipButton;     // Reference to the Skip Button UI
    [SerializeField] private Text[] timelineTexts;  // References to all text elements in the timeline

    private void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.Play();
            playableDirector.stopped += OnTimelineStopped; // Register the callback
        }

        if (mainMenu != null)
        {
            mainMenu.SetActive(false); // Hide the main menu initially
        }

        // Attach the skip functionality to the button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTimeline);
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(true); // Show the main menu
        }
    }

    private void SkipTimeline()
    {
        if (playableDirector != null)
        {
            playableDirector.time = playableDirector.duration;  // Skip to the end of the timeline
            playableDirector.Stop();  // Stop the timeline
        }

        // Disable all text elements in the timeline
        foreach (var textElement in timelineTexts)
        {
            if (textElement != null)
            {
                textElement.gameObject.SetActive(false); // Hide the text element
            }
        }

        // Hide the skip button
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }
    }
}
