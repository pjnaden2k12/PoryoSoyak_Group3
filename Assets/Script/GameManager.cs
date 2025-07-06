using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float requiredTime;

    private float elapsedTime = 0f;
    private bool isGamePlaying = false;

    public static GameManager Instance;

    public delegate void TimerUpdate(int seconds);
    public event TimerUpdate OnTimerUpdate;

    public delegate void GameStateChanged(bool isPlaying);
    public event GameStateChanged OnGameStateChanged;
    public delegate void GameResult(bool isWin); // true = win, false = lose
    public event GameResult OnGameEnded;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!isGamePlaying) return;

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
        elapsedTime = 0f;
        isGamePlaying = true;
        MedicineAutoMove.isPlayPressed = true;
        OnGameStateChanged?.Invoke(true);
        OnTimerUpdate?.Invoke(0);
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
        elapsedTime = 0f;
        isGamePlaying = false;
        MedicineAutoMove.isPlayPressed = false;
        OnGameStateChanged?.Invoke(false);
        OnTimerUpdate?.Invoke(0);
    }

    public bool IsPlaying()
    {
        return isGamePlaying;
    }
}
