using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UiManager : MonoBehaviour
{
    public GameObject HomePanel;
    public GameObject HelpPanel;
    public GameObject Selectlevelpanel;

    private CanvasGroup homeGroup;
    private CanvasGroup helpGroup;
    private CanvasGroup selectGroup;

    public AudioClip homeAndHelpMusic;
    public AudioClip selectLevelMusic;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Levels trong scene (bật/tắt theo chọn)")]
    public GameObject[] levels;

    private int currentLevelIndex = -1;

    private void Start()
    {
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

        UpdateLevelButtons();
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
        ShowPanel(selectGroup);

        musicSource.Stop();
        sfxSource.PlayOneShot(selectLevelMusic);

        UpdateLevelButtons();
    }

    // Cập nhật trạng thái các nút level (mở khóa hoặc khóa)
    public void UpdateLevelButtons()
    {
        Button[] buttons = Selectlevelpanel.GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            string name = btn.gameObject.name;
            if (name.StartsWith("LevelButton_"))
            {
                int levelIndex;
                if (int.TryParse(name.Substring("LevelButton_".Length), out levelIndex))
                {
                    // Level 0 luôn mở khóa, level tiếp theo mở khi level trước hoàn thành
                    bool unlocked = levelIndex == 0 || PlayerPrefs.GetInt("LevelCompleted_" + (levelIndex - 1), 0) == 1;

                    btn.interactable = unlocked;

                    // Tìm icon lock (con của nút)
                    Transform lockIcon = btn.transform.Find("Lock");
                    if (lockIcon != null)
                    {
                        // Ẩn icon lock nếu mở khóa, hiện nếu khóa
                        lockIcon.gameObject.SetActive(!unlocked);
                    }

                    btn.onClick.RemoveAllListeners();
                    int capturedIndex = levelIndex;
                    btn.onClick.AddListener(() => OnLevelButtonClicked(capturedIndex));
                }
            }
        }
    }

    // Khi người chơi bấm nút level
    public void OnLevelButtonClicked(int levelIndex)
    {
        currentLevelIndex = levelIndex;

        // Gán level được chọn trong LevelManager
        LevelManager.Instance.SetSelectedMapIndex(levelIndex);

        // Gọi LoadLevel của LevelManager để kích hoạt level tương ứng
        LevelManager.Instance.LoadLevel();

        HidePanel(selectGroup);
        // Hiện panel gameplay nếu có
    }

    // Gọi khi người chơi hoàn thành level hiện tại
    public void CompleteCurrentLevel()
    {
        if (currentLevelIndex < 0) return;

        // Gọi hàm lưu level hoàn thành trong LevelManager
        LevelManager.Instance.CompleteLevel();

        // Mở khóa level tiếp theo bằng PlayerPrefs (cơ chế mở khóa vẫn dựa vào completed của level trước)
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel < levels.Length)
        {
            PlayerPrefs.SetInt("LevelCompleted_" + currentLevelIndex, 1);
            PlayerPrefs.Save();
        }

        UpdateLevelButtons();
    }
}
