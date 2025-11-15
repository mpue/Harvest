using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Unit Info")]
    [SerializeField] private string unitName = "Unit";
    [SerializeField] private bool isBuilding = false;

    private bool isSelected = false;
    private Renderer[] unitRenderers;
    private Material[] originalMaterials;
    private Color originalOutlineColor;
    private ProductionComponent productionComponent;
    private TeamComponent teamComponent;
    private TeamVisualIndicator teamVisualIndicator;

    public bool IsSelected => isSelected;
    public bool IsBuilding => isBuilding;
    public string UnitName => unitName;
    public ProductionComponent ProductionComponent => productionComponent;
    public TeamComponent TeamComponent => teamComponent;

    void Awake()
    {
        // Get all renderers for visual feedback
        unitRenderers = GetComponentsInChildren<Renderer>();

        // Store original materials
        if (unitRenderers.Length > 0)
        {
            originalMaterials = new Material[unitRenderers.Length];
            for (int i = 0; i < unitRenderers.Length; i++)
            {
                originalMaterials[i] = unitRenderers[i].material;
            }
        }

        // Get ProductionComponent if available
        productionComponent = GetComponent<ProductionComponent>();
  
        // Get TeamComponent if available
        teamComponent = GetComponent<TeamComponent>();
        
        // Get or add TeamVisualIndicator if unit has a team
        if (teamComponent != null)
        {
            teamVisualIndicator = GetComponent<TeamVisualIndicator>();
            if (teamVisualIndicator == null)
            {
                // Auto-add visual indicator if team exists but no indicator
                teamVisualIndicator = gameObject.AddComponent<TeamVisualIndicator>();
            }
        }
    }

    void Start()
    {
        // Hide selection indicator at start
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Selects this unit and shows visual feedback
    /// </summary>
    public void Select()
    {
        if (isSelected) return;

        isSelected = true;
        ShowSelectionVisual(true);
        OnSelected();
    }

    /// <summary>
    /// Deselects this unit and hides visual feedback
    /// </summary>
    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;
        ShowSelectionVisual(false);
        OnDeselected();
    }

    /// <summary>
    /// Shows or hides the selection visual feedback
    /// </summary>
    private void ShowSelectionVisual(bool show)
    {
        // Show/hide selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(show);
        }

        // Change material color or add outline effect
        if (unitRenderers != null && unitRenderers.Length > 0)
        {
            foreach (Renderer renderer in unitRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    // Set emission color for selected units
                    if (show)
                    {
                        renderer.material.EnableKeyword("_EMISSION");
                        renderer.material.SetColor("_EmissionColor", selectedColor * 0.3f);
                    }
                    else
                    {
                        renderer.material.DisableKeyword("_EMISSION");
                        renderer.material.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when unit is selected - override in derived classes
    /// </summary>
    protected virtual void OnSelected()
    {
        Debug.Log($"{unitName} selected");

        // Open/switch production panel if this unit has production capability
        if (productionComponent != null)
        {
            // Try to use ProductionUIManager first
            if (ProductionUIManager.Instance != null)
            {
                ProductionUIManager.Instance.ShowProductionPanel(this);
            }
            else
            {
                // Fallback to direct panel search
                ProductionPanel panel = FindObjectOfType<ProductionPanel>();
                if (panel != null)
                {
                    panel.Show(this);
                }
                else
                {
                    Debug.LogWarning("ProductionPanel not found in scene. Please add a ProductionPanel to your UI.");
                }
            }
        }
        else
        {
            // If this unit has no production capability, close any open production panel
            if (ProductionUIManager.Instance != null)
            {
                ProductionUIManager.Instance.HideProductionPanel();
            }
            else
            {
                ProductionPanel panel = FindObjectOfType<ProductionPanel>();
                if (panel != null)
                {
                    panel.Hide();
                }
            }
        }
    }

    /// <summary>
    /// Called when unit is deselected - override in derived classes
    /// </summary>
    protected virtual void OnDeselected()
    {
        Debug.Log($"{unitName} deselected");

        // Don't close production panel automatically
      // Panel should only close via Close button or when selecting a different production unit
        // This is typical RTS behavior (StarCraft, Age of Empires, etc.)
    }

    /// <summary>
    /// Check if this unit is within a screen space rect (for box selection)
    /// </summary>
    public bool IsWithinSelectionBounds(Rect selectionRect, Camera camera)
    {
        if (camera == null) return false;

        // Get the object's screen position
        Vector3 screenPos = camera.WorldToScreenPoint(transform.position);

        // Check if behind camera
        if (screenPos.z < 0) return false;

        // Convert screen position to GUI coordinates (flip Y)
        screenPos.y = Screen.height - screenPos.y;

        // Check if within selection rectangle
        return selectionRect.Contains(screenPos);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere when selected in editor
        Gizmos.color = selectedColor;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
