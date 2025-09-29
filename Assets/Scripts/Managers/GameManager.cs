using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    private void OnEnable()
    {
        EventBus.Subscribe(GameEvent.PauseToggle, OnPauseToggleRequested);
    }

    private void OnDisable()
    {
        EventBus.Unsubcribe(GameEvent.PauseToggle, OnPauseToggleRequested);
    }

    private void OnPauseToggleRequested()
    {
        if (isGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void RestartLevel()
    {
        EventBus.Publish(GameEvent.RestartLevel);
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
    public void UndoAction()
    {
        EventBus.Publish(GameEvent.UndoAction);
    }
}
