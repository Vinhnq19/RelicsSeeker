using UnityEngine;

[System.Serializable]
public class PushEventData
{
    public Vector2Int pushDirection;
    public Vector2Int fromPosition;
    public Vector2Int toPosition;
    public EntityBase pusher;      // Ai đang đẩy
    public EntityBase pushed;      // Cái gì được đẩy
    
    public PushEventData(Vector2Int direction, Vector2Int from, Vector2Int to, 
                        EntityBase pusher, EntityBase pushed)
    {
        this.pushDirection = direction;
        this.fromPosition = from;
        this.toPosition = to;
        this.pusher = pusher;
        this.pushed = pushed;
    }
}