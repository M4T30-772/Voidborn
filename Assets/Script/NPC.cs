using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    public GameObject contButton;
    [Header("Dialogue Settings")]
    public string[] dialogue;
    public float wordSpeed = 0.05f;
    private int currentDialogueIndex = 0;
    private bool playerIsClose = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && playerIsClose)
        {
            if (dialoguePanel.activeInHierarchy)
            {
                ResetDialogue();
            }
            else
            {
                ShowDialoguePanel();
            }
        }
        if (dialogueText.text == dialogue[currentDialogueIndex])
        {
            contButton.SetActive(true);
        }
    }
    private void ResetDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        dialogueText.text = string.Empty;
        currentDialogueIndex = 0;
        dialoguePanel.SetActive(false);
    }
    private void ShowDialoguePanel()
    {
        dialoguePanel.SetActive(true);
        typingCoroutine = StartCoroutine(TypeDialogue());
    }
    private IEnumerator TypeDialogue()
    {
        if (isTyping) yield break;
        isTyping = true;
        dialogueText.text = string.Empty;
        foreach (char letter in dialogue[currentDialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
        isTyping = false;
    }

    public void OnNextLine()
    {
        contButton.SetActive(false);
        if (currentDialogueIndex < dialogue.Length - 1)
        {
            currentDialogueIndex++;
            typingCoroutine = StartCoroutine(TypeDialogue());
        }
        else
        {
            ResetDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            ResetDialogue();
        }
    }
}
