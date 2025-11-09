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

    public bool IsSelected => isSelected;
    public bool IsBuilding => isBuilding;
    public string UnitName => unitName;

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
    }

    /// <summary>
    /// Called when unit is deselected - override in derived classes
    /// </summary>
    protected virtual void OnDeselected()
    {
        Debug.Log($"{unitName} deselected");
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
