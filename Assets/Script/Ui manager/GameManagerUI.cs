﻿using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManagerUI : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;
    public Button homeButton;

    public TextMeshProUGUI requiredTimeText1;
    public TextMeshProUGUI requiredTimeText2;

    public TextMeshProUGUI elapsedTimeText1;
    public TextMeshProUGUI elapsedTimeText2;

    public GameObject resultPanel;
    public RectTransform resultImage;
    public Sprite winSprite;
    public Sprite loseSprite;
    public static GameManagerUI Instance;
    public GameObject gameUI;

    void Awake() => Instance = this;

    void Start()
    {
        pauseButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);  // ẩn nút home ban đầu

        if (gameUI != null)
            gameUI.SetActive(false);

        SetTwoDigitText(elapsedTimeText1, elapsedTimeText2, 0);

        GameManager.Instance.OnTimerUpdate += UpdateElapsedTime;
        GameManager.Instance.OnGameStateChanged += UpdateUIState;
        GameManager.Instance.OnGameEnded += OnGameEnded;
        GameManager.Instance.OnGameEnded += HandleGameEnded;

       
    }

    void OnEnable()
    {
        MapSpawner.OnMapSpawned += OnMapSpawnedHandler;
    }

    void OnDisable()
    {
        MapSpawner.OnMapSpawned -= OnMapSpawnedHandler;
    }

    void OnMapSpawnedHandler()
    {
        ShowGameUI();
        UpdateRequiredTimeDisplay();
    }

    void UpdateRequiredTimeDisplay()
    {
        if (GameManager.Instance != null)
        {
            int requiredTime = Mathf.CeilToInt(GameManager.Instance.RequiredTime);
            SetTwoDigitText(requiredTimeText1, requiredTimeText2, requiredTime);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdate -= UpdateElapsedTime;
            GameManager.Instance.OnGameStateChanged -= UpdateUIState;
            GameManager.Instance.OnGameEnded -= OnGameEnded;
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
        }
    }

    void OnGameEnded(bool isWin)
    {
        playButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(true);

        resultPanel.SetActive(true);
        resultImage.gameObject.SetActive(true);
        resultImage.GetComponent<Image>().sprite = isWin ? winSprite : loseSprite;

        Vector3 targetPos = resultImage.position;
        Vector3 targetScale = resultImage.localScale;

        resultImage.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        resultImage.localScale = Vector3.zero;

        resultImage.DOScale(2f, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(resultImage.DOMove(targetPos, 0.6f).SetEase(Ease.InOutSine));
            seq.Join(resultImage.DOScale(targetScale, 0.6f).SetEase(Ease.InOutSine));
        });
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
        LevelManager.Instance.ResetLevel();
        HideResultPanelAndHomeButton();
        
    }

    void HandleGameEnded(bool isWin)
    {
        DisableAllItems();
    }

    void DisableAllItems()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            var dragMove = item.GetComponent<DragMove>();
            if (dragMove != null) dragMove.enabled = false;

            var dragItem = item.GetComponent<DragItem>();
            if (dragItem != null) dragItem.enabled = false;

            var col = item.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            var sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.5f;
                sr.color = c;
            }
        }
    }

    void ShowGameUI()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(true);

            CanvasGroup cg = gameUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameUI.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.DOFade(1, 0.5f);
        }
    }
    public void HideResultPanelAndHomeButton()
    {
        if (playButton != null)
            playButton.gameObject.SetActive(true);
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (homeButton != null)
            homeButton.gameObject.SetActive(true);
        SetTwoDigitText(elapsedTimeText1, elapsedTimeText2, 0);
        
    }

}
