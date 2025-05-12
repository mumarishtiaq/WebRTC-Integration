using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
   [SerializeField] private Animator _animator;
   public SkinnedMeshRenderer HeadMesh;



    private void Start()
    {
        if(!_animator)
            _animator = GetComponent<Animator>();
    }
    public void TriggerAnimation(AnimationType animType)
    {
        switch (animType)
        {
            case AnimationType.Sit:
                TriggerSittingAnimation();
                break;
            case AnimationType.Idle:
                TriggerIdleAnimation();
                break;
            default:
                break;
        }
    }

    [ContextMenu("Sit")]
    private void TriggerSittingAnimation()
    {
        _animator.SetTrigger("Sit");
    } 
    
    [ContextMenu("Idle")]
    private void TriggerIdleAnimation()
    {
        _animator.SetTrigger("Idle");
    }


}
