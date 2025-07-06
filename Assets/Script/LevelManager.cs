using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private int selectedMapIndex = 0;
    public int SelectedMapIndex => selectedMapIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedMapIndex(int index)
    {
        selectedMapIndex = index;
    }

    public void LoadLevelScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CompleteLevel()
    {
        PlayerPrefs.SetInt("LevelCompleted_" + selectedMapIndex, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelCompleted(int mapIndex)
    {
        return PlayerPrefs.GetInt("LevelCompleted_" + mapIndex, 0) == 1;
    }
}
