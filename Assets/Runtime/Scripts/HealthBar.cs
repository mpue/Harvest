using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that follows a unit
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health healthComponent;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Canvas canvas;

    [Header("Display Settings (main)")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool onlyShowWhenSelected = true; // NEW: Only show when unit is selected
    [SerializeField] private bool alwaysFaceCamera = true;
    [SerializeField] private bool overrideParentRotation = true; // NEW: Completely ignore parent rotation
    [SerializeField] private float hiddenHealthThreshold = 0.99f;
    [SerializeField] private bool instantSelectionToggle = true; // NEW: Instant show/hide on selection change

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float midHealthThreshold = 0.5f;
    [SerializeField] private float lowHealthThreshold = 0.25f;

    [Header("Animation")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;

    private Camera mainCamera;
    private float targetFillAmount = 1f;
    private CanvasGroup canvasGroup;
    private BaseUnit baseUnit; // NEW: Reference to BaseUnit for selection state
    private bool wasSelectedLastFrame = false; // NEW: Track selection state changes

    void Awake()
    {
        // Auto-find health component if not assigned
        if (healthComponent == null)
        {
            healthComponent = GetComponentInParent<Health>();
        }

        // NEW: Find BaseUnit for selection state
        if (healthComponent != null)
        {
            baseUnit = healthComponent.GetComponent<BaseUnit>();
        }

        // Setup canvas
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        // Setup canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Find main camera
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        // Subscribe to health events
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged.AddListener(OnHealthChanged);
            healthComponent.OnDeath.AddListener(OnDeath);

            // Initialize with current health
            UpdateHealthBar(healthComponent.HealthPercentage);
        }
    }

    void OnDisable()
    {
        // Unsubscribe from health events
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged.RemoveListener(OnHealthChanged);
            healthComponent.OnDeath.RemoveListener(OnDeath);
        }
    }

    void LateUpdate()
    {
        if (healthComponent == null) return;

        // Position healthbar above unit (in world space)
        Vector3 targetPosition = healthComponent.transform.position + offset;

        // If overriding parent rotation, work in world space
        if (overrideParentRotation)
        {
            // Set world position directly
            transform.position = targetPosition;
        }
        else
        {
            // Use local offset
            transform.position = targetPosition;
        }

        // FIXED: Make healthbar always face camera correctly
        if (alwaysFaceCamera && mainCamera != null)
        {
            if (overrideParentRotation)
            {
                // Directly set world rotation to face camera
                // This completely ignores any parent rotation
                Vector3 lookDirection = mainCamera.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = rotation;
            }
            else
            {
                // Simple billboard effect - always look at camera
                // This ensures the healthbar never rotates with the parent
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }

        // Smooth fill transition
        if (smoothTransition && fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
        }

        // NEW: Determine visibility based on selection and health
        UpdateVisibility();
    }

    /// <summary>
    /// NEW: Update healthbar visibility based on selection and health
    /// </summary>
    private void UpdateVisibility()
    {
        if (canvasGroup == null) return;

        bool shouldShow = true;

        // Check if should only show when selected
        if (onlyShowWhenSelected && baseUnit != null)
        {
            shouldShow = baseUnit.IsSelected;
        }

        // Simple: Set alpha directly
        canvasGroup.alpha = shouldShow ? 1f : 0f;
    }

    /// <summary>
    /// Called when health changes
    /// </summary>
    private void OnHealthChanged(float current, float max)
    {
        float percentage = max > 0 ? current / max : 0f;
        UpdateHealthBar(percentage);
    }

    /// <summary>
    /// Called when unit dies
    /// </summary>
    private void OnDeath()
    {
        // Optionally hide healthbar on death
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Update the health bar visual
    /// </summary>
    private void UpdateHealthBar(float percentage)
    {
        percentage = Mathf.Clamp01(percentage);

        if (smoothTransition)
        {
            targetFillAmount = percentage;
        }
        else if (fillImage != null)
        {
            fillImage.fillAmount = percentage;
        }

    }

    /// <summary>
    /// Get health bar color based on percentage
    /// </summary>
    private Color GetHealthColor(float percentage)
    {
        if (percentage <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (percentage <= midHealthThreshold)
        {
            // Lerp between low and mid
            float t = (percentage - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else
        {
            // Lerp between mid and full
            float t = (percentage - midHealthThreshold) / (1f - midHealthThreshold);
            return Color.Lerp(midHealthColor, fullHealthColor, t);
        }
    }

    /// <summary>
    /// Manually set health bar visibility
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    /// <summary>
    /// Setup healthbar with automatic configuration
    /// </summary>
    public static HealthBar CreateHealthBar(GameObject unit, GameObject healthBarPrefab)
    {
        if (unit == null || healthBarPrefab == null)
        {
            Debug.LogError("Cannot create healthbar: unit or prefab is null");
            return null;
        }

        GameObject healthBarObj = Instantiate(healthBarPrefab, unit.transform);
        HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();

        if (healthBar != null)
        {
            healthBar.healthComponent = unit.GetComponent<Health>();
        }

        return healthBar;
    }
}
