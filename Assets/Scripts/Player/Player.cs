using UnityEngine;

public class Player : EntityBase
{
    public override void Init(Vector2Int startPos)
    {
        base.Init(startPos);
        // Additional player-specific initialization
    }
    [Header("Player Settings")]
    [SerializeField]  private float moveSpeed = 1f;
    [SerializeField]  private float moveDuration = 0.1f; // Thời gian di chuyển giữa các ô

    private bool isMoving = false;

    private void Start()
    {
        // Gán vị trí ban đầu từ EntityBase
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    #region Input Handling Methods (Called by InputManager)
    public bool CanMove()
    {
        return !isMoving;
    }

    public void HandleMovement(Vector2Int direction)
    {
        if (!CanMove()) return;
        TryMove(direction);
    }

    public void HandleUndo()
    {
        Debug.Log("Player: Handling undo action");
        // TODO: Implement undo logic specific to player
        // Ví dụ: quay lại vị trí trước đó, hoàn tác hành động
    }

    public void HandleRestart()
    {
        Debug.Log("Player: Handling restart action");
        // TODO: Reset player to initial state
        // Ví dụ: reset vị trí, health, inventory
    }
    #endregion

    #region Movement Methods
    private void TryMove(Vector2Int dir)
    {
        Vector2Int targetGridPos = gridPosition + dir;

        // TODO: check obstacle, enemy tại targetGridPos
        StartCoroutine(MoveTo(targetGridPos));
    }

    private System.Collections.IEnumerator MoveTo(Vector2Int targetPos)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, 0);
        float elapsed = 0;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        gridPosition = targetPos;

        isMoving = false;
    }
    #endregion

    public override void OnInteract(EntityBase other)
    {
        // Handle interaction with other entities
        Debug.Log("Player interacted with " + other.name);
    }
}
