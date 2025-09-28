using System;
using UnityEngine;

public class ObstacleBase : EntityBase
{
    [Header("Obstacle Properties")]
    [SerializeField] protected bool canBePushed = true;
    [SerializeField] protected float pushDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] protected bool debugPush = true;
    
    protected bool isBeingPushed = false;

    private void OnEnable()
    {
        EventBus.SubscribePush(GameEvent.PlayerPush, OnPushedReceived);
    }

    private void OnDisable()
    {
        EventBus.UnsubscribePush(GameEvent.PlayerPush, OnPushedReceived);
    }

    private void OnPushedReceived(PushEventData pushData)
    {
        // Kiểm tra xem obstacle này có phải target không
        if (pushData.pushed == this && canBePushed && !isBeingPushed)
        {
            StartCoroutine(PushTo(pushData.toPosition));
        }
    }

    protected virtual System.Collections.IEnumerator PushTo(Vector2Int targetPos)
    {
        isBeingPushed = true;
        
        // ✅ UPDATE GRID POSITION NGAY LẬP TỨC để tránh collision
        Vector2Int oldPos = gridPosition;
        gridPosition = targetPos;
                
        Vector3 start = transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, 0);
        float elapsed = 0;
        
        while (elapsed < pushDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / pushDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = end;
        isBeingPushed = false;
                
        // Hook cho derived classes
        OnPushCompleted();
    }
    
    protected virtual void OnPushCompleted()
    {
        // Override này trong Rock hoặc các obstacle khác nếu cần
    }
    
    public virtual bool CanBePushed()
    {
        return canBePushed && !isBeingPushed;
    }
    
    // ✅ THÊM METHOD: Expose push duration cho synchronization
    public float GetPushDuration()
    {
        return pushDuration;
    }
    
    public override void OnInteract(EntityBase other)
    {
        // Base obstacle không tương tác, chỉ bị đẩy
        if (debugPush) Debug.Log($"Obstacle {name}: Interacted with {other.name}");
    }
}
