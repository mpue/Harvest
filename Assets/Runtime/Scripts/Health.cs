using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health component for units and buildings
/// Handles damage, death, and health regeneration
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRegenerate = false;
    [SerializeField] private float regenerationRate = 5f; // HP per second
    [SerializeField] private float regenerationDelay = 3f; // Delay after taking damage

    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionScale = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private float audioVolume = 1f;

    [Header("Visual Feedback")]
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    // Events
    public UnityEvent<float, float> OnHealthChanged; // current, max
    public UnityEvent<float> OnDamaged; // damage amount
    public UnityEvent OnDeath;
    public UnityEvent OnFullyHealed;

    // Internal state
    private bool isDead = false;
    private float timeSinceLastDamage = 0f;
    private AudioSource audioSource;
    private Renderer[] unitRenderers;
    private Material[] originalMaterials;
    private BaseUnit baseUnit;

    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;
    public bool IsFullHealth => currentHealth >= maxHealth;
    public bool IsDamaged => currentHealth < maxHealth;

    void Awake()
    {
        // Initialize health to max
        currentHealth = maxHealth;

        // Get components
        baseUnit = GetComponent<BaseUnit>();
        unitRenderers = GetComponentsInChildren<Renderer>();

        // Setup AudioSource with AudioManager
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Use AudioManager if available to create AudioSource with proper mixer group
            if (AudioManager.Instance != null)
            {
                audioSource = AudioManager.Instance.CreateAudioSource(gameObject, AudioManager.AudioCategory.UnitSounds, false);
            }
            else
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else if (AudioManager.Instance != null)
        {
            // Setup existing AudioSource with mixer group
            AudioManager.Instance.SetupAudioSource(audioSource, AudioManager.AudioCategory.UnitSounds);
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = audioVolume;

        // Store original materials for flash effect
        if (flashOnHit && unitRenderers.Length > 0)
        {
            originalMaterials = new Material[unitRenderers.Length];
            for (int i = 0; i < unitRenderers.Length; i++)
            {
                if (unitRenderers[i] != null)
                {
                    originalMaterials[i] = unitRenderers[i].material;
                }
            }
        }
    }

    void Start()
    {
        // Trigger initial health event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        // Handle regeneration
        if (canRegenerate && currentHealth < maxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenerationDelay)
            {
                Heal(regenerationRate * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Apply damage to this unit
    /// </summary>
    public void TakeDamage(float damage, TeamComponent attacker = null)
    {
        if (isDead || damage <= 0) return;

        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Reset regeneration timer
        timeSinceLastDamage = 0f;

        // Trigger events
        OnDamaged?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Visual and audio feedback
        PlayHitSound();
        if (flashOnHit)
        {
            StartCoroutine(HitFlashCoroutine());
        }

        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die(attacker);
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Heal this unit
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead || amount <= 0) return;

        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Trigger events if health actually changed
        if (currentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth >= maxHealth)
            {
                OnFullyHealed?.Invoke();
            }
        }
    }

    /// <summary>
    /// Set health to a specific value
    /// </summary>
    public void SetHealth(float health)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Instantly kill this unit
    /// </summary>
    public void Kill(TeamComponent killer = null)
    {
        if (isDead) return;
        currentHealth = 0;
        Die(killer);
    }

    /// <summary>
    /// Handle death
    /// </summary>
    private void Die(TeamComponent killer = null)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        // Trigger death event
        OnDeath?.Invoke();

        // Play death sound
        PlayDeathSound();

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionScale;
            Destroy(explosion, 5f);
        }

        // Disable components
        DisableComponents();

        // Destroy or disable unit
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Disable unit components on death
    /// </summary>
    private void DisableComponents()
    {
        // Disable controllable
        Controllable controllable = GetComponent<Controllable>();
        if (controllable != null)
        {
            controllable.enabled = false;
        }

        // Disable weapons
        WeaponController weaponController = GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.enabled = false;
        }

        // Disable selection
        if (baseUnit != null && baseUnit.IsSelected)
        {
            baseUnit.Deselect();
        }

        // Disable collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// Play random hit sound
    /// </summary>
    private void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0 || audioSource == null)
            return;

        int randomIndex = Random.Range(0, hitSounds.Length);
        AudioClip clip = hitSounds[randomIndex];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }

    /// <summary>
    /// Play random death sound
    /// </summary>
    private void PlayDeathSound()
    {
        if (deathSounds == null || deathSounds.Length == 0 || audioSource == null)
            return;

        int randomIndex = Random.Range(0, deathSounds.Length);
        AudioClip clip = deathSounds[randomIndex];

        if (clip != null)
        {
            // Create temporary audio source that destroys itself
            GameObject tempAudio = new GameObject("DeathSound");
            tempAudio.transform.position = transform.position;
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = audioVolume;
            tempSource.spatialBlend = 1f;
            tempSource.Play();
            Destroy(tempAudio, clip.length);
        }
    }

    /// <summary>
    /// Visual hit flash effect
    /// </summary>
    private System.Collections.IEnumerator HitFlashCoroutine()
    {
        // Set flash color
        foreach (Renderer renderer in unitRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.SetColor("_EmissionColor", hitFlashColor);
                renderer.material.EnableKeyword("_EMISSION");
            }
        }

        yield return new WaitForSeconds(hitFlashDuration);

        // Reset to original
        for (int i = 0; i < unitRenderers.Length; i++)
        {
            if (unitRenderers[i] != null && originalMaterials[i] != null)
            {
                unitRenderers[i].material = originalMaterials[i];
            }
        }
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw health bar in editor
        if (Application.isEditor)
        {
            Vector3 position = transform.position + Vector3.up * 2f;
            float barWidth = 1f;
            float barHeight = 0.1f;

            // Background (red)
            Gizmos.color = Color.red;
            Gizmos.DrawCube(position, new Vector3(barWidth, barHeight, 0.01f));

            // Foreground (green)
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            Gizmos.color = Color.green;
            Vector3 healthBarPos = position - new Vector3(barWidth * (1f - healthPercent) * 0.5f, 0, 0);
            Gizmos.DrawCube(healthBarPos, new Vector3(barWidth * healthPercent, barHeight, 0.02f));
        }
    }
}
