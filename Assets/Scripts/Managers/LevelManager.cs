using System;
using UnityEngine;

// Quản lý các cấp độ trong trò chơi, Spawn level, reset level, load next level, v.v.
public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public TextAsset[] levelFiles; 
    private int currentLevelIndex = 0;
    public GameObject playerPrefab;
    public GameObject obstaclePrefab;
    public GameObject enemyPrefab;
    public GameObject victoryItemPrefab;
    public GameObject chestPrefab;

    [Header("Camera")]
    [SerializeField] private bool autoSetupCamera = true;
    [SerializeField] private float cameraPadding = 1.5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugLevel = true;
    
    // Public properties để các script khác có thể truy cập
    public int CurrentLevelIndex => currentLevelIndex;
    public int TotalLevels => levelFiles?.Length ?? 0;
    public bool HasNextLevel => currentLevelIndex + 1 < TotalLevels;

    private void OnEnable()
    {
        EventBus.Subscribe(GameEvent.RestartLevel, OnRestartLevel);
        EventBus.Subscribe(GameEvent.LevelCompleted, OnLevelCompleted);
    }


    private void OnDisable()
    {
        EventBus.Unsubcribe(GameEvent.RestartLevel, OnRestartLevel);
        EventBus.Unsubcribe(GameEvent.LevelCompleted, OnLevelCompleted);
    }

    private void Start()
    {
        LoadLevel(0); // Load level 0
    }

    public void LoadLevel(int levelIndex)
    {
        if (debugLevel) Debug.Log($"LevelManager: Loading Level {levelIndex}");
        
        // Validate level files array
        if (levelFiles == null || levelFiles.Length == 0)
        {
            Debug.LogError("LevelManager: No level files assigned!");
            return;
        }
        
        // Validate level index
        if (levelIndex < 0 || levelIndex >= levelFiles.Length)
        {
            Debug.LogError($"LevelManager: Invalid level index {levelIndex}. Available levels: 0-{levelFiles.Length - 1}");
            return;
        }
        
        // Validate level file content
        if (levelFiles[levelIndex] == null)
        {
            Debug.LogError($"LevelManager: Level file at index {levelIndex} is null!");
            return;
        }
        
        ClearLevel();
        currentLevelIndex = levelIndex;
        
        string[] lines = levelFiles[levelIndex].text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
        {
            Debug.LogError($"LevelManager: Level file {levelIndex} is empty!");
            return;
        }

        for(int y = 0; y < lines.Length; y++)
        {
            string line = lines[y].Trim();
            
            // Bỏ qua dòng comment hoặc dòng trống
            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;
                
            // Nếu có comment trong dòng, chỉ lấy phần trước comment
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
                line = line.Substring(0, commentIndex).Trim();
            
            for(int x = 0; x < line.Length; x++)
            {
                char c = line[x];
                Vector2 position = new Vector2(x, -y);
                
                switch(c)
                {
                    case 'P':
                        SpawnEntity(EntityType.Player, position);
                        break;
                    case 'O':
                        SpawnEntity(EntityType.Obstacle, position);
                        break;
                    case 'E':
                        SpawnEntity(EntityType.Enemy, position);
                        break;
                    case 'C':
                        SpawnEntity(EntityType.Chest, position);
                        break;
                    case 'V':
                        SpawnEntity(EntityType.VictoryItem, position);
                        break;
                    case '.':
                        // Empty space, không làm gì
                        break;
                    case '#':
                        SpawnEntity(EntityType.Wall, position);
                        break;
                    case ' ':
                        // Space, không làm gì 
                        break;
                    default:
                        Debug.LogWarning($"LevelManager: Unknown character '{c}' in level file at ({x}, {-y})");
                        break;
                }
            }
        }
        SetupCameraForLevel();
    }
    private void OnRestartLevel()
    {
        if (debugLevel) Debug.Log("LevelManager: Handling RestartLevel event");
        
        // Restart current level
        LoadLevel(currentLevelIndex);
    }
    private void OnLevelCompleted()
    {
        if (debugLevel) Debug.Log("LevelManager: Level completed! Loading next level...");
        
        // Tăng level index và load level tiếp theo
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < levelFiles.Length)
        {
            LoadLevel(nextLevelIndex);
        }
        else
        {
            Debug.Log("LevelManager: All levels completed!");
            // Có thể thêm logic khi hoàn thành tất cả level
        }
    }

    private void ClearLevel()
    {
        if (debugLevel) Debug.Log("LevelManager: Clearing current level...");
        
        EntityBase[] existingEntities = FindObjectsByType<EntityBase>(FindObjectsSortMode.None);
        foreach (var entity in existingEntities)
        {
            if (debugLevel) Debug.Log($"LevelManager: Destroying {entity.name} ({entity.GetType().Name})");
            Destroy(entity.gameObject);
        }
        
        if (debugLevel) Debug.Log($"LevelManager: Cleared {existingEntities.Length} entities");
    }

    private void SpawnEntity(EntityType type, Vector2 position)
    {
        GameObject prefab = type switch
        {
            EntityType.Player => playerPrefab,
            EntityType.Obstacle => obstaclePrefab,
            EntityType.Enemy => enemyPrefab,
            EntityType.VictoryItem => victoryItemPrefab,
            EntityType.Chest => chestPrefab,
            _ => null
        };

        if (prefab != null)
        {
            GameObject spawnedObject = Instantiate(prefab, position, Quaternion.identity);
            
            EntityBase entity = spawnedObject.GetComponent<EntityBase>();
            if (entity != null)
            {
                Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
                entity.Init(gridPos);
                
                //if (debugLevel) Debug.Log($"LevelManager: Spawned {type} at {gridPos}");
                if (type == EntityType.Player)
                {
                    InputManager inputManager = FindFirstObjectByType<InputManager>();
                    if (inputManager != null)
                    {
                        inputManager.SetPlayer(entity as Player);
                    }
                    else
                    {
                        Debug.LogError("LevelManager: InputManager not found!");
                    }
                }
            }
            else
            {
                Debug.LogError($"LevelManager: Prefab for {type} does not have EntityBase component!");
            }
        }
        else
        {
            Debug.LogError($"LevelManager: No prefab assigned for {type}");
        }
    }
    
    private void SetupCameraForLevel()
    {
        if (!autoSetupCamera) return;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            if (debugLevel) Debug.LogWarning("LevelManager: No camera found for auto setup!");
            return;
        }
        
        // Đợi một frame để đảm bảo tất cả entities đã được spawn
        StartCoroutine(SetupCameraDelayed(mainCamera));
    }
    
    private System.Collections.IEnumerator SetupCameraDelayed(Camera camera)
    {
        yield return new WaitForEndOfFrame();
        
        // Sử dụng utility để auto setup camera
        CameraSetupUtility.AutoSetupCameraForCurrentLevel(camera, cameraPadding);
        
        if (debugLevel) Debug.Log("LevelManager: Camera auto-setup completed!");
    }
}

public enum EntityType { Player, Obstacle, Enemy, Chest, VictoryItem, Wall }
