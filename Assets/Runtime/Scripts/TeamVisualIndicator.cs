using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides clear visual indicators for team affiliation
/// Shows the player which units belong to which team
/// </summary>
public class TeamVisualIndicator : MonoBehaviour
{
    [Header("Visual Indicator Settings")]
    [SerializeField] private IndicatorType indicatorType = IndicatorType.ColorRing;
    [SerializeField] private bool showIndicator = true;
    [SerializeField] private float indicatorHeightOffset = 2f;
    [SerializeField] private float indicatorScale = 1f;

    [Header("Color Ring Settings")]
    [SerializeField] private GameObject colorRingPrefab;
    [SerializeField] private float ringRotationSpeed = 30f;
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.2f;

    [Header("Shield Icon Settings")]
    [SerializeField] private GameObject shieldIconPrefab;
    [SerializeField] private bool billboardIcon = true;

    [Header("Material Tint Settings")]
    [SerializeField] private bool tintMaterials = true;
    [SerializeField] private float tintStrength = 0.3f;
    [SerializeField] private string[] materialPropertyNames = { "_BaseColor", "_Color", "_TintColor" };

    [Header("Outline Settings")]
    [SerializeField] private bool useOutline = false;
    [SerializeField] private float outlineWidth = 0.05f;

    private TeamComponent teamComponent;
    private GameObject indicatorObject;
    private Renderer indicatorRenderer;
    private Material indicatorMaterial;
    private Renderer[] unitRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;
    private float pulseTimer = 0f;

    public enum IndicatorType
    {
        ColorRing,          // Colored ring at unit's feet
        ShieldIcon,         // Shield icon above unit
        MaterialTint,       // Tint unit materials
        Outline, // Outline effect
        Combined            // Multiple indicators
    }

    void Awake()
    {
        teamComponent = GetComponent<TeamComponent>();
        if (teamComponent == null)
        {
            Debug.LogWarning($"TeamVisualIndicator on {gameObject.name} requires a TeamComponent!");
            enabled = false;
            return;
        }

        unitRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        mainCamera = Camera.main;
    }

    void Start()
    {
        if (showIndicator)
        {
            CreateVisualIndicator();
        }
    }

    void Update()
    {
        if (indicatorObject != null && indicatorObject.activeSelf)
        {
            UpdateIndicatorEffects();
        }
    }

    /// <summary>
    /// Create the visual indicator based on type
    /// </summary>
    private void CreateVisualIndicator()
    {
        switch (indicatorType)
        {
            case IndicatorType.ColorRing:
                CreateColorRing();
                break;
            case IndicatorType.ShieldIcon:
                CreateShieldIcon();
                break;
            case IndicatorType.MaterialTint:
                ApplyMaterialTint();
                break;
            case IndicatorType.Outline:
                ApplyOutlineEffect();
                break;
            case IndicatorType.Combined:
                CreateColorRing();
                ApplyMaterialTint();
                break;
        }
    }

    /// <summary>
    /// Create a colored ring at the unit's base
    /// </summary>
    private void CreateColorRing()
    {
        GameObject ring;

        // Use prefab if available, otherwise create a simple ring
        if (colorRingPrefab != null)
        {
            ring = Instantiate(colorRingPrefab, transform);
        }
        else
        {
            // Create a simple flat ring using a plane
            ring = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ring.name = "TeamColorRing";
            ring.transform.SetParent(transform);

            // Remove collider
            Collider collider = ring.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }

        // Position the ring
        ring.transform.localPosition = new Vector3(0, 0.01f, 0);
        ring.transform.localScale = Vector3.one * indicatorScale * 0.3f;
        ring.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Create or get material
        indicatorRenderer = ring.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            // Create a new unlit material with team color
            Material ringMaterial = new Material(Shader.Find("Unlit/Color"));
            ringMaterial.color = teamComponent.TeamColor;

            // Make it emissive for better visibility
            if (ringMaterial.HasProperty("_EmissionColor"))
            {
                ringMaterial.EnableKeyword("_EMISSION");
                ringMaterial.SetColor("_EmissionColor", teamComponent.TeamColor * 0.5f);
            }

            indicatorRenderer.material = ringMaterial;
            indicatorMaterial = ringMaterial;
        }

        indicatorObject = ring;
    }

    /// <summary>
    /// Create a shield icon above the unit
    /// </summary>
    private void CreateShieldIcon()
    {
        GameObject icon;

        if (shieldIconPrefab != null)
        {
            icon = Instantiate(shieldIconPrefab, transform);
        }
        else
        {
            // Create a simple quad for the icon
            icon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            icon.name = "TeamShieldIcon";
            icon.transform.SetParent(transform);

            // Remove collider
            Collider collider = icon.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }

        // Position above unit
        icon.transform.localPosition = new Vector3(0, indicatorHeightOffset, 0);
        icon.transform.localScale = Vector3.one * indicatorScale * 0.5f;

        // Create material with team color
        indicatorRenderer = icon.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            Material iconMaterial = new Material(Shader.Find("Unlit/Color"));
            iconMaterial.color = teamComponent.TeamColor;
            indicatorRenderer.material = iconMaterial;
            indicatorMaterial = iconMaterial;
        }

        indicatorObject = icon;
    }

    /// <summary>
    /// Apply team color tint to unit materials
    /// </summary>
    private void ApplyMaterialTint()
    {
        if (!tintMaterials || unitRenderers == null) return;

        Color tintColor = teamComponent.TeamColor * tintStrength;

        foreach (Renderer renderer in unitRenderers)
        {
            if (renderer == null) continue;

            // Try different common color property names
            foreach (string propertyName in materialPropertyNames)
            {
                if (renderer.material.HasProperty(propertyName))
                {
                    renderer.GetPropertyBlock(propertyBlock);

                    // Get current color and blend with team color
                    Color currentColor = renderer.material.GetColor(propertyName);
                    Color newColor = Color.Lerp(currentColor, teamComponent.TeamColor, tintStrength);

                    propertyBlock.SetColor(propertyName, newColor);
                    renderer.SetPropertyBlock(propertyBlock);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Apply outline effect to unit
    /// </summary>
    private void ApplyOutlineEffect()
    {
        if (!useOutline || unitRenderers == null) return;

        foreach (Renderer renderer in unitRenderers)
        {
            if (renderer == null) continue;

            Material mat = renderer.material;

            // Try to set outline properties if the shader supports it
            if (mat.HasProperty("_OutlineWidth"))
            {
                mat.SetFloat("_OutlineWidth", outlineWidth);
            }

            if (mat.HasProperty("_OutlineColor"))
            {
                mat.SetColor("_OutlineColor", teamComponent.TeamColor);
            }
        }
    }

    /// <summary>
    /// Update indicator effects (rotation, pulse, billboard)
    /// </summary>
    private void UpdateIndicatorEffects()
    {
        if (indicatorObject == null) return;

        // Rotate color ring
        if (indicatorType == IndicatorType.ColorRing || indicatorType == IndicatorType.Combined)
        {
            indicatorObject.transform.Rotate(Vector3.up, ringRotationSpeed * Time.deltaTime);

            // Pulse effect
            if (pulseEffect)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float scale = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
                indicatorObject.transform.localScale = Vector3.one * indicatorScale * 0.3f * scale;
            }
        }

        // Billboard effect for icons
        if ((indicatorType == IndicatorType.ShieldIcon) && billboardIcon && mainCamera != null)
        {
            indicatorObject.transform.rotation = Quaternion.LookRotation(
       indicatorObject.transform.position - mainCamera.transform.position
                );
        }
    }

    /// <summary>
    /// Update team color if it changes
    /// </summary>
    public void UpdateTeamColor()
    {
        if (teamComponent == null) return;

        // Update indicator color
        if (indicatorMaterial != null)
        {
            indicatorMaterial.color = teamComponent.TeamColor;

            if (indicatorMaterial.HasProperty("_EmissionColor"))
            {
                indicatorMaterial.SetColor("_EmissionColor", teamComponent.TeamColor * 0.5f);
            }
        }

        // Update material tint
        if (tintMaterials)
        {
            ApplyMaterialTint();
        }

        // Update outline
        if (useOutline)
        {
            ApplyOutlineEffect();
        }
    }

    /// <summary>
    /// Show or hide the visual indicator
    /// </summary>
    public void SetIndicatorVisible(bool visible)
    {
        showIndicator = visible;

        if (indicatorObject != null)
        {
            indicatorObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Change indicator type at runtime
    /// </summary>
    public void SetIndicatorType(IndicatorType newType)
    {
        if (indicatorType == newType) return;

        // Clean up old indicator
        if (indicatorObject != null)
        {
            Destroy(indicatorObject);
            indicatorObject = null;
        }

        indicatorType = newType;

        if (showIndicator)
        {
            CreateVisualIndicator();
        }
    }

    void OnDestroy()
    {
        // Clean up materials
        if (indicatorMaterial != null)
        {
            Destroy(indicatorMaterial);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw indicator position preview
        Gizmos.color = teamComponent != null ? teamComponent.TeamColor : Color.white;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * indicatorHeightOffset, 0.3f * indicatorScale);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.01f, 0.3f * indicatorScale);
    }
}
