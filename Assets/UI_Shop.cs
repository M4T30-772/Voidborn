using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UI_Shop : MonoBehaviour
{
    private Transform Shop;
    private Transform ShopItemTemplate;
    public bool playerInRange;  

    public int weaponUpgradeCost = 3000;
    public int spellDamageUpgradeCost = 4500;
    public int healthShardUpgradeCost = 2700;

    [SerializeField] private Button closeButton; 

    private void Awake()
    {
        Debug.Log("UI_Shop script initialized.");
        Shop = transform.Find("Shop");
        ShopItemTemplate = Shop.Find("ShopItemTemplate");
        ShopItemTemplate.gameObject.SetActive(false);
        Shop.gameObject.SetActive(false); 

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop); 
        }
        else
        {
            Debug.LogWarning("Close button is not assigned in the inspector!");
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed and player in range.");
            OpenShop();
        }

        if (Shop.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC key pressed. Closing shop.");
            CloseShop();
        }
    }

    private void OpenShop()
    {
        Shop.gameObject.SetActive(true);
        ActivateAllChildren(Shop);
    }

    public void CloseShop()
    {
        Shop.gameObject.SetActive(false);
    }

    private void ActivateAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(true);
            ActivateAllChildren(child);
        }
    }

    public void BuyWeaponUpgrade()
    {
        if (SaveData.Instance.money >= weaponUpgradeCost)
        {
            SaveData.Instance.money -= weaponUpgradeCost;
            PlayerController.Instance.damage += 1;
            SaveData.Instance.weaponDamage += 1;
            SaveData.Instance.SavePlayerData(); 

            Debug.Log("Weapon upgraded! Current weapon damage: " + SaveData.Instance.weaponDamage);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public void BuySpellDamageUpgrade()
    {
        if (SaveData.Instance.money >= spellDamageUpgradeCost)
        {
            SaveData.Instance.money -= spellDamageUpgradeCost;
            PlayerController.Instance.spellDamage += 1; 
            SaveData.Instance.spellDamage += 1;
            SaveData.Instance.SavePlayerData(); 

            Debug.Log("Spell damage upgraded! Current spell damage: " + SaveData.Instance.spellDamage);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public void BuyHealthShard()
    {
        if (SaveData.Instance.money >= healthShardUpgradeCost)
        {
            SaveData.Instance.money -= healthShardUpgradeCost;
            PlayerController.Instance.heartShards += 1;
            SaveData.Instance.playerHeartShards += 1;
            SaveData.Instance.SavePlayerData(); 

            Debug.Log("Health shard purchased! Total health shards: " + SaveData.Instance.playerHeartShards);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}
