using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
    public Button nextButton;

    [Header("Fade UI")]
    public Image blackOverlay; // 🟥 Kéo Image đen vào đây

    [Header("After Dialogue")]
    public GameObject gameplayPrefab;

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Reset trạng thái
        leftImage.transform.localScale = Vector3.one;
        rightImage.transform.localScale = Vector3.one;
        currentLine = 0;

        // Mờ màn hình đen → rồi hiện hội thoại
        blackOverlay.color = new Color(0, 0, 0, 1); // Full đen
        blackOverlay.DOFade(0, 1f).OnComplete(() =>
        {
            ShowDialogueLine();
        });
    }

    public void OnNextClicked()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = lines[currentLine].dialogueText;
            isTyping = false;
            return;
        }

        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowDialogueLine();
        }
        else
        {
            StartCoroutine(FadeToEnd());
        }
    }

    void ShowDialogueLine()
    {
        DialogueLine line = lines[currentLine];

        leftImage.gameObject.SetActive(true);
        rightImage.gameObject.SetActive(true);

        leftImage.sprite = line.isLeftSide ? line.speakerImage : GetOtherSprite(line.speakerImage);
        rightImage.sprite = line.isLeftSide ? GetOtherSprite(line.speakerImage) : line.speakerImage;

        SetAlpha(leftImage, line.isLeftSide ? 1f : 0.3f);
        SetAlpha(rightImage, line.isLeftSide ? 0.3f : 1f);

        typingCoroutine = StartCoroutine(TypeLine(line.dialogueText));
    }

    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        isTyping = false;
    }

    IEnumerator FadeToEnd()
    {
        nextButton.interactable = false;

        // Thu nhỏ nhân vật
        Sequence shrink = DOTween.Sequence();
        shrink.Join(leftImage.transform.DOScale(Vector3.zero, 0.5f));
        shrink.Join(rightImage.transform.DOScale(Vector3.zero, 0.5f));
        yield return shrink.WaitForCompletion();

        // Tối màn hình lại
        blackOverlay.DOFade(1, 1f).OnComplete(() =>
        {
            gameObject.SetActive(false);

            if (gameplayPrefab != null)
            {
                Instantiate(gameplayPrefab, Vector3.zero, Quaternion.identity);
            }
        });
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    Sprite GetOtherSprite(Sprite current)
    {
        foreach (var l in lines)
        {
            if (l.speakerImage != current)
                return l.speakerImage;
        }
        return null;
    }
}
