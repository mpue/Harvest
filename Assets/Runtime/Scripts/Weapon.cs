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

    [Header("Shot Points")]
    [SerializeField] private Transform[] shotPoints; // Where projectiles spawn
    [SerializeField] private int currentShotPointIndex = 0; // For alternating fire

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Visual/Audio")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float muzzleFlashDuration = 0.1f;

    // Internal state
    private Transform currentTarget;
    private float lastFireTime = 0f;
    private bool isAimed = false;
    private TeamComponent ownerTeam;
    private AudioSource audioSource;

    public float Range => range;
    public bool IsAimed => isAimed;
    public Transform CurrentTarget => currentTarget;
    public string WeaponName => weaponName;

    void Awake()
    {
        ownerTeam = GetComponentInParent<TeamComponent>();
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
            Debug.LogWarning($"Weapon '{weaponName}' has no shot points assigned!");
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' has no projectile prefab assigned!");
        }
    }

    void Update()
    {
        if (currentTarget != null)
        {
            AimAtTarget();
        }
    }

    /// <summary>
    /// Set the target for this weapon
    /// </summary>
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        isAimed = false;
    }

    /// <summary>
    /// Clear current target
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
        isAimed = false;
    }

    /// <summary>
    /// Aims turret and barrel at the current target
    /// </summary>
    private void AimAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 targetPosition = currentTarget.position;
        Vector3 directionToTarget = targetPosition - transform.position;

        // Aim turret (horizontal rotation)
        if (turretTransform != null)
        {
            Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            if (horizontalDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
                turretTransform.rotation = Quaternion.RotateTowards(turretTransform.rotation, targetRotation, turretRotationSpeed * Time.deltaTime );

                // Check if turret is aimed
                float angle = Quaternion.Angle(turretTransform.rotation, targetRotation);
                isAimed = angle < 5f; // Within 5 degrees = aimed
            }
        }
        else
        {
            // No turret, always aimed
            isAimed = true;
        }

        // Aim barrel (vertical rotation)
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
            return false;
        }

        // Check if aimed
        if (!isAimed)
        {
            return false;
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
            }
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
}
