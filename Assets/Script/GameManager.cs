using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameMapList mapList;

    private MapData currentMapData;
    private float elapsedTime = 0f;
    private bool isGamePlaying = false;

    private float requiredTime;

    public static GameManager Instance;

    public delegate void TimerUpdate(int seconds);
    public event TimerUpdate OnTimerUpdate;

    public delegate void GameStateChanged(bool isPlaying);
    public event GameStateChanged OnGameStateChanged;

    public delegate void GameResult(bool isWin);
    public event GameResult OnGameEnded;

    public float RequiredTime => requiredTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetCurrentMap(int mapIndex)
    {
        if (mapList == null || mapIndex < 0 || mapIndex >= mapList.allMaps.Length)
        {
            Debug.LogError("Invalid map index");
            currentMapData = null;
            return;
        }

        currentMapData = mapList.allMaps[mapIndex];
        SetPlayTimeLimit(currentMapData.playTimeLimit);
        ResetTimer();
    }

    public void SetPlayTimeLimit(float limit)
    {
        requiredTime = limit;
    }

    void Update()
    {
        if (!isGamePlaying || currentMapData == null) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime > requiredTime)
            elapsedTime = requiredTime;

        int seconds = Mathf.FloorToInt(elapsedTime);
        OnTimerUpdate?.Invoke(seconds);

        if (!AreAnyMedicineTagsPresent())
        {
            WinGame();
            return;
        }

        if (elapsedTime >= requiredTime)
        {
            if (AreAnyMedicineTagsPresent())
                LoseGame();
            else
                WinGame();
        }
    }

    bool AreAnyMedicineTagsPresent()
    {
        return GameObject.FindWithTag("mdCircle") != null
            || GameObject.FindWithTag("mdSquare") != null
            || GameObject.FindWithTag("mdTriangle") != null;
    }

    void WinGame()
    {
        if (!isGamePlaying) return;
        isGamePlaying = false;
        MedicineAutoMove.isPlayPressed = false;
        OnGameStateChanged?.Invoke(false);
        OnGameEnded?.Invoke(true);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteLevel();
        }
        Debug.Log("Win! Không còn medicine tag nào.");
    }

    void LoseGame()
    {
        if (!isGamePlaying) return;
        isGamePlaying = false;
        MedicineAutoMove.isPlayPressed = false;
        OnGameStateChanged?.Invoke(false);
        OnGameEnded?.Invoke(false);
        Debug.Log("Lose! Vẫn còn medicine tag khi hết thời gian.");
    }

    public void StartGame()
    {
        if (currentMapData == null)
        {
            Debug.LogError("Không có map hiện tại, không thể bắt đầu game.");
            return;
        }
        isGamePlaying = true;
        MedicineAutoMove.isPlayPressed = true;
        OnGameStateChanged?.Invoke(true);
        OnTimerUpdate?.Invoke(Mathf.FloorToInt(elapsedTime));
    }

    public void PauseGame()
    {
        isGamePlaying = false;
        MedicineAutoMove.isPlayPressed = false;
        OnGameStateChanged?.Invoke(false);
    }

    public void TogglePlayPause()
    {
        if (isGamePlaying) PauseGame();
        else StartGame();
    }

    public void ResetGame()
    {
        ResetTimer();
        isGamePlaying = false;
        MedicineAutoMove.isPlayPressed = false;
        OnGameStateChanged?.Invoke(false);
        OnTimerUpdate?.Invoke(0);
    }

    private void ResetTimer()
    {
        elapsedTime = 0f;
    }

    public bool IsPlaying()
    {
        return isGamePlaying;
    }
}
