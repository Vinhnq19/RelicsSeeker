using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    
    [Header("Input Settings")]
    [SerializeField] private bool inputEnabled = true;

    private void Start()
    {
        // Tự động tìm Player nếu chưa được gán
        if (player == null)
            player = FindFirstObjectByType<Player>();
        
        if (player == null)
            Debug.LogError("InputManager: Không tìm thấy Player component!");
    }

    #region Input Action Callbacks
    // Input System sẽ gọi các method này (setup trong PlayerInput component)
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !context.performed) return;
        
        if (player != null && player.CanMove())
        {
            Vector2 input = context.ReadValue<Vector2>();
            
            // Chuyển đổi input thành hướng grid
            Vector2Int direction = GetGridDirection(input);
            player.HandleMovement(direction);
        }
    }

    public void OnUndo(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !context.performed) return;
        
        Debug.Log("InputManager: Undo action performed");
        // Có thể delegate tới GameManager hoặc Player tùy logic game
        if (player != null)
            player.HandleUndo();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !context.performed) return;
        
        Debug.Log("InputManager: Pause action performed");
        // Delegate tới GameManager hoặc UI Manager
        HandlePause();
    }

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (!inputEnabled || !context.performed) return;
        
        Debug.Log("InputManager: Restart action performed");
        // Delegate tới GameManager
        HandleRestart();
    }
    #endregion

    #region Helper Methods
    private Vector2Int GetGridDirection(Vector2 input)
    {
        // Chỉ nhận hướng grid (4 hướng chính)
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            return input.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    private void HandlePause()
    {
        // TODO: Implement pause logic
        // Có thể gọi GameManager.Instance.TogglePause();
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    private void HandleRestart()
    {
        // TODO: Implement restart logic
        // Có thể gọi GameManager.Instance.RestartLevel();
        if (player != null)
            player.HandleRestart();
    }
    #endregion

    #region Public Methods
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public bool IsInputEnabled()
    {
        return inputEnabled;
    }

    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
    }
    #endregion
}
