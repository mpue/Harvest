using UnityEngine;

/// <summary>
/// Projectile that moves forward and damages targets
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool useGravity = false;
    [SerializeField] private bool destroyOnImpact = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float impactEffectLifetime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip impactSound;

    // Internal state
    private Vector3 direction;
    private float speed;
    private float damage;
    private float maxRange;
    private Vector3 startPosition;
    private TeamComponent ownerTeam;
    private Rigidbody rb;
    private bool isInitialized = false;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// Initialize the projectile with parameters
    /// </summary>
    public void Initialize(Vector3 direction, float speed, float damage, float maxRange, TeamComponent ownerTeam)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.maxRange = maxRange;
        this.ownerTeam = ownerTeam;
        this.startPosition = transform.position;
        this.spawnTime = Time.time;

        isInitialized = true;

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = this.direction * this.speed;
        }

        // Set rotation to face direction
        if (this.direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(this.direction);
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isInitialized) return;

        // Check if exceeded max range
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > maxRange)
        {
            DestroyProjectile(false);
        }
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

        // Move projectile (if not using Rigidbody velocity)
        if (rb == null || useGravity)
        {
            transform.position += direction * speed * Time.fixedDeltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInitialized) return;

        // Don't collide immediately after spawn
        if (Time.time - spawnTime < 0.1f) return;

        // Check if hit owner
        if (other.transform.IsChildOf(ownerTeam?.transform))
        {
            return; // Don't hit self
        }

        // Check for BaseUnit
        BaseUnit hitUnit = other.GetComponent<BaseUnit>();
        if (hitUnit == null)
        {
            hitUnit = other.GetComponentInParent<BaseUnit>();
        }

        if (hitUnit != null)
        {
            // Check team
            TeamComponent hitTeam = hitUnit.GetComponent<TeamComponent>();

            // Don't damage allies
            if (hitTeam != null && ownerTeam != null)
            {
                if (!ownerTeam.IsEnemy(hitTeam))
                {
                    return; // Same team or ally
                }
            }

            // Apply damage (will be implemented when we add Health system)
            OnHitTarget(hitUnit, other.ClosestPoint(transform.position));
        }
        else
        {
            // Hit environment
            OnHitEnvironment(other.ClosestPoint(transform.position));
        }

        if (destroyOnImpact)
        {
            DestroyProjectile(true);
        }
    }

    /// <summary>
    /// Called when projectile hits a target unit
    /// </summary>
    protected virtual void OnHitTarget(BaseUnit target, Vector3 hitPoint)
    {
        Debug.Log($"Projectile hit {target.UnitName} for {damage} damage at {hitPoint}");

        // Apply damage to health component
        Health healthComponent = target.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage, ownerTeam);
        }

        SpawnImpactEffect(hitPoint);
    }

    /// <summary>
    /// Called when projectile hits environment
    /// </summary>
    protected virtual void OnHitEnvironment(Vector3 hitPoint)
    {
        SpawnImpactEffect(hitPoint);
    }

    /// <summary>
    /// Spawn impact visual effect
    /// </summary>
    private void SpawnImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            Destroy(effect, impactEffectLifetime);
        }

        if (impactSound != null)
        {
            // Use AudioManager if available for proper mixer group assignment
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(impactSound, position, AudioManager.AudioCategory.WeaponSounds);
            }
            else
            {
                AudioSource.PlayClipAtPoint(impactSound, position);
            }
        }
    }

    /// <summary>
    /// Destroy the projectile
    /// </summary>
    private void DestroyProjectile(bool wasImpact)
    {
        // Detach trail before destroying
        if (trail != null)
        {
            trail.transform.SetParent(null);
            Destroy(trail.gameObject, trail.time);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmos()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, direction * 2f);

            // Draw max range from start position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, maxRange);
        }
    }
}
