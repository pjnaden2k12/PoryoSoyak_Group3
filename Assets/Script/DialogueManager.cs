using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerImage;
        [TextArea(2, 5)]
        public string dialogueText;
        public bool isLeftSide;
    }

    [Header("Dialogue Data")]
    public DialogueLine[] lines;

    [Header("UI Elements")]
    public Image leftImage;
    public Image rightImage;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton;

    [Header("After Dialogue")]
    public GameObject gameplayPrefab; // Kéo Prefab màn chơi vào đây

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        currentLine = 0;
        ShowDialogueLine();
    }

    public void OnNextClicked()
    {
        // Nếu đang gõ thì bỏ qua và hiện toàn bộ dòng luôn
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = lines[currentLine].dialogueText;
            isTyping = false;
            return;
        }

        // Nếu chưa hết thoại thì chuyển sang dòng tiếp
        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowDialogueLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void ShowDialogueLine()
    {
        DialogueLine line = lines[currentLine];

        // Bật cả 2 ảnh
        leftImage.gameObject.SetActive(true);
        rightImage.gameObject.SetActive(true);

        // Gán ảnh cho cả 2 bên
        leftImage.sprite = (lines.Length > currentLine) ? lines[0].speakerImage : null;
        rightImage.sprite = (lines.Length > currentLine) ? lines[1].speakerImage : null;

        // Gán ảnh cho đúng nhân vật nói
        if (line.isLeftSide)
        {
            leftImage.sprite = line.speakerImage;
        }
        else
        {
            rightImage.sprite = line.speakerImage;
        }

        // Làm mờ bên không nói
        if (line.isLeftSide)
        {
            leftImage.color = new Color(1f, 1f, 1f, 1f); // ảnh rõ
            rightImage.color = new Color(1f, 1f, 1f, 0.4f); // mờ
        }
        else
        {
            rightImage.color = new Color(1f, 1f, 1f, 1f);
            leftImage.color = new Color(1f, 1f, 1f, 0.4f);
        }

        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
    }


    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        gameObject.SetActive(false); // Ẩn khung hội thoại

        if (gameplayPrefab != null)
        {
            Instantiate(gameplayPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
