using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var uiShop = FindObjectOfType<UI_Shop>();
            if (uiShop != null)
            {
                uiShop.playerInRange = true;
            }
            else
            {
                Debug.LogWarning("UI_Shop instance not found in OnTriggerEnter2D.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            var uiShop = FindObjectOfType<UI_Shop>();
            if (uiShop != null)
            {
                uiShop.playerInRange = false;

                // Ensure the shop is closed if it's still open
                if (uiShop.gameObject.activeSelf)
                {
                    uiShop.CloseShop();
                }
            }
            else
            {
                Debug.LogWarning("UI_Shop instance not found in OnTriggerExit2D.");
            }
        }
    }
}
