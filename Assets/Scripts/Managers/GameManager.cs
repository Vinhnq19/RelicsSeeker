using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int currentLevel = 0;
    public bool isGamePaused = false;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void RestartLevel()
    {
        EventBus.Publish(GameEvent.RestartLevel);
    }
    public void LoadNextLevel()
    {
        currentLevel++;
        EventBus.Publish(GameEvent.LoadNextLevel);
    }
    public void PauseGame()
    {
        isGamePaused = true;
        EventBus.Publish(GameEvent.GamePaused);
    }
    public void ResumeGame()
    {
        isGamePaused = false;
        EventBus.Publish(GameEvent.GameResumed);
    }
}
