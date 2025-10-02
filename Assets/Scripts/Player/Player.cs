using UnityEngine;

public class Player : EntityBase
{
    private SpriteRenderer sr;
    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveDuration = 0.1f; // Thời gian di chuyển giữa các ô
    [SerializeField] private Vector2Int startingPosition = new Vector2Int(0, 0); // Vị trí bắt đầu

    
    [Header("Debug")]
    [SerializeField] private bool debugMovement = true;

    private bool isMoving = false;
    private bool isFacingRight = false; // Hướng mặt hiện tại của Player

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }
    public override void Init(Vector2Int startPos)
    {
        base.Init(startPos);
    }

    private void Start()
    {
        // CHỈ auto-init nếu gridPosition chưa được set (tránh override LevelManager init)
        if (gridPosition == Vector2Int.zero)
        {
            if (startingPosition != Vector2Int.zero)
            {
                Init(startingPosition);
            }
            else
            {
                // Sử dụng vị trí hiện tại của transform nếu có
                Vector2Int currentPos = new Vector2Int(
                    Mathf.RoundToInt(transform.position.x),
                    Mathf.RoundToInt(transform.position.y)
                );
                Init(currentPos);
            }
        }

        // Đảm bảo transform position khớp với grid position
        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    #region Input Handling Methods (Called by InputManager)
    public bool CanMove()
    {
        bool canMove = !isMoving;
        if (debugMovement && !canMove) Debug.Log("Player: Cannot move - already moving");
        return canMove;
    }

    public void HandleMovement(Vector2Int direction)
    {
        if (!CanMove()) return;
        
            HandleFlip(direction.x);
        TryMove(direction);
    }

    #endregion

    #region Movement Methods
    private void TryMove(Vector2Int dir)
    {
        if (Time.time < 0.1f) // Trong 0.1s đầu game
        {
            return;
        }

        Vector2Int targetGridPos = gridPosition + dir;

        //Kiểm tra xem có vật cản không
        EntityBase targetEntity = GetEntityAtPosition(targetGridPos);

        if (targetEntity != null)
        {

            if (targetEntity is ObstacleBase obstacleBase && obstacleBase.CanBePushed())
            {
                Vector2Int pushToPos = targetGridPos + dir; // Vị trí obstacle sẽ được đẩy tới

                // Kiểm tra vị trí đẩy có trống không
                if (IsPositionFree(pushToPos))
                {
                    // Phát push event
                    PushEventData pushData = new PushEventData(dir, targetGridPos, pushToPos, this, obstacleBase);
                    EventBus.PublishPush(GameEvent.PlayerPush, pushData);
                    StartCoroutine(MoveToWithDelay(targetGridPos, obstacleBase.GetPushDuration()));
                }
            }
            if (targetEntity is VictoryItem victoryItem)
            {
                StartCoroutine(MoveTo(targetGridPos));
                victoryItem.OnInteract(this);
                return;
            }
        }
        else
        {
            // Vị trí trống, di chuyển bình thường
            StartCoroutine(MoveTo(targetGridPos));
        }
    }

    private EntityBase GetEntityAtPosition(Vector2Int pos)
    {
        // Tìm entity tại vị trí chỉ định
        EntityBase[] entities = FindObjectsByType<EntityBase>(FindObjectsSortMode.None);

        foreach (var entity in entities)
        {
            if (entity.gridPosition == pos && entity != this)
            {
                return entity;
            }
        }
        return null;
    }

    private bool IsPositionFree(Vector2Int pos)
    {
        return GetEntityAtPosition(pos) == null;
    }


    private System.Collections.IEnumerator MoveTo(Vector2Int targetPos) //Move to target grid position
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

    // Movement với delay cho push synchronization
    private System.Collections.IEnumerator MoveToWithDelay(Vector2Int targetPos, float pushDuration)
    {
        isMoving = true;

        // Chờ một chút để obstacle bắt đầu di chuyển trước
        yield return new WaitForSeconds(pushDuration * 0.1f);

        Vector3 start = transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, 0);
        float elapsed = 0;

        // Sync movement duration với push duration (nhưng không quá chậm)
        float actualMoveDuration = Mathf.Max(moveDuration, pushDuration * 0.8f);

        while (elapsed < actualMoveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / actualMoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        gridPosition = targetPos;


        isMoving = false;
    }
    #endregion

    #region Visual Methods
    private void HandleFlip(float xDirection)
    {
        if (xDirection > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (xDirection < 0 && isFacingRight)
        {
            Flip();
        }
    }
    
    public void Flip()
    {
        sr.flipX = !sr.flipX;
        isFacingRight = !isFacingRight;
    }
    
    public override void OnInteract(EntityBase other)
    {
        // Handle interaction with other entities
        Debug.Log("Player interacted with " + other.name);
    }
    #endregion
}
