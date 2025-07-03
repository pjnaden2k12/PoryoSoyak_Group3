using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public Sprite portrait;
        [TextArea(2, 4)] public string sentence;
    }

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Dialogue Lines")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Màn chơi")]
    public GameObject levelPrefab; // Prefab hoặc container của màn chơi

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        dialoguePanel.SetActive(true);          // Hiện khung thoại
        if (levelPrefab != null) levelPrefab.SetActive(false); // Ẩn màn chơi
        StartDialogue();
    }

    public void StartDialogue()
    {
        currentLine = 0;
        ShowLine();
    }

    void ShowLine()
    {
        if (currentLine >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentLine];
        nameText.text = line.speaker;
        portraitImage.sprite = line.portrait;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void OnNextPressed()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = dialogueLines[currentLine].sentence;
            isTyping = false;
        }
        else
        {
            currentLine++;
            ShowLine();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (levelPrefab != null)
        {
            levelPrefab.SetActive(true); // 👉 hiện màn chơi sau khi thoại xong
        }

        Debug.Log("✅ Hội thoại kết thúc - Màn chơi đã xuất hiện.");
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            OnNextPressed();
        }
    }
}
