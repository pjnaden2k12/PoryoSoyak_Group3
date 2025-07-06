using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject HelpPanel;
    public GameObject Selectlevelpanel;
    public DialogueManager dialogueManager;

    private CanvasGroup homeGroup;
    private CanvasGroup helpGroup;
    private CanvasGroup selectGroup;

    public AudioClip homeAndHelpMusic;
    public AudioClip selectLevelMusic;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public Button resetDataButton;
    public Button[] levelButtons;

    private Vector3[] originalScales;
    private const string DialogueShownKey = "DialogueShown";


    private void Start()
    {
        resetDataButton.onClick.AddListener(ResetAllPlayerPrefs);

        homeGroup = SetupCanvasGroup(HomePanel);
        helpGroup = SetupCanvasGroup(HelpPanel);
        selectGroup = SetupCanvasGroup(Selectlevelpanel);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.clip = homeAndHelpMusic;
        musicSource.volume = 0.5f;
        musicSource.Play();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        ShowPanel(homeGroup);
        HidePanel(helpGroup, false);
        HidePanel(selectGroup, false);

        CacheOriginalScales();
        SetupLevelButtons();
    }
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs đã được xóa.");

        // Optional: reload scene hoặc update UI
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private CanvasGroup SetupCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    private void ShowPanel(CanvasGroup cg, float duration = 0.5f)
    {
        cg.gameObject.SetActive(true);
        cg.transform.localScale = Vector3.one * 0.8f;
        cg.alpha = 0;
        cg.transform.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true).SetId(cg);
        cg.DOFade(1f, duration).SetEase(Ease.Linear).SetUpdate(true).SetId(cg);
    }

    private void HidePanel(CanvasGroup cg, bool deactivateAfter = true, float duration = 0.3f)
    {
        DOTween.Kill(cg);
        cg.transform.DOScale(0.8f, duration).SetEase(Ease.InBack).SetUpdate(true).SetId(cg);
        cg.DOFade(0f, duration).SetEase(Ease.Linear).SetUpdate(true).SetId(cg).OnComplete(() =>
        {
            if (deactivateAfter) cg.gameObject.SetActive(false);
        });
    }

    public void GoHelp()
    {
        HidePanel(homeGroup);
        ShowPanel(helpGroup);
    }

    public void BackHome()
    {
        HidePanel(helpGroup);
        HidePanel(selectGroup);
        ShowPanel(homeGroup);

        if (musicSource.clip != homeAndHelpMusic)
        {
            musicSource.Stop();
            musicSource.clip = homeAndHelpMusic;
            musicSource.Play();
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void GoSelectLevel()
    {
        HidePanel(homeGroup);
        MedicineAutoMove.isPlayPressed = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }

        SetupLevelButtons(); // Load lại trạng thái các level

        StartCoroutine(ShowDialogueThenSelectPanel());
    }



    private IEnumerator ShowDialogueThenSelectPanel()
    {
        // Kiểm tra nếu chưa hiện dialogue lần nào
        if (PlayerPrefs.GetInt(DialogueShownKey, 0) == 0)
        {
            dialogueManager.gameObject.SetActive(true);

            bool isDialogueDone = false;

            dialogueManager.OnStoryFinished = () =>
            {
                isDialogueDone = true;
            };

            while (!isDialogueDone)
                yield return null;

            PlayerPrefs.SetInt(DialogueShownKey, 1);
            PlayerPrefs.Save();
        }

        // PHÁT ÂM THANH SAU KHI HỘI THOẠI XONG
        musicSource.Stop();
        sfxSource.PlayOneShot(selectLevelMusic);

        ShowPanel(selectGroup);
        AnimateLevelButtons(); // nếu bạn có hiệu ứng nút
    }


    private void CacheOriginalScales()
    {
        originalScales = new Vector3[levelButtons.Length];
        for (int i = 0; i < levelButtons.Length; i++)
        {
            originalScales[i] = levelButtons[i].transform.localScale;
        }
    }

    private void AnimateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            Transform btn = levelButtons[i].transform;
            btn.localScale = Vector3.zero;

            btn.DOScale(originalScales[i], 0.4f)
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.08f);
        }
    }

    private void SetupLevelButtons()
    {
        int maxUnlockedLevel = -1;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (LevelManager.Instance.IsLevelCompleted(i))
                maxUnlockedLevel = i;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button btn = levelButtons[i];
            Transform lockIcon = btn.transform.Find("LockIcon");
            bool isUnlocked = i <= maxUnlockedLevel + 1;

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!isUnlocked);

            btn.interactable = isUnlocked;
            btn.onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                int capturedIndex = i;
                btn.onClick.AddListener(() => OnLevelButtonClicked(capturedIndex));
            }
        }
    }

    private void OnLevelButtonClicked(int index)
    {
        LevelManager.Instance.SetSelectedMapIndex(index);
        LevelManager.Instance.LoadLevel();
        HidePanel(selectGroup);

        if (musicSource.clip != homeAndHelpMusic)
        {
            musicSource.Stop();
            musicSource.clip = homeAndHelpMusic;
            musicSource.Play();
        }
    }
}
