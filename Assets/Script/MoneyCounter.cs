using UnityEngine;
using TMPro; // Make sure to include the TextMeshPro namespace

public class MoneyCounter : MonoBehaviour
{
    private TextMeshProUGUI txt;

    private void Awake()
    {
        // Get the TextMeshProUGUI component instead of the Text component
        txt = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Update the displayed text with the current amount of souls
        txt.text = SaveData.Instance.money + " souls";

        // Check for key presses
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddSouls(100);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SubtractSouls(100);
        }
    }

    private void AddSouls(int amount)
    {
        SaveData.Instance.money += amount;
    }

    private void SubtractSouls(int amount)
    {
        // Ensure that souls don't go below 0
        SaveData.Instance.money = Mathf.Max(SaveData.Instance.money - amount, 0);
    }
}
