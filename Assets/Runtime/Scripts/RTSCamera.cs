using UnityEngine;

/// <summary>
/// RTS Camera Controller with Smart Control Mode
/// 
/// CONTROL MODES:
/// ==============
/// 
/// 1. SMART MODE (Default - Recommended):
///    No units selected  ? Right Click = Camera Rotation
///    Units selected   ? Right Click = Move Command
///    
/// 2. MODIFIER KEY MODE:
///    Right Click        ? Move Command
///    Alt + Right Click  ? Camera Rotation
///  
/// 3. ALWAYS FREE-LOOK:
///    Right Click    ? Camera Rotation (always)
///    
/// Controls:
/// ---------
/// WASD      = Move camera
/// Q/E       = Move up/down
/// Shift     = Faster movement
/// Right MB  = Free-look rotation (context dependent)
/// Middle MB = Pan camera
/// 
/// See: CameraControl_Conflict_Solution.md for details
/// </summary>
public class RTSCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float fastMoveSpeed = 20f;
    [SerializeField] private float flySpeedMultiplier = 2f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private KeyCode freeLookKey = KeyCode.Mouse1; // Can be changed to Alt, etc.

    [Header("Height Settings")]
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;

    [Header("Control Mode")]
    [SerializeField] private CameraControlMode controlMode = CameraControlMode.Smart;

    [Header("Focus Settings")]
    [SerializeField] private KeyCode focusKey = KeyCode.F;
    [SerializeField] private float focusSpeed = 5f;
    [SerializeField] private float focusDistance = 15f; // Distance from target
    [SerializeField] private float focusHeightOffset = 5f; // Height above target

    private bool isFreeLook = false;
    private Vector3 lastMousePosition;
    private Camera cam;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private UnitSelector unitSelector;

    // Focus/Follow state
    private bool isFocusing = false;
    private Transform focusTarget = null;
    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    public enum CameraControlMode
    {
        Smart,      // Rechtsklick für Kamera nur wenn keine Units selektiert
        AlwaysFreeLook,     // Rechtsklick immer für Kamera
        ModifierKey    // Alt/Strg + Rechtsklick für Kamera
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        // Find UnitSelector for Smart mode
        unitSelector = FindFirstObjectByType<UnitSelector>();

        // Initialize rotation from current transform
        Vector3 currentRotation = transform.eulerAngles;
        rotationY = currentRotation.y;
        rotationX = currentRotation.x;
    }

    void Update()
    {
        HandleFocusInput();
        HandleFreeLook();
        HandleWASDMovement();
        HandlePan();
        ClampCameraHeight();
        UpdateFocusMovement();
    }

    /// <summary>
    /// Handle F key input to focus on selected unit/building
    /// </summary>
    private void HandleFocusInput()
    {
        if (Input.GetKeyDown(focusKey))
        {
            FocusOnSelection();
        }

        // Cancel focus on any manual movement input
        if (isFocusing)
        {
            bool hasManualInput = Input.GetAxis("Horizontal") !=0 ||
                Input.GetAxis("Vertical") !=0 ||
                Input.GetKey(KeyCode.Q) ||
                Input.GetKey(KeyCode.E) ||
                Input.GetMouseButton(1) ||
                Input.GetMouseButton(2);

            if (hasManualInput)
            {
                CancelFocus();
            }
        }
    }

    /// <summary>
    /// Focus camera on currently selected unit(s) or building
    /// </summary>
    private void FocusOnSelection()
    {
        if (unitSelector == null || unitSelector.SelectedCount ==0)
        {
            Debug.Log("RTSCamera: No selection to focus on");
            return;
        }

        // Get first selected unit/building
        var selected = unitSelector.SelectedUnits;
        if (selected == null || selected.Count ==0)
            return;

        Transform target = selected[0].transform;
        
        // Calculate target position (behind and above the target)
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        directionToTarget.y =0;
        directionToTarget.Normalize();

        // Position camera behind target at specified distance and height
        Vector3 desiredPosition = target.position - directionToTarget * focusDistance;
        desiredPosition.y = target.position.y + focusHeightOffset;

        // Calculate rotation to look at target
        Vector3 lookDirection = target.position - desiredPosition;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        // Start smooth transition
        targetPosition = desiredPosition;
        targetRotation = desiredRotation;
        focusTarget = target;
        isFocusing = true;

        Debug.Log($"RTSCamera: Focusing on {target.name}");
    }

    /// <summary>
    /// Update smooth camera movement when focusing
    /// </summary>
    private void UpdateFocusMovement()
    {
        if (!isFocusing)
            return;

        // Smooth move to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * focusSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * focusSpeed);

        // Update rotation values for free-look system
        Vector3 euler = transform.eulerAngles;
        rotationX = euler.x;
        rotationY = euler.y;

        // Check if close enough to target
        if (Vector3.Distance(transform.position, targetPosition) <0.1f &&
            Quaternion.Angle(transform.rotation, targetRotation) <1f)
        {
            // Arrived at target
            isFocusing = false;
            Debug.Log("RTSCamera: Focus complete");
        }
    }

    /// <summary>
    /// Cancel focus mode
    /// </summary>
    private void CancelFocus()
    {
        isFocusing = false;
        focusTarget = null;
    }

    private void HandleFreeLook()
    {
        // Check if free-look should be enabled based on control mode
        bool shouldEnableFreeLook = false;

        switch (controlMode)
        {
            case CameraControlMode.Smart:
                // Only enable free-look if no units are selected
                shouldEnableFreeLook = Input.GetMouseButton(1) &&
                          (unitSelector == null || unitSelector.SelectedCount == 0);
                break;

            case CameraControlMode.AlwaysFreeLook:
                // Always use right click for camera
                shouldEnableFreeLook = Input.GetMouseButton(1);
                break;

            case CameraControlMode.ModifierKey:
                // Require modifier key (Alt) + Right Click
                shouldEnableFreeLook = Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt);
                break;
        }

        // Enable/disable free-look
        if (shouldEnableFreeLook && !isFreeLook)
        {
            isFreeLook = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!shouldEnableFreeLook && isFreeLook)
        {
            isFreeLook = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Apply rotation when in free-look mode
        if (isFreeLook)
        {
            // Get mouse movement
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Update rotation
            rotationY += mouseX;
            rotationX -= mouseY;

            // Clamp vertical rotation to prevent flipping
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            // Apply rotation
            transform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
        }
    }

    private void HandleWASDMovement()
    {
        // WASD movement works all the time (like Unity Editor)
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Only move if there's input
        if (horizontal != 0 || vertical != 0 || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            // Determine current speed (shift for fast move)
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

            // Apply fly speed multiplier when in free-look mode
            if (isFreeLook)
            {
                currentSpeed *= flySpeedMultiplier;
            }

            // Move relative to camera's forward and right directions
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Keep movement on XZ plane for standard RTS feel (unless in free-look)
            if (!isFreeLook)
            {
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
            }

            Vector3 movement = (forward * vertical + right * horizontal) * currentSpeed * Time.deltaTime;

            // Add vertical movement with Q/E keys
            if (Input.GetKey(KeyCode.E))
            {
                movement.y += currentSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                movement.y -= currentSpeed * Time.deltaTime;
            }

            transform.position += movement;
        }
    }

    private void HandlePan()
    {
        // Middle mouse button pan
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Convert screen space movement to world space
            Vector3 move = new Vector3(-delta.x * panSpeed, 0, -delta.y * panSpeed);

            // Transform movement relative to camera orientation
            move = transform.TransformDirection(move);
            move.y = 0; // Keep pan on horizontal plane

            transform.position += move * Time.deltaTime;

            lastMousePosition = Input.mousePosition;
        }
    }

    private void ClampCameraHeight()
    {
        // Clamp camera height to stay within bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;
    }

    /// <summary>
    /// Returns true if camera is currently in free-look mode
    /// </summary>
    public bool IsInFreeLookMode()
    {
        return isFreeLook;
    }

    /// <summary>
    /// Set the control mode at runtime
    /// </summary>
    public void SetControlMode(CameraControlMode mode)
    {
        controlMode = mode;
    }

    /// <summary>
    /// Check if camera is currently focusing on a target
    /// </summary>
    public bool IsFocusing()
    {
        return isFocusing;
    }

    /// <summary>
    /// Focus on a specific transform (can be called from code)
    /// </summary>
    public void FocusOnTarget(Transform target)
    {
        if (target == null)
            return;

        // Calculate target position (behind and above the target)
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        directionToTarget.y =0;
        directionToTarget.Normalize();

        // Position camera behind target at specified distance and height
        Vector3 desiredPosition = target.position - directionToTarget * focusDistance;
        desiredPosition.y = target.position.y + focusHeightOffset;

        // Calculate rotation to look at target
        Vector3 lookDirection = target.position - desiredPosition;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        // Start smooth transition
        targetPosition = desiredPosition;
        targetRotation = desiredRotation;
        focusTarget = target;
        isFocusing = true;

        Debug.Log($"RTSCamera: Focusing on {target.name}");
    }
}
