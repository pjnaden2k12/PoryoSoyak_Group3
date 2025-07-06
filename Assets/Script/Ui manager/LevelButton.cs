using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public int levelIndex;
    public Button button;
    public GameObject lockIcon;

    private void Start()
    {
        UpdateButtonState();
        button.onClick.AddListener(OnClick);
    }

    public void UpdateButtonState()
    {
        bool isUnlocked = levelIndex == 0 || LevelManager.Instance.IsLevelCompleted(levelIndex - 1);

        button.interactable = isUnlocked;
        if (lockIcon != null) lockIcon.SetActive(!isUnlocked);
    }

    private void OnClick()
    {
        LevelManager.Instance.SetSelectedMapIndex(levelIndex);
        SceneManager.LoadScene("Level" + levelIndex); // Tên scene phải khớp trong Build Settings
    }
}
