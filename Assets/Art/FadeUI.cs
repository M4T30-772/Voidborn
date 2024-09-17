using System.Collections;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeUIIn(float seconds)
    {
        StartCoroutine(FadeIn(seconds));
    }

    public void FadeUIOut(float seconds)
    {
        StartCoroutine(FadeOut(seconds));
    }

    private IEnumerator FadeOut(float seconds)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / seconds;
            yield return null;
        }
        canvasGroup.alpha = 0; // Ensure alpha is set to 0 at the end
    }

    private IEnumerator FadeIn(float seconds)
    {
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / seconds;
            yield return null;
        }
        canvasGroup.alpha = 1; // Ensure alpha is set to 1 at the end
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialization code here (if needed)
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic here (if needed)
    }
}
