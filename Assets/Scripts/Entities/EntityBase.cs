using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    public Vector2Int gridPosition;

    public virtual void Init(Vector2Int startPos)
    {
        gridPosition = startPos;
        transform.position = new Vector3(startPos.x, startPos.y, 0);
    }

    public abstract void OnInteract(EntityBase other);
}
