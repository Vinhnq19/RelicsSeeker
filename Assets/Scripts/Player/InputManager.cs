using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInputSet inputActions;

    [Header("References")]
    [SerializeField] private Player player;

    [Header("Input Settings")]
    [SerializeField] private bool inputEnabled = true;

    [Header("Debug")]
    private bool debugInput = true;

    private void Awake()
    {
        // Tạo input actions
        inputActions = new PlayerInputSet();

        // Bind input actions
        inputActions.Player.Movement.performed += OnMovementInput;
        inputActions.Player.Undo.performed += OnUndoInput;
        inputActions.Player.Pause.performed += OnPauseInput;
        inputActions.Player.Restart.performed += OnRestartInput;
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void Start()
    {
        // Tự động tìm Player nếu chưa được gán
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (player == null)
            Debug.LogError("InputManager: Không tìm thấy Player component!");
    }

    private void OnDestroy()
    {
        // Cleanup input actions
        if (inputActions != null)
        {
            inputActions.Player.Movement.performed -= OnMovementInput;
            inputActions.Player.Undo.performed -= OnUndoInput;
            inputActions.Player.Pause.performed -= OnPauseInput;
            inputActions.Player.Restart.performed -= OnRestartInput;
        }
    }

    #region Input Action Handlers
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;

        Vector2 input = context.ReadValue<Vector2>();

        if (player != null && player.CanMove())
        {
            Vector2Int direction = GetGridDirection(input);
            player.HandleMovement(direction);
        }
        else if (debugInput)
        {
            if (player == null) Debug.Log("InputManager: Player is null!");
            else Debug.Log("InputManager: Player cannot move right now");
        }
    }

    private void OnUndoInput(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;

        HandleUndo();
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;

        HandlePause();
    }

    private void OnRestartInput(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
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
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isGamePaused)
                GameManager.Instance.ResumeGame();
            else
                GameManager.Instance.PauseGame();
        }
    }


    private void HandleRestart()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartLevel();
    }

    private void HandleUndo()
    {
        Debug.Log("InputManager: Handling undo action");
        if (GameManager.Instance != null)
            EventBus.Publish(GameEvent.UndoAction);
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
