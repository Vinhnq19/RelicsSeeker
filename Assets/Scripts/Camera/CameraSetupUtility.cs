using UnityEngine;

/// <summary>
/// Utility class để setup camera cho game grid-based
/// Cung cấp các method static để tự động điều chỉnh camera cho level
/// </summary>
public static class CameraSetupUtility
{
    /// <summary>
    /// Tự động setup camera để hiển thị toàn bộ level hiện tại
    /// </summary>
    /// <param name="camera">Camera cần setup</param>
    /// <param name="padding">Khoảng cách padding xung quanh level</param>
    public static void AutoSetupCameraForCurrentLevel(Camera camera, float padding = 1f)
    {
        if (camera == null)
        {
            Debug.LogError("CameraSetupUtility: Camera is null!");
            return;
        }
        
        // Tìm tất cả entities trong scene
        EntityBase[] entities = Object.FindObjectsByType<EntityBase>(FindObjectsSortMode.None);
        
        if (entities.Length == 0)
        {
            Debug.LogWarning("CameraSetupUtility: No entities found in scene!");
            return;
        }
        
        // Tính bounds của level từ các entities
        Bounds levelBounds = CalculateLevelBounds(entities, padding);
        
        // Set camera position tại center của level
        Vector3 cameraPosition = new Vector3(
            levelBounds.center.x, 
            levelBounds.center.y, 
            camera.transform.position.z // Giữ nguyên Z position
        );
        camera.transform.position = cameraPosition;
        
        // Tính và set orthographic size tối ưu
        float optimalSize = CalculateOptimalCameraSize(levelBounds);
        camera.orthographicSize = optimalSize;
        
        Debug.Log($"CameraSetupUtility: Camera setup completed - Position: {cameraPosition}, Size: {optimalSize}");
    }
    
    /// <summary>
    /// Setup camera cho level với kích thước cố định
    /// </summary>
    /// <param name="camera">Camera cần setup</param>
    /// <param name="levelSize">Kích thước level (width, height)</param>
    /// <param name="levelOffset">Offset của level (nếu level không bắt đầu từ 0,0)</param>
    /// <param name="padding">Padding xung quanh level</param>
    public static void SetupCameraForFixedLevel(Camera camera, Vector2Int levelSize, Vector2Int levelOffset = default, float padding = 1f)
    {
        if (camera == null)
        {
            Debug.LogError("CameraSetupUtility: Camera is null!");
            return;
        }
        
        // Tính center của level
        Vector3 levelCenter = new Vector3(
            levelOffset.x + (levelSize.x - 1) / 2f,
            levelOffset.y - (levelSize.y - 1) / 2f, // Negative vì Unity grid Y giảm xuống
            camera.transform.position.z
        );
        
        // Set camera position
        camera.transform.position = levelCenter;
        
        // Tính orthographic size
        float screenAspect = (float)Screen.width / Screen.height;
        float levelAspect = (float)levelSize.x / levelSize.y;
        
        float cameraSize;
        if (levelAspect > screenAspect)
        {
            // Level rộng hơn screen → fit theo width
            cameraSize = (levelSize.x + padding * 2) / (2f * screenAspect);
        }
        else
        {
            // Level cao hơn screen → fit theo height
            cameraSize = (levelSize.y + padding * 2) / 2f;
        }
        
        // Clamp size để tránh quá nhỏ hoặc quá lớn
        cameraSize = Mathf.Clamp(cameraSize, 1f, 50f);
        camera.orthographicSize = cameraSize;
        
        Debug.Log($"CameraSetupUtility: Fixed level setup - Size: {levelSize}, Center: {levelCenter}, Camera Size: {cameraSize}");
    }
    
    /// <summary>
    /// Tính bounds của level dựa trên vị trí các entities
    /// </summary>
    /// <param name="entities">Array các entities trong level</param>
    /// <param name="padding">Padding xung quanh bounds</param>
    /// <returns>Bounds của level</returns>
    private static Bounds CalculateLevelBounds(EntityBase[] entities, float padding)
    {
        if (entities.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one);
        
        // Khởi tạo min/max với entity đầu tiên
        Vector3 firstPos = entities[0].transform.position;
        Vector3 min = firstPos;
        Vector3 max = firstPos;
        
        // Tìm min/max từ tất cả entities
        foreach (var entity in entities)
        {
            Vector3 pos = entity.transform.position;
            
            if (pos.x < min.x) min.x = pos.x;
            if (pos.y < min.y) min.y = pos.y;
            if (pos.x > max.x) max.x = pos.x;
            if (pos.y > max.y) max.y = pos.y;
        }
        
        // Thêm padding
        min -= new Vector3(padding, padding, 0);
        max += new Vector3(padding, padding, 0);
        
        // Tính center và size
        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;
        
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// Tính orthographic size tối ưu để hiển thị toàn bộ bounds
    /// </summary>
    /// <param name="bounds">Bounds cần hiển thị</param>
    /// <returns>Orthographic size tối ưu</returns>
    private static float CalculateOptimalCameraSize(Bounds bounds)
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float boundsAspect = bounds.size.x / bounds.size.y;
        
        float cameraSize;
        
        if (boundsAspect > screenAspect)
        {
            // Bounds rộng hơn screen → fit theo width
            cameraSize = bounds.size.x / (2f * screenAspect);
        }
        else
        {
            // Bounds cao hơn screen → fit theo height
            cameraSize = bounds.size.y / 2f;
        }
        
        // Clamp size trong phạm vi hợp lý
        cameraSize = Mathf.Clamp(cameraSize, 1f, 50f);
        
        return cameraSize;
    }
    
    /// <summary>
    /// Setup camera với smooth transition
    /// </summary>
    /// <param name="camera">Camera cần setup</param>
    /// <param name="targetPosition">Vị trí target</param>
    /// <param name="targetSize">Size target</param>
    /// <param name="duration">Thời gian transition</param>
    public static void SetupCameraSmooth(Camera camera, Vector3 targetPosition, float targetSize, float duration = 1f)
    {
        if (camera == null) return;
        
        // Tìm hoặc tạo CameraController để handle smooth transition
        var cameraController = camera.GetComponent<CameraController>();
        if (cameraController == null)
        {
            // Nếu không có CameraController, set trực tiếp
            camera.transform.position = targetPosition;
            camera.orthographicSize = targetSize;
        }
        else
        {
            // Sử dụng CameraController để smooth transition
            cameraController.RecenterCamera();
        }
    }
    
    /// <summary>
    /// Get level size từ level file text
    /// </summary>
    /// <param name="levelText">Nội dung level file</param>
    /// <returns>Kích thước level (width, height)</returns>
    public static Vector2Int GetLevelSizeFromText(string levelText)
    {
        if (string.IsNullOrEmpty(levelText))
            return Vector2Int.zero;
        
        string[] lines = levelText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length == 0)
            return Vector2Int.zero;
        
        int maxWidth = 0;
        int height = 0;
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // Bỏ qua comment lines
            if (trimmedLine.StartsWith("//") || string.IsNullOrEmpty(trimmedLine))
                continue;
            
            // Xử lý inline comments
            int commentIndex = trimmedLine.IndexOf("//");
            if (commentIndex >= 0)
                trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
            
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                maxWidth = Mathf.Max(maxWidth, trimmedLine.Length);
                height++;
            }
        }
        
        return new Vector2Int(maxWidth, height);
    }
    
    /// <summary>
    /// Debug method để vẽ level bounds trong Scene view
    /// </summary>
    /// <param name="bounds">Bounds cần vẽ</param>
    /// <param name="color">Màu của bounds</param>
    public static void DrawLevelBounds(Bounds bounds, Color color = default)
    {
        if (color == default) color = Color.yellow;
        
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        
        // Vẽ 4 cạnh của bounds
        Vector3 topLeft = center + new Vector3(-size.x/2, size.y/2, 0);
        Vector3 topRight = center + new Vector3(size.x/2, size.y/2, 0);
        Vector3 bottomLeft = center + new Vector3(-size.x/2, -size.y/2, 0);
        Vector3 bottomRight = center + new Vector3(size.x/2, -size.y/2, 0);
        
        Debug.DrawLine(topLeft, topRight, color, 2f);
        Debug.DrawLine(topRight, bottomRight, color, 2f);
        Debug.DrawLine(bottomRight, bottomLeft, color, 2f);
        Debug.DrawLine(bottomLeft, topLeft, color, 2f);
        
        // Vẽ center point
        Debug.DrawLine(center + Vector3.left * 0.2f, center + Vector3.right * 0.2f, Color.red, 2f);
        Debug.DrawLine(center + Vector3.down * 0.2f, center + Vector3.up * 0.2f, Color.red, 2f);
    }
}