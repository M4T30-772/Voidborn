using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private float fadeTime = 1f;
    private Image fadeOutUIImage;

    public enum FadeDirection
    {
        In,
        Out
    }

    public void CallFadeAndLoadScene(string _sceneToLoad)
    {
        StartCoroutine(FadeAndLoadScene(FadeDirection.In, _sceneToLoad));
    }

    private void Awake()
    {
        fadeOutUIImage = GetComponent<Image>();
        if (fadeOutUIImage == null)
        {
            Debug.LogError("Image component not found on SceneFader.");
        }
    }

    public IEnumerator Fade(FadeDirection _fadeDirection)
    {
        float _alpha = _fadeDirection == FadeDirection.Out ? 1 : 0;
        float _fadeEndValue = _fadeDirection == FadeDirection.Out ? 0 : 1;

        Debug.Log("Starting fade " + _fadeDirection.ToString() + " with initial alpha: " + _alpha);

        if (_fadeDirection == FadeDirection.Out)
        {
            while (_alpha >= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }

            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;

            while (_alpha <= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }

            // Ensure that the fade out image remains visible but transparent after fade in
            if (_fadeDirection == FadeDirection.In)
            {
                fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, 0);
            }
        }

        Debug.Log("Fade " + _fadeDirection.ToString() + " completed with final alpha: " + _alpha);
    }

    public IEnumerator FadeAndLoadScene(FadeDirection _fadeDirection, string _sceneToLoad)
    {
        yield return Fade(FadeDirection.Out);
        SceneManager.LoadScene(_sceneToLoad);
        yield return Fade(FadeDirection.In);
    }

    void SetColorImage(ref float _alpha, FadeDirection _fadeDirection)
    {
        // Adjust only the alpha value, keep the color the same
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, _alpha);
        
        // Update alpha based on fade direction
        _alpha += Time.deltaTime * (1 / fadeTime) * (_fadeDirection == FadeDirection.Out ? -1 : 1);
    }

    public float GetFadeTime()
    {
        return fadeTime;
    }
}
