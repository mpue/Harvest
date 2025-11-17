using UnityEngine;

/// <summary>
/// Manages the production UI system
/// </summary>
public class ProductionUIManager : MonoBehaviour
{
    private static ProductionUIManager instance;
    public static ProductionUIManager Instance => instance;

    [Header("References")]
    [SerializeField] private ProductionPanel productionPanel;

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Find production panel if not assigned
        if (productionPanel == null)
        {
            productionPanel = FindObjectOfType<ProductionPanel>();
        }

        if (productionPanel == null)
        {
            Debug.LogError("ProductionPanel not found! Please assign it in the ProductionUIManager or add one to the scene.");
        }
    }

    /// <summary>
    /// Show production panel for a specific BaseUnit
    /// </summary>
    public void ShowProductionPanel(BaseUnit baseUnit)
    {
        if (productionPanel != null)
        {
            productionPanel.Show(baseUnit);
        }
        else
        {
            Debug.LogWarning("Cannot show production panel: ProductionPanel reference is missing");
        }
    }

    /// <summary>
    /// Hide the production panel
    /// </summary>
    public void HideProductionPanel()
    {
        if (productionPanel != null)
        {
            productionPanel.Hide();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
