using UnityEngine;

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
            return;

        MapSpawner spawner = FindFirstObjectByType<MapSpawner>();
        if (spawner != null)
        {
            spawner.SpawnMap();

            // Tắt UI kết quả (win/lose) nếu có
            if (GameManagerUI.Instance != null)
            {
                GameManagerUI.Instance.HideResultPanelAndHomeButton();
            }
        }
    }
    public void DestroyMap()
    {
        MapSpawner spawner = FindFirstObjectByType<MapSpawner>();
        if (spawner != null)
        {
            spawner.ResetMap();  // Xóa tất cả các đối tượng hiện tại của bản đồ
        }
    }

    public void ResetLevel()
    {
        MapSpawner spawner = FindFirstObjectByType<MapSpawner>();
        if (spawner != null)
        {
            spawner.ResetMap();
            spawner.SpawnMap();
        }
    }
}
