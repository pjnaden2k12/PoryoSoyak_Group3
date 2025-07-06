using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private int selectedMapIndex = -1;
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

    public void CompleteLevel()
    {
        PlayerPrefs.SetInt("LevelCompleted_" + selectedMapIndex, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelCompleted(int mapIndex)
    {
        return PlayerPrefs.GetInt("LevelCompleted_" + mapIndex, 0) == 1;
    }
    public void LoadLevel()
    {
        if (selectedMapIndex < 0)
        {
            Debug.LogWarning("SelectedMapIndex is invalid. Cannot load level.");
            return;
        }

        MapSpawner spawner = FindFirstObjectByType<MapSpawner>();
        if (spawner == null)
        {
            Debug.LogError("MapSpawner not found in scene.");
            return;
        }

        spawner.SpawnMap();
    }
    public void ResetLevel()
    {
        MapSpawner spawner = FindFirstObjectByType<MapSpawner>();
        if (spawner != null)
        {
            spawner.ResetMap();  // Bạn cần thêm hàm ResetMap() trong MapSpawner
            spawner.SpawnMap();
        }
        else
        {
            Debug.LogError("MapSpawner not found in scene.");
        }
    }

}
