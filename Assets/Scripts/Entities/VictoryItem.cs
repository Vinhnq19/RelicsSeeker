using UnityEngine;

public class VictoryItem : EntityBase
{
    public override void OnInteract(EntityBase other)
    {
        if (other is Player)
        {
            Debug.Log("Victory Item collected! Level Complete.");
            EventBus.Publish(GameEvent.LevelCompleted);
            Destroy(gameObject); // Xóa vật phẩm sau khi nhặt
        }
    }
}
