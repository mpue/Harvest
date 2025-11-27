using UnityEngine;

/// <summary>
/// Component that handles unit animations based on state
/// Works with Controllable to set animation states during movement
/// </summary>
[RequireComponent(typeof(Animator))]
public class Animatable : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private string moveParameterName = "IsMoving";
    [SerializeField] private string moveSpeedParameterName = "MoveSpeed";
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string moveStateName = "Move";

    [Header("Animation Settings")]
    [SerializeField] private bool useBoolParameter = true; // Use bool parameter for IsMoving
    [SerializeField] private bool useSpeedParameter = true; // Use float parameter for movement speed
    [SerializeField] private bool useStateTriggers = false; // Use triggers to switch states
    [SerializeField] private float movementSpeedMultiplier = 1f; // Multiplier for animation speed

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private Animator animator;
    private bool isMoving = false;
    private float currentSpeed = 0f;

    // Parameter hash IDs for better performance
    private int moveParameterHash;
    private int moveSpeedParameterHash;
    private int idleStateHash;
    private int moveStateHash;

    public bool IsMoving => isMoving;
    public float CurrentSpeed => currentSpeed;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"? Animatable on {gameObject.name}: No Animator component found!");
            enabled = false;
            return;
        }

        // Cache parameter hashes for performance
        moveParameterHash = Animator.StringToHash(moveParameterName);
        moveSpeedParameterHash = Animator.StringToHash(moveSpeedParameterName);
        idleStateHash = Animator.StringToHash(idleStateName);
        moveStateHash = Animator.StringToHash(moveStateName);
    }

    void Start()
    {
        if (animator != null)
        {
            Debug.Log($"? Animatable on {gameObject.name} initialized (Animator: {animator.runtimeAnimatorController?.name ?? "None"})");
        }
    }

    /// <summary>
    /// Set the movement state
    /// </summary>
    public void SetMoving(bool moving)
    {
        if (animator == null) return;

        isMoving = moving;

        // Set bool parameter
        if (useBoolParameter)
        {
            animator.SetBool(moveParameterHash, moving);

            if (debugLogging)
            {
                Debug.Log($"?? {gameObject.name}: Animation state changed - IsMoving: {moving}");
            }
        }

        // Set state triggers
        if (useStateTriggers)
        {
            if (moving)
            {
                animator.SetTrigger(moveStateHash);
            }
            else
            {
                animator.SetTrigger(idleStateHash);
            }
        }

        // Reset speed when stopping
        if (!moving && useSpeedParameter)
        {
            SetMovementSpeed(0f);
        }
    }

    /// <summary>
    /// Set the movement speed for animation blending
    /// </summary>
    public void SetMovementSpeed(float speed)
    {
        if (animator == null) return;

        currentSpeed = speed * movementSpeedMultiplier;

        if (useSpeedParameter)
        {
            animator.SetFloat(moveSpeedParameterHash, currentSpeed);

            if (debugLogging)
            {
                Debug.Log($"?? {gameObject.name}: Animation speed set to {currentSpeed:F2}");
            }
        }
    }

    /// <summary>
    /// Set a custom animation parameter (bool)
    /// </summary>
    public void SetBool(string parameterName, bool value)
    {
        if (animator == null) return;

        int hash = Animator.StringToHash(parameterName);
        animator.SetBool(hash, value);
    }

    /// <summary>
    /// Set a custom animation parameter (float)
    /// </summary>
    public void SetFloat(string parameterName, float value)
    {
        if (animator == null) return;

        int hash = Animator.StringToHash(parameterName);
        animator.SetFloat(hash, value);
    }

    /// <summary>
    /// Set a custom animation parameter (int)
    /// </summary>
    public void SetInteger(string parameterName, int value)
    {
        if (animator == null) return;

        int hash = Animator.StringToHash(parameterName);
        animator.SetInteger(hash, value);
    }

    /// <summary>
    /// Trigger a custom animation
    /// </summary>
    public void SetTrigger(string triggerName)
    {
        if (animator == null) return;

        int hash = Animator.StringToHash(triggerName);
        animator.SetTrigger(hash);
    }

    /// <summary>
    /// Play a specific animation state
    /// </summary>
    public void PlayState(string stateName, int layer = 0)
    {
        if (animator == null) return;

        animator.Play(stateName, layer);
    }

    /// <summary>
    /// Get the animator component
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }
}
