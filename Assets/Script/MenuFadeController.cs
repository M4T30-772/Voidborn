using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFadeController : MonoBehaviour
{
    private FadeUI fadeUI;
    [SerializeField] private float fadeTime;

    void Start()
    {
        fadeUI = GetComponent<FadeUI>();

        if (fadeUI == null)
        {
            Debug.LogError("FadeUI component not found on the GameObject. Please ensure it is attached.");
            return;
        }

        Debug.Log("FadeUI component found. Starting fade out.");
        fadeUI.FadeUIOut(fadeTime);
    }

    public void CallFadeAndStartGame(string sceneToLoad)
    {
        if (fadeUI == null)
        {
            Debug.LogError("FadeUI component not found. Please ensure it is assigned before calling this method.");
            return;
        }

        StartCoroutine(FadeAndStartGame(sceneToLoad));
    }

    private IEnumerator FadeAndStartGame(string sceneToLoad)
    {
        fadeUI.FadeUIOut(fadeTime);
        yield return new WaitForSecondsRealtime(fadeTime);
        SceneManager.LoadScene(sceneToLoad);
    }

    void Update()
    {
    }
}
