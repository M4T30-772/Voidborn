using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseMaxHealth : MonoBehaviour
{
    [SerializeField] GameObject particles;
    [SerializeField] GameObject canvasUI;
    [SerializeField] HeartShards heartShards;

    bool used;

    void Start()
    {
        if (PlayerController.Instance.maxHealth >= PlayerController.Instance.maxTotalHealth)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player") && !used)
        {
            used = true;
            Debug.Log("Player entered trigger zone for IncreaseMaxHealth.");
            StartCoroutine(ShowUI());
        }
    }

    IEnumerator ShowUI()
    {
        Debug.Log("ShowUI coroutine started.");
        GameObject _particles = Instantiate(particles, transform.position, Quaternion.identity);
        Destroy(_particles, 0.5f);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.5f);

        canvasUI.SetActive(true);
        heartShards.initialFillAmount = PlayerController.Instance.heartShards * 0.25f;
        PlayerController.Instance.heartShards++;
        heartShards.targetFillAmount = PlayerController.Instance.heartShards * 0.25f;

        StartCoroutine(heartShards.LerpFill());

        yield return new WaitForSeconds(2.5f);
        SaveData.Instance.SavePlayerData();
        canvasUI.SetActive(false);
        Destroy(gameObject);
    }
}
