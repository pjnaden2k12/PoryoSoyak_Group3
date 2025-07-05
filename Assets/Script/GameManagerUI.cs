using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerUI : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;

    public TextMeshProUGUI requiredTimeText1;
    public TextMeshProUGUI requiredTimeText2;

    public TextMeshProUGUI elapsedTimeText1;
    public TextMeshProUGUI elapsedTimeText2;

    void Start()
    {
        pauseButton.gameObject.SetActive(false);

        SetTwoDigitText(requiredTimeText1, requiredTimeText2, Mathf.CeilToInt(GameManager.Instance.requiredTime));
        SetTwoDigitText(elapsedTimeText1, elapsedTimeText2, 0);

        GameManager.Instance.OnTimerUpdate += UpdateElapsedTime;
        GameManager.Instance.OnGameStateChanged += UpdateUIState;
        GameManager.Instance.OnGameEnded += OnGameEnded;

    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdate -= UpdateElapsedTime;
            GameManager.Instance.OnGameStateChanged -= UpdateUIState;
            GameManager.Instance.OnGameEnded -= OnGameEnded;

        }
    }
    void OnGameEnded(bool isWin)
    {
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
    }

    void UpdateElapsedTime(int seconds)
    {
        SetTwoDigitText(elapsedTimeText1, elapsedTimeText2, seconds);
    }

    void SetTwoDigitText(TextMeshProUGUI text1, TextMeshProUGUI text2, int number)
    {
        number = Mathf.Clamp(number, 0, 99);
        int tens = number / 10;
        int ones = number % 10;
        text1.text = tens.ToString();
        text2.text = ones.ToString();
    }

    void UpdateUIState(bool isPlaying)
    {
        playButton.gameObject.SetActive(!isPlaying);
        pauseButton.gameObject.SetActive(isPlaying);
        UpdateItemsState(isPlaying);
    }

    public void OnPlayPauseButtonPressed()
    {
        playButton.transform.DOScale(1.1f, 0.1f).OnComplete(() => {
            playButton.transform.DOScale(1f, 0.1f);
        });

        pauseButton.transform.DOScale(1.1f, 0.1f).OnComplete(() => {
            pauseButton.transform.DOScale(1f, 0.1f);
        });

        GameManager.Instance.TogglePlayPause();
    }

    void UpdateItemsState(bool isPlaying)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            DragMove dragMove = item.GetComponent<DragMove>();
            DragItem dragItem = item.GetComponent<DragItem>();

            bool isSnapped = false;
            if (dragMove != null) isSnapped = dragMove.isSnapped;
            if (dragItem != null) isSnapped = dragItem.isSnapped;

            if (isPlaying)
            {
                if (!isSnapped)
                {
                    if (dragMove != null) dragMove.enabled = false;
                    if (dragItem != null) dragItem.enabled = false;

                    Collider2D col = item.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 0.4f;
                        sr.color = c;
                    }
                }
                else
                {
                    if (dragMove != null) dragMove.enabled = false;
                    if (dragItem != null) dragItem.enabled = false;

                    Collider2D col = item.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true;

                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 1f;
                        sr.color = c;
                    }
                }
            }
            else
            {
                if (dragMove != null) dragMove.enabled = true;
                if (dragItem != null) dragItem.enabled = true;

                Collider2D col = item.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f;
                    sr.color = c;
                }
            }
        }
    }

    public void OnReplayButtonPressed()
    {
        MedicineAutoMove.isPlayPressed = false;
        DOTween.KillAll();
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
