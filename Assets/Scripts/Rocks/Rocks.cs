using UnityEngine;

public class Rocks : ObstacleBase
{
    [Header("Rock Specific")]
    [SerializeField] private bool makeSoundOnPush = true;
    [SerializeField] private ParticleSystem dustEffect;
    
    protected override void OnPushCompleted()
    {
        base.OnPushCompleted();
        
        // Rock specific behavior sau khi được đẩy
        if (makeSoundOnPush)
        {
            // TODO: Play push sound
            Debug.Log($"Rock {name}: *Push sound*");
        }
        
        if (dustEffect != null)
        {
            dustEffect.Play();
        }
        
        // Có thể check xem rock có rơi vào hole không, v.v.
        CheckSpecialPositions();
    }
    
    private void CheckSpecialPositions()
    {
        // TODO: Implement logic check special positions
        // Ví dụ: nếu rock ở vị trí target, trigger win condition
        Debug.Log($"Rock {name}: Checking special positions at {gridPosition}");
    }
    
    public override bool CanBePushed()
    {
        // Rock có thể có logic riêng, ví dụ: không thể đẩy khi đang trên ice
        bool baseCanPush = base.CanBePushed();
        
        // TODO: Add rock-specific conditions
        // if (IsOnIce()) return false;
        
        return baseCanPush;
    }
}
