using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float padding = 1f; // Khoảng cách từ biên level đến edge camera
    [SerializeField] private float transitionSpeed = 2f; // Tốc độ chuyển đổi camera
    
    [Header("Debug")]
    [SerializeField] private bool debugCamera = true;
    
    private LevelManager levelManager;
    
    private void Awake()
    {
        // Tự động tìm main camera nếu chưa được gán
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe(GameEvent.RestartLevel, OnLevelChanged);
        EventBus.Subscribe(GameEvent.LevelCompleted, OnLevelChanged);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubcribe(GameEvent.RestartLevel, OnLevelChanged);
        EventBus.Unsubcribe(GameEvent.LevelCompleted, OnLevelChanged);
    }
    
    private void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        
        // Đợi một frame để level được load xong
        Invoke(nameof(CenterCameraOnLevel), 0.1f);
    }
    
    private void OnLevelChanged()
    {
        // Đợi một chút để level mới được spawn xong
        Invoke(nameof(CenterCameraOnLevel), 0.2f);
    }
    
    /// <summary>
    /// Tính toán và center camera dựa trên kích thước level hiện tại
    /// </summary>
    public void CenterCameraOnLevel()
    {
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Main camera not found!");
            return;
        }
        
        // Tìm tất cả entities trong level
        EntityBase[] entities = FindObjectsByType<EntityBase>(FindObjectsSortMode.None);
        
        if (entities.Length == 0)
        {
            Debug.LogWarning("CameraController: No entities found in level!");
            return;
        }
        
        // Tính bounds của level
        Bounds levelBounds = CalculateLevelBounds(entities);
        
        // Tính position và size tối ưu cho camera
        Vector3 centerPosition = new Vector3(levelBounds.center.x, levelBounds.center.y, mainCamera.transform.position.z);
        float optimalSize = CalculateOptimalCameraSize(levelBounds);
        
        // Apply camera settings
        if (transitionSpeed > 0)
        {
            // Smooth transition
            StartCoroutine(SmoothCameraTransition(centerPosition, optimalSize));
        }
        else
        {
            // Instant change
            mainCamera.transform.position = centerPosition;
            mainCamera.orthographicSize = optimalSize;
        }
        
        if (debugCamera)
        {
            Debug.Log($"CameraController: Level bounds: {levelBounds}");
            Debug.Log($"CameraController: Camera centered at: {centerPosition}");
            Debug.Log($"CameraController: Camera size: {optimalSize}");
        }
    }
    
    /// <summary>
    /// Tính bounds của level dựa trên vị trí các entities
    /// </summary>
    private Bounds CalculateLevelBounds(EntityBase[] entities)
    {
        Vector3 min = entities[0].transform.position;
        Vector3 max = entities[0].transform.position;
        
        foreach (var entity in entities)
        {
            Vector3 pos = entity.transform.position;
            
            if (pos.x < min.x) min.x = pos.x;
            if (pos.y < min.y) min.y = pos.y;
            if (pos.x > max.x) max.x = pos.x;
            if (pos.y > max.y) max.y = pos.y;
        }
        
        // Thêm padding
        min -= Vector3.one * padding;
        max += Vector3.one * padding;
        
        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;
        
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// Tính kích thước camera tối ưu để hiển thị toàn bộ level
    /// </summary>
    private float CalculateOptimalCameraSize(Bounds levelBounds)
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float levelAspect = levelBounds.size.x / levelBounds.size.y;
        
        float cameraSize;
        
        if (levelAspect > screenAspect)
        {
            // Level rộng hơn → fit theo width
            cameraSize = levelBounds.size.x / (2f * screenAspect);
        }
        else
        {
            // Level cao hơn → fit theo height  
            cameraSize = levelBounds.size.y / 2f;
        }
        
        // Clamp size để tránh quá nhỏ hoặc quá lớn
        cameraSize = Mathf.Clamp(cameraSize, 2f, 20f);
        
        return cameraSize;
    }
    
    /// <summary>
    /// Smooth camera transition
    /// </summary>
    private System.Collections.IEnumerator SmoothCameraTransition(Vector3 targetPosition, float targetSize)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Smooth interpolation
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure exact final values
        mainCamera.transform.position = targetPosition;
        mainCamera.orthographicSize = targetSize;
    }
    
    /// <summary>
    /// Public method để force recenter camera
    /// </summary>
    public void RecenterCamera()
    {
        CenterCameraOnLevel();
    }
    
    /// <summary>
    /// Set camera padding
    /// </summary>
    public void SetPadding(float newPadding)
    {
        padding = newPadding;
        CenterCameraOnLevel();
    }
}