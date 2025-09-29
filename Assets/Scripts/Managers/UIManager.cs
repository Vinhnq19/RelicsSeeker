using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameUI;
    
    [Header("Debug")]
    [SerializeField] private bool debugUI = true;

    private void OnEnable()
    {
        EventBus.Subscribe(GameEvent.GamePaused, OnGamePaused);
        EventBus.Subscribe(GameEvent.GameResumed, OnGameResumed);
        EventBus.Subscribe(GameEvent.RestartLevel, OnRestartLevel);
    }

    private void OnDisable()
    {
        EventBus.Unsubcribe(GameEvent.GamePaused, OnGamePaused);
        EventBus.Unsubcribe(GameEvent.GameResumed, OnGameResumed);
        EventBus.Unsubcribe(GameEvent.RestartLevel, OnRestartLevel);
    }

    private void Start()
    {
        // ✅ Initialize UI state
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }

    // ✅ EVENT HANDLERS cho UI management
    private void OnGamePaused()
    {
        if (debugUI) Debug.Log("UIManager: Handling GamePaused event");
        
        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
    }

    private void OnGameResumed()
    {
        if (debugUI) Debug.Log("UIManager: Handling GameResumed event");
        
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }

    private void OnRestartLevel()
    {
        if (debugUI) Debug.Log("UIManager: Handling RestartLevel event - resetting UI");
        
        // Reset UI state khi restart
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        
        // TODO: Reset step counter, score, etc.
    }

    // ✅ PUBLIC METHODS để update UI
    public void UpdateStepCounter(int steps)
    {
        // TODO: Update step display
        if (debugUI) Debug.Log($"UIManager: Steps updated to {steps}");
    }

    public void ShowWinMessage()
    {
        // TODO: Show win popup
        if (debugUI) Debug.Log("UIManager: Showing win message");
    }
}
