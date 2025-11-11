using UnityEngine;

/// <summary>
/// Simple explosion effect that can damage nearby units
/// Automatically destroys itself after a set time
/// </summary>
public class Explosion : MonoBehaviour
{
    [Header("Damage Settings")]
  [SerializeField] private bool dealsDamage = true;
    [SerializeField] private float damageAmount = 50f;
 [SerializeField] private float damageRadius = 5f;
    [SerializeField] private LayerMask damageLayerMask = -1;
  [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Force Settings")]
    [SerializeField] private bool applyForce = true;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float upwardsModifier = 1f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem[] particleSystems;
  [SerializeField] private Light explosionLight;
    [SerializeField] private float lightFadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float audioVolume = 1f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f;

    private TeamComponent ownerTeam;
    private float initialLightIntensity;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;

        // Store initial light intensity
        if (explosionLight != null)
     {
            initialLightIntensity = explosionLight.intensity;
        }

        // Play explosion sound
        if (explosionSound != null)
        {
  AudioSource.PlayClipAtPoint(explosionSound, transform.position, audioVolume);
        }

// Deal damage
  if (dealsDamage)
     {
      DealExplosionDamage();
        }

   // Apply force
        if (applyForce)
        {
       ApplyExplosionForce();
        }

        // Auto-destroy
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
// Fade out light
      if (explosionLight != null && lightFadeDuration > 0)
        {
 float age = Time.time - spawnTime;
       float fadeProgress = Mathf.Clamp01(age / lightFadeDuration);
    explosionLight.intensity = Mathf.Lerp(initialLightIntensity, 0, fadeProgress);
        }
    }

    /// <summary>
    /// Initialize explosion with owner team
    /// </summary>
    public void Initialize(TeamComponent owner)
    {
      ownerTeam = owner;
    }

    /// <summary>
    /// Deal damage to units in radius
    /// </summary>
    private void DealExplosionDamage()
    {
     Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius, damageLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            // Find BaseUnit
    BaseUnit unit = hitCollider.GetComponent<BaseUnit>();
   if (unit == null)
 {
     unit = hitCollider.GetComponentInParent<BaseUnit>();
}

     if (unit == null) continue;

 // Check team
            TeamComponent targetTeam = unit.GetComponent<TeamComponent>();
            if (targetTeam != null && ownerTeam != null)
     {
   if (!ownerTeam.IsEnemy(targetTeam))
        {
           continue; // Don't damage allies
      }
  }

     // Calculate distance-based damage
  float distance = Vector3.Distance(transform.position, unit.transform.position);
        float normalizedDistance = Mathf.Clamp01(distance / damageRadius);
    float damageMultiplier = damageFalloff.Evaluate(normalizedDistance);
            float finalDamage = damageAmount * damageMultiplier;

            // Apply damage
   Health health = unit.GetComponent<Health>();
            if (health != null)
            {
   health.TakeDamage(finalDamage, ownerTeam);
Debug.Log($"Explosion damaged {unit.UnitName} for {finalDamage} damage (distance: {distance:F1}m)");
    }
   }
    }

    /// <summary>
    /// Apply physics force to rigidbodies in radius
/// </summary>
    private void ApplyExplosionForce()
    {
   Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius, damageLayerMask);

foreach (Collider hitCollider in hitColliders)
        {
       Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
         if (rb != null && !rb.isKinematic)
            {
        rb.AddExplosionForce(explosionForce, transform.position, damageRadius, upwardsModifier, ForceMode.Impulse);
          }
        }
  }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmosSelected()
{
        // Draw damage radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, damageRadius);

        // Draw wireframe
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
