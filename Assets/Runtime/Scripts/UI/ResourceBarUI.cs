using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Component that displays player resources (Gold, Energy, etc.)
/// </summary>
public class ResourceBarUI : MonoBehaviour
{
    [Header("Resource Manager")]
    [SerializeField] private ResourceManager resourceManager;

    [Header("Gold Display")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Image goldIcon;

    [Header("Energy Display")]
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Image energyIcon;
    [SerializeField] private Image energyFillBar;

    [Header("Display Settings")]
    [SerializeField] private bool showGold = true;
    [SerializeField] private bool showEnergy = true;
    [SerializeField] private string goldFormat = "{0}";
    [SerializeField] private string energyFormat = "{0}/{1}";

    [Header("Energy Bar Colors")]
    [SerializeField] private Color energyFullColor = Color.green;
    [SerializeField] private Color energyMidColor = Color.yellow;
    [SerializeField] private Color energyLowColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float lowEnergyThreshold = 0.2f;
    [SerializeField, Range(0f, 1f)] private float midEnergyThreshold = 0.5f;

    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Tooltips (Optional)")]
    [SerializeField] private bool showTooltips = true;
    [SerializeField] private string goldTooltip = "Gold: Used for purchasing units and buildings";
    [SerializeField] private string energyTooltip = "Energy: Required to power buildings";

    // Animation
    private int currentGold;
    private int targetGold;
    private int currentEnergy;
    private int targetEnergy;
    private int currentMaxEnergy;
    private int targetMaxEnergy;
    private float animationTimer;

    void Awake()
    {
        // Auto-find ResourceManager if not assigned
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }

        if (resourceManager == null)
        {
            Debug.LogWarning("ResourceBarUI: No ResourceManager found! Create one in the scene.");
        }
    }

    void Start()
    {
        if (resourceManager != null)
        {
            // Subscribe to resource events
            resourceManager.OnResourcesChanged += OnResourcesChanged;
            resourceManager.OnEnergyChanged += OnEnergyChanged;

            // Initialize with current values
            currentGold = resourceManager.Gold;
            targetGold = currentGold;
            currentEnergy = resourceManager.CurrentEnergy;
            targetEnergy = currentEnergy;
            currentMaxEnergy = resourceManager.MaxEnergy;
            targetMaxEnergy = currentMaxEnergy;

            UpdateDisplay();
        }

        // Setup tooltips if enabled
        if (showTooltips)
        {
            SetupTooltips();
        }

        // Hide elements if disabled
        UpdateVisibility();
    }

    void Update()
    {
        // Animate value changes
        if (animateChanges && animationTimer > 0f)
        {
            animationTimer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(animationTimer / animationDuration);

            // Animate values
            if (currentGold != targetGold)
            {
                currentGold = Mathf.RoundToInt(Mathf.Lerp(currentGold, targetGold, t));
            }
            if (currentEnergy != targetEnergy)
            {
                currentEnergy = Mathf.RoundToInt(Mathf.Lerp(currentEnergy, targetEnergy, t));
            }
            if (currentMaxEnergy != targetMaxEnergy)
            {
                currentMaxEnergy = Mathf.RoundToInt(Mathf.Lerp(currentMaxEnergy, targetMaxEnergy, t));
            }

            UpdateDisplay();
        }
    }

    private void OnResourcesChanged(int food, int wood, int stone, int gold)
    {
        if (animateChanges)
        {
            targetGold = gold;
            animationTimer = animationDuration;
        }
        else
        {
            currentGold = gold;
            targetGold = gold;
            UpdateDisplay();
        }
    }

    private void OnEnergyChanged(int current, int max)
    {
        if (animateChanges)
        {
            targetEnergy = current;
            targetMaxEnergy = max;
            animationTimer = animationDuration;
        }
        else
        {
            currentEnergy = current;
            targetEnergy = current;
            currentMaxEnergy = max;
            targetMaxEnergy = max;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        // Update Gold
        if (showGold && goldText != null)
        {
            goldText.text = string.Format(goldFormat, currentGold);
        }

        // Update Energy
        if (showEnergy && energyText != null)
        {
            int available = currentMaxEnergy - currentEnergy;
            energyText.text = string.Format(energyFormat, available, currentMaxEnergy);

            // Update energy bar color
            if (energyFillBar != null && currentMaxEnergy > 0)
            {
                float fillAmount = (float)available / currentMaxEnergy;
                energyFillBar.fillAmount = fillAmount;

                // Color based on available energy
                if (fillAmount <= lowEnergyThreshold)
                {
                    energyFillBar.color = energyLowColor;
                }
                else if (fillAmount <= midEnergyThreshold)
                {
                    energyFillBar.color = energyMidColor;
                }
                else
                {
                    energyFillBar.color = energyFullColor;
                }
            }
        }
    }

    private void UpdateVisibility()
    {
        if (goldText != null) goldText.gameObject.SetActive(showGold);
        if (goldIcon != null) goldIcon.gameObject.SetActive(showGold);
        if (energyText != null) energyText.gameObject.SetActive(showEnergy);
        if (energyIcon != null) energyIcon.gameObject.SetActive(showEnergy);
        if (energyFillBar != null) energyFillBar.gameObject.SetActive(showEnergy);
    }

    private void SetupTooltips()
    {
        // Add tooltip components if available
        // This is optional and depends on your tooltip system
        // For now, we'll just add the tooltip text to the objects

        if (goldText != null)
        {
            AddTooltip(goldText.gameObject, goldTooltip);
        }
        if (energyText != null)
        {
            AddTooltip(energyText.gameObject, energyTooltip);
        }
    }

    private void AddTooltip(GameObject obj, string tooltip)
    {
        // Check if a tooltip component exists
        var tooltipComponent = obj.GetComponent<UITooltip>();
        if (tooltipComponent == null && !string.IsNullOrEmpty(tooltip))
        {
            tooltipComponent = obj.AddComponent<UITooltip>();
            tooltipComponent.tooltipText = tooltip;
        }
    }

    /// <summary>
    /// Manually set the resource manager (useful for testing or dynamic assignment)
    /// </summary>
    public void SetResourceManager(ResourceManager manager)
    {
        // Unsubscribe from old manager
        if (resourceManager != null)
        {
            resourceManager.OnResourcesChanged -= OnResourcesChanged;
            resourceManager.OnEnergyChanged -= OnEnergyChanged;
        }

        resourceManager = manager;

        // Subscribe to new manager
        if (resourceManager != null)
        {
            resourceManager.OnResourcesChanged += OnResourcesChanged;
            resourceManager.OnEnergyChanged += OnEnergyChanged;

            // Update immediately
            currentGold = resourceManager.Gold;
            targetGold = currentGold;
            currentEnergy = resourceManager.CurrentEnergy;
            targetEnergy = currentEnergy;
            currentMaxEnergy = resourceManager.MaxEnergy;
            targetMaxEnergy = currentMaxEnergy;
            UpdateDisplay();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (resourceManager != null)
        {
            resourceManager.OnResourcesChanged -= OnResourcesChanged;
            resourceManager.OnEnergyChanged -= OnEnergyChanged;
        }
    }

    // Editor helpers
    void OnValidate()
    {
        UpdateVisibility();
    }
}

/// <summary>
/// Simple tooltip component (optional)
/// </summary>
public class UITooltip : MonoBehaviour
{
    public string tooltipText;
    // Implement your tooltip display logic here
    // This is just a placeholder
}
