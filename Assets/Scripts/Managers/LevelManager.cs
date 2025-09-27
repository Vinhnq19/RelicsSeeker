using UnityEngine;

// Quản lý các cấp độ trong trò chơi, Spawn level, reset level, load next level, v.v.
public class LevelManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject rockPrefab;
    public GameObject enemyPrefab;

    public void LoadLevel(int levelIndex)
    {
        Debug.Log("Load Level: " + levelIndex);

        // Example: Hardcode data, sau này đọc từ file
        SpawnEntity(EntityType.Player, new Vector2(0, 0));
        SpawnEntity(EntityType.Rock, new Vector2(1, 0));
    }

    private void SpawnEntity(EntityType type, Vector2 position)
    {
        GameObject prefab = type switch
        {
            EntityType.Player => playerPrefab,
            EntityType.Rock => rockPrefab,
            EntityType.Enemy => enemyPrefab,
            _ => null
        };

        if (prefab != null)
            Instantiate(prefab, position, Quaternion.identity);
    }
}

public enum EntityType { Player, Rock, Enemy, Chest }
