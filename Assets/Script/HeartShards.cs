using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartShards : MonoBehaviour
{
    public Image fill;

    public float targetFillAmount;
    public float lerpDuration = 1.5f;
    public float initialFillAmount;

void Start()
{
    if (fill == null)
    {
        Debug.LogError("Fill Image is not assigned in Start.");
    }
}

public IEnumerator LerpFill()
{
    Debug.Log("Starting LerpFill coroutine");

    if (fill == null)
    {
        Debug.LogError("Fill Image is not assigned.");
        yield break;
    }

    float elapsedTime = 0f;
    while (elapsedTime < lerpDuration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / lerpDuration);

        float lerpedFillAmount = Mathf.Lerp(initialFillAmount, targetFillAmount, t);
        fill.fillAmount = lerpedFillAmount;

        if (fill.fillAmount == 1)
        {
            if (PlayerController.Instance != null)
            {
                Debug.Log("Updating PlayerController");
                PlayerController.Instance.maxHealth++;
                PlayerController.Instance.onHealthChangedCallback();
                PlayerController.Instance.heartShards = 0;
            }
            else
            {
                Debug.LogError("PlayerController instance is not assigned.");
            }
        }

        yield return null;
    }
}

}
