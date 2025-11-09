using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BaseUnit))]
public class Controllable : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Navigation")]
    [SerializeField] private bool useNavMesh = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject moveTargetIndicator;
    [SerializeField] private float indicatorLifetime = 1f;
    [SerializeField] private Color pathColor = Color.cyan;
    [SerializeField] private bool showPath = true;

    private NavMeshAgent navAgent;
    private BaseUnit baseUnit;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private bool isMoving = false;

    // Formation offset for group movement
    private Vector3 formationOffset = Vector3.zero;

    public bool IsMoving => isMoving;
    public Vector3 TargetPosition => targetPosition;
    public bool HasTarget => hasTarget;

    void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();

        if (useNavMesh)
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<NavMeshAgent>();
            }

            // Configure NavMeshAgent
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed * 50f;
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = stoppingDistance;
        }
    }

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (hasTarget)
        {
            if (useNavMesh && navAgent != null)
            {
                UpdateNavMeshMovement();
            }
            else
            {
                UpdateManualMovement();
            }
        }
    }

    /// <summary>
    /// Moves the unit to a specific position
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        MoveTo(destination, Vector3.zero);
    }

    /// <summary>
    /// Moves the unit to a specific position with formation offset
    /// </summary>
    public void MoveTo(Vector3 destination, Vector3 offset)
    {
        targetPosition = destination + offset;
        formationOffset = offset;
        hasTarget = true;
        isMoving = true;

        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(targetPosition);
        }

        // Show visual feedback
        ShowMoveIndicator(targetPosition);

        OnMoveCommand(targetPosition);
    }

    /// <summary>
    /// Stops the unit's movement
    /// </summary>
    public void Stop()
    {
        hasTarget = false;
        isMoving = false;

        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        OnStop();
    }

    /// <summary>
    /// Updates movement using NavMeshAgent
    /// </summary>
    private void UpdateNavMeshMovement()
    {
        if (navAgent == null || !navAgent.isOnNavMesh) return;

        // Check if reached destination
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
            {
                hasTarget = false;
                isMoving = false;
                OnReachedDestination();
            }
        }
    }

    /// <summary>
    /// Updates movement without NavMesh (manual movement)
    /// </summary>
    private void UpdateManualMovement()
    {
        Vector3 direction = (targetPosition - transform.position);
        float distance = direction.magnitude;

        // Check if reached destination
        if (distance <= stoppingDistance)
        {
            hasTarget = false;
            isMoving = false;
            OnReachedDestination();
            return;
        }

        direction.Normalize();

        // Move towards target
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate towards movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Shows a visual indicator at the move target position
    /// </summary>
    private void ShowMoveIndicator(Vector3 position)
    {
        if (moveTargetIndicator != null)
        {
            GameObject indicator = Instantiate(moveTargetIndicator, position, Quaternion.identity);
            Destroy(indicator, indicatorLifetime);
        }
    }

    /// <summary>
    /// Sets the formation offset for this unit
    /// </summary>
    public void SetFormationOffset(Vector3 offset)
    {
        formationOffset = offset;
    }

    /// <summary>
    /// Gets the current formation offset
    /// </summary>
    public Vector3 GetFormationOffset()
    {
        return formationOffset;
    }

    /// <summary>
    /// Checks if the unit can move to a position
    /// </summary>
    public bool CanMoveTo(Vector3 position)
    {
        if (useNavMesh && navAgent != null)
        {
            NavMeshPath path = new NavMeshPath();
            return navAgent.CalculatePath(position, path) && path.status == NavMeshPathStatus.PathComplete;
        }
        return true;
    }

    /// <summary>
    /// Called when a move command is issued - override in derived classes
    /// </summary>
    protected virtual void OnMoveCommand(Vector3 destination)
    {
        Debug.Log($"{baseUnit.UnitName} moving to {destination}");
    }

    /// <summary>
    /// Called when the unit reaches its destination - override in derived classes
    /// </summary>
    protected virtual void OnReachedDestination()
    {
        Debug.Log($"{baseUnit.UnitName} reached destination");
    }

    /// <summary>
    /// Called when the unit stops - override in derived classes
    /// </summary>
    protected virtual void OnStop()
    {
        Debug.Log($"{baseUnit.UnitName} stopped");
    }

    /// <summary>
    /// Draws debug information
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showPath || !hasTarget) return;

        // Draw line to target
        Gizmos.color = pathColor;
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, stoppingDistance);

        // Draw NavMesh path if available
        if (useNavMesh && navAgent != null && navAgent.hasPath)
        {
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw movement range/info when selected
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    /// <summary>
    /// Enable/Disable NavMeshAgent at runtime
    /// </summary>
    public void SetUseNavMesh(bool use)
    {
        useNavMesh = use;
        if (navAgent != null)
        {
            navAgent.enabled = use;
        }
    }
}
