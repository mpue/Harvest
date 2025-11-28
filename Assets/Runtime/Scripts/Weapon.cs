using UnityEngine;

/// <summary>
/// Base class for all weapons
/// Handles targeting, aiming, and shooting
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private string weaponName = "Weapon";
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 1f; // Shots per second
    [SerializeField] private float range = 20f;
    [SerializeField] private float projectileSpeed = 30f;

    [Header("Turret/Aiming")]
    [SerializeField] private Transform turretTransform; // Rotates horizontally towards target
    [SerializeField] private Transform barrelTransform; // Optional: Rotates vertically
    [SerializeField] private float turretRotationSpeed = 90f; // Degrees per second
    [SerializeField] private float barrelRotationSpeed = 45f;

    [Header("Unit Rotation (No Turret)")]
    [SerializeField] private bool rotateUnitToTarget = true; // Rotate entire unit if no turret
    [SerializeField] private float unitRotationSpeed = 180f; // Degrees per second for unit rotation
    [SerializeField] private float aimAngleTolerance = 15f; // Degrees tolerance for aiming without turret

    [Header("Shot Points")]
    [SerializeField] private Transform[] shotPoints; // Where projectiles spawn
    [SerializeField] private int currentShotPointIndex = 0; // For alternating fire

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Visual/Audio")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float muzzleFlashDuration = 0.1f;

    [Header("Animation")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private string aimParameterName = "IsAiming";
    [SerializeField] private string fireTriggerName = "Fire";
    [SerializeField] private bool useBoolForAim = true; // Use bool parameter for IsAiming
    [SerializeField] private bool useTriggerForFire = true; // Use trigger for Fire

    [Header("Movement-Based Animation")]
    [SerializeField] private bool useMovementBasedAnimation = true;
    [SerializeField] private string aimMovingParameterName = "IsAimingMoving";
    [SerializeField] private string aimStationaryParameterName = "IsAimingStationary";
    [SerializeField] private string fireMovingTriggerName = "FireMoving";
    [SerializeField] private string fireStationaryTriggerName = "FireStationary";
    [SerializeField] private float movementThreshold = 0.1f; // Speed threshold to consider unit as moving

    // Internal state
    private Transform currentTarget;
    private float lastFireTime = 0f;
    private bool isAimed = false;
    private TeamComponent ownerTeam;
    private AudioSource audioSource;
    private Animatable animatable;
    private Controllable controllable;
    private bool wasAiming = false; // Track previous aiming state
    private bool wasMoving = false; // Track previous movement state

    public float Range => range;
    public bool IsAimed => isAimed;
    public Transform CurrentTarget => currentTarget;
    public string WeaponName => weaponName;
    public float Damage => damage;
    public float FireRate => fireRate;

    void Awake()
    {
        ownerTeam = GetComponentInParent<TeamComponent>();
        animatable = GetComponentInParent<Animatable>();
        controllable = GetComponentInParent<Controllable>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && fireSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // Validate setup
        if (shotPoints == null || shotPoints.Length == 0)
        {
            Debug.LogWarning($"Weapon '{weaponName}' on {gameObject.name} has NO shot points assigned!");
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' on {gameObject.name} has NO projectile prefab assigned!");
        }

        // Log successful initialization
        if (shotPoints != null && shotPoints.Length > 0 && projectilePrefab != null)
        {
            Debug.Log($"Weapon '{weaponName}' on {gameObject.name} initialized successfully (Range: {range}, Damage: {damage}, FireRate: {fireRate})");
        }

        // Log animation component status
        if (useAnimation)
        {
            if (animatable != null)
            {
                Debug.Log($"Weapon '{weaponName}': Animation support enabled (Aim: {aimParameterName}, Fire: {fireTriggerName})");
            }
            else
            {
                Debug.Log($"Weapon '{weaponName}': Animation enabled but no Animatable component found on parent");
            }
        }

        // Log movement-based animation status
        if (useMovementBasedAnimation)
        {
            if (controllable != null && animatable != null)
            {
                Debug.Log($"Weapon '{weaponName}': Movement-based animation enabled (Moving: {aimMovingParameterName}/{fireMovingTriggerName}, Stationary: {aimStationaryParameterName}/{fireStationaryTriggerName})");
            }
            else
            {
                Debug.LogWarning($"Weapon '{weaponName}': Movement-based animation enabled but missing Controllable ({controllable != null}) or Animatable ({animatable != null})");
            }
        }

        // Log unit rotation status
        if (turretTransform == null && rotateUnitToTarget)
        {
            Debug.Log($"Weapon '{weaponName}': Unit rotation enabled (no turret) - Speed: {unitRotationSpeed}°/s, Tolerance: {aimAngleTolerance}°");
        }
    }

    void Update()
    {
        if (currentTarget != null)
        {
            AimAtTarget();
        }

        // Update animation state based on aiming
        if (useAnimation && animatable != null)
        {
            bool isAiming = currentTarget != null;

            // Only update if state changed
            if (isAiming != wasAiming)
            {
                if (useBoolForAim)
                {
                    animatable.SetBool(aimParameterName, isAiming);
                }

                wasAiming = isAiming;
            }
        }

        // Update movement-based animations
        if (useMovementBasedAnimation && animatable != null && controllable != null)
        {
            UpdateMovementBasedAnimations();
        }
    }

    /// <summary>
    /// Set the target for this weapon
    /// </summary>
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        isAimed = false;

        // Set aiming animation when target is acquired
        if (useAnimation && animatable != null && target != null)
        {
            if (useBoolForAim)
            {
                animatable.SetBool(aimParameterName, true);
            }
        }
    }

    /// <summary>
    /// Clear current target
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
        isAimed = false;

        // Clear aiming animation when target is lost
        if (useAnimation && animatable != null)
        {
            if (useBoolForAim)
            {
                animatable.SetBool(aimParameterName, false);
            }
        }
    }

    /// <summary>
    /// Aims turret and barrel at the current target
    /// </summary>
    private void AimAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 targetPosition = currentTarget.position;

        // Aim turret (horizontal rotation)
        if (turretTransform != null)
        {
            // Arbeite im World Space für präzisere Berechnungen
            Vector3 directionToTarget = targetPosition - turretTransform.position;

            // Projiziere auf die horizontale Ebene
            directionToTarget.y = 0;

            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                // Berechne die Ziel-Rotation
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                // Konvertiere zu lokaler Rotation wenn Parent existiert
                if (turretTransform.parent != null)
                {
                    targetRotation = Quaternion.Inverse(turretTransform.parent.rotation) * targetRotation;
                }

                // Extrahiere nur die Y-Achsen-Rotation
                Vector3 targetEuler = targetRotation.eulerAngles;
                Vector3 currentEuler = turretTransform.localEulerAngles;

                // Interpoliere nur die Y-Rotation
                float newYAngle = Mathf.MoveTowardsAngle(
              currentEuler.y,
                    targetEuler.y,
                turretRotationSpeed * Time.deltaTime
                );

                // Setze neue Rotation - WICHTIG: Bewahre X und Z exakt
                turretTransform.localRotation = Quaternion.Euler(
                 currentEuler.x,
              newYAngle,
           currentEuler.z
                       );

                // Prüfe ob ausgerichtet
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentEuler.y, targetEuler.y));
                isAimed = angleDiff < 5f;
            }
        }
        else if (rotateUnitToTarget)
        {
            // Kein Turret → Drehe gesamte Unit zum Ziel
            RotateUnitToTarget(targetPosition);
        }
        else
        {
            isAimed = true;
        }

        // Barrel aiming bleibt gleich
        if (barrelTransform != null && turretTransform != null)
        {
            Vector3 localTarget = turretTransform.InverseTransformPoint(targetPosition);
            float targetAngle = Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;

            Vector3 currentAngles = barrelTransform.localEulerAngles;
            float newAngle = Mathf.MoveTowards(currentAngles.x, -targetAngle, barrelRotationSpeed * Time.deltaTime);
            barrelTransform.localEulerAngles = new Vector3(newAngle, currentAngles.y, currentAngles.z);
        }
    }

    /// <summary>
    /// Rotate entire unit to face target (for units without turret)
    /// </summary>
    private void RotateUnitToTarget(Vector3 targetPosition)
    {
        // Calculate direction to target on horizontal plane
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            isAimed = true;
            return;
        }

        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Smoothly rotate unit towards target
        transform.rotation = Quaternion.RotateTowards(
      transform.rotation,
       targetRotation,
          unitRotationSpeed * Time.deltaTime
             );

        // Check if aimed (within tolerance)
        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        isAimed = angle <= aimAngleTolerance;
    }

    /// <summary>
    /// Try to fire at the current target
    /// Returns true if weapon fired
    /// </summary>
    public bool TryFire()
    {
        // Check if can fire
        if (!CanFire())
        {
            return false;
        }

        // Check if target is valid
        if (currentTarget == null)
        {
            return false;
        }

        // Check if target is in range
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > range)
        {
            // Debug every few seconds
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"?? {weaponName}: Target {currentTarget.name} out of range ({distanceToTarget:F1}m > {range}m)");
            }
            return false;
        }

        // Check if aimed (relaxed - allow firing even if not perfectly aimed)
        if (!isAimed && turretTransform != null)
        {
            // Be more lenient - fire if reasonably close to target
            Vector3 directionToTarget = (currentTarget.position - turretTransform.position).normalized;
            Vector3 turretForward = turretTransform.forward;
            float angle = Vector3.Angle(directionToTarget, turretForward);

            // Allow firing if within 15 degrees (was 5 degrees)
            if (angle > 15f)
            {
                return false;
            }
        }

        // Check team (don't shoot allies)
        TeamComponent targetTeam = currentTarget.GetComponent<TeamComponent>();
        if (targetTeam != null && ownerTeam != null)
        {
            if (!ownerTeam.IsEnemy(targetTeam))
            {
                return false; // Don't shoot allies or same team
            }
        }

        // Fire!
        Fire();
        return true;
    }

    /// <summary>
    /// Check if weapon can fire (fire rate)
    /// </summary>
    private bool CanFire()
    {
        float timeSinceLastFire = Time.time - lastFireTime;
        float fireInterval = 1f / fireRate;
        return timeSinceLastFire >= fireInterval;
    }

    /// <summary>
    /// Fire the weapon
    /// </summary>
    private void Fire()
    {
        lastFireTime = Time.time;

        // Trigger fire animation based on movement state
        if (useMovementBasedAnimation && animatable != null && controllable != null)
        {
            bool isMoving = IsUnitMoving();

            if (isMoving)
            {
                animatable.SetTrigger(fireMovingTriggerName);
            }
            else
            {
                animatable.SetTrigger(fireStationaryTriggerName);
            }
        }
        else if (useAnimation && animatable != null)
        {
            // Fallback to standard fire animation
            if (useTriggerForFire)
            {
                animatable.SetTrigger(fireTriggerName);
            }
        }

        // Get shot point
        Transform shotPoint = GetNextShotPoint();
        if (shotPoint == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' tried to fire but has no valid shot point!");
            return;
        }

        // Spawn projectile
        if (projectilePrefab != null)
        {
            GameObject projectileObj = Instantiate(projectilePrefab, shotPoint.position, shotPoint.rotation);
            Projectile projectile = projectileObj.GetComponent<Projectile>();

            if (projectile != null)
            {
                // Calculate direction to target
                Vector3 direction = (currentTarget.position - shotPoint.position).normalized;

                projectile.Initialize(direction, projectileSpeed, damage, range, ownerTeam);

                Debug.Log($" {weaponName} FIRED at {currentTarget.name}! (Distance: {Vector3.Distance(transform.position, currentTarget.position):F1}m)");
            }
            else
            {
                Debug.LogError($" Projectile prefab '{projectilePrefab.name}' has no Projectile component!");
            }
        }
        else
        {
            Debug.LogError($" {weaponName}: Cannot fire - no projectile prefab assigned!");
        }

        // Visual effects
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Audio
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // Callback
        OnWeaponFired();
    }

    /// <summary>
    /// Get next shot point for alternating fire
    /// </summary>
    private Transform GetNextShotPoint()
    {
        if (shotPoints == null || shotPoints.Length == 0)
        {
            return transform; // Fallback to weapon transform
        }

        Transform shotPoint = shotPoints[currentShotPointIndex];

        // Alternate shot points for multi-barrel weapons
        currentShotPointIndex = (currentShotPointIndex + 1) % shotPoints.Length;

        return shotPoint;
    }

    /// <summary>
    /// Check if target is in range
    /// </summary>
    public bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= range;
    }

    /// <summary>
    /// Called when weapon fires - override for custom behavior
    /// </summary>
    protected virtual void OnWeaponFired()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // Draw shot points
        if (shotPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform shotPoint in shotPoints)
            {
                if (shotPoint != null)
                {
                    Gizmos.DrawWireSphere(shotPoint.position, 0.2f);
                    Gizmos.DrawRay(shotPoint.position, shotPoint.forward * 2f);
                }
            }
        }

        // Draw line to target
        if (currentTarget != null)
        {
            Gizmos.color = isAimed ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    /// <summary>
    /// Update movement-based animations
    /// </summary>
    private void UpdateMovementBasedAnimations()
    {
        if (controllable == null || animatable == null) return;

        // Check if unit is moving
        bool isMoving = IsUnitMoving();

        // Only update if state changed
        if (isMoving != wasMoving || currentTarget != null)
        {
            // Update aim animations based on movement and target
            if (currentTarget != null)
            {
                if (isMoving)
                {
                    // Unit is moving and has target → Aiming while moving
                    animatable.SetBool(aimMovingParameterName, true);
                    animatable.SetBool(aimStationaryParameterName, false);
                }
                else
                {
                    // Unit is stationary and has target → Aiming while stationary
                    animatable.SetBool(aimMovingParameterName, false);
                    animatable.SetBool(aimStationaryParameterName, true);
                }
            }
            else
            {
                // No target → Clear all aim animations
                animatable.SetBool(aimMovingParameterName, false);
                animatable.SetBool(aimStationaryParameterName, false);
            }

            wasMoving = isMoving;
        }
    }

    /// <summary>
    /// Check if unit is currently moving
    /// </summary>
    private bool IsUnitMoving()
    {
        if (controllable == null) return false;

        return controllable.IsMoving;
    }

    /// <summary>
    /// Enable/disable animation support
    /// </summary>
    public void SetUseAnimation(bool use)
    {
        useAnimation = use;
    }

    /// <summary>
    /// Enable/disable movement-based animation
    /// </summary>
    public void SetUseMovementBasedAnimation(bool use)
    {
        useMovementBasedAnimation = use;
    }

    /// <summary>
    /// Enable/disable unit rotation to target (for units without turret)
    /// </summary>
    public void SetRotateUnitToTarget(bool rotate)
    {
        rotateUnitToTarget = rotate;
    }

    /// <summary>
    /// Get Animatable component
    /// </summary>
    public Animatable GetAnimatable()
    {
        return animatable;
    }

    /// <summary>
    /// Manually trigger fire animation (useful for testing or custom behavior)
    /// </summary>
    public void TriggerFireAnimation()
    {
        if (useAnimation && animatable != null && useTriggerForFire)
        {
            animatable.SetTrigger(fireTriggerName);
        }
    }

    /// <summary>
    /// Manually set aim animation (useful for custom behavior)
    /// </summary>
    public void SetAimAnimation(bool aiming)
    {
        if (useAnimation && animatable != null && useBoolForAim)
        {
            animatable.SetBool(aimParameterName, aiming);
        }
    }
}
