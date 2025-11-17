using UnityEngine;

/// <summary>
/// Component for all buildings - handles energy management and destruction
/// </summary>
[RequireComponent(typeof(Health))]
public class BuildingComponent : MonoBehaviour
{
    [Header("Building Info")]
    [SerializeField] private Product buildingProduct;
    [SerializeField] private ResourceManager resourceManager;

    [Header("Status")]
    [SerializeField] private bool isConstructed = true;
    [SerializeField] private bool isPowered = true;

    private Health healthComponent;
    private bool energyApplied = false; // Prevent double application

    // Properties
    public Product BuildingProduct => buildingProduct;
    public bool IsConstructed => isConstructed;
    public bool IsPowered => isPowered;
    public bool IsEnergyProducer => buildingProduct != null && buildingProduct.EnergyProduction > 0;
    public bool IsHeadquarter => buildingProduct != null && buildingProduct.BuildingType == BuildingType.Headquarter;

    void Awake()
    {
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            healthComponent = gameObject.AddComponent<Health>();
        }
    }

    void Start()
    {
        // Subscribe to death event
        if (healthComponent != null)
        {
            healthComponent.OnDeath.AddListener(OnBuildingDestroyed);
        }

        // Only run energy logic if already initialized
        // (Initialize() will handle it if called before Start())
        if (buildingProduct != null && resourceManager != null)
        {
            ApplyEnergyChanges();
        }
    }

    /// <summary>
    /// Initialize building with product data
    /// </summary>
    public void Initialize(Product product, ResourceManager manager)
    {
        buildingProduct = product;
        resourceManager = manager;

        // Set building properties based on product
        if (product != null)
        {
            // You can set health, armor, etc. from product here
            gameObject.name = product.ProductName;
        }

        // Apply energy changes immediately (before Start() might run)
        if (product != null && manager != null)
        {
            ApplyEnergyChanges();
        }
    }

    /// <summary>
    /// Apply energy production/consumption
    /// </summary>
    private void ApplyEnergyChanges()
    {
        if (buildingProduct == null || resourceManager == null)
        {
            return;
        }

        // Prevent double application
        if (!IsEnergyProducer && energyApplied)
        {
            Debug.LogWarning($"{buildingProduct.ProductName}: Energy already applied, skipping.");
            return;
        }

        // Add energy production if applicable
        if (IsEnergyProducer)
        {
            resourceManager.IncreaseMaxEnergy(buildingProduct.EnergyProduction);
            Debug.Log($"? {buildingProduct.ProductName} providing {buildingProduct.EnergyProduction} energy. New max: {resourceManager.MaxEnergy}");
        }

        // Check if we have enough energy for buildings that consume it
        UpdatePowerStatus();

        energyApplied = true;
    }

    /// <summary>
    /// Update power status based on available energy
    /// </summary>
    private void UpdatePowerStatus()
    {
        if (buildingProduct == null || resourceManager == null)
        {
            isPowered = true;
            return;
        }

        // Headquarters and Energy blocks are always powered (they don't consume energy)
        if (buildingProduct.BuildingType == BuildingType.Headquarter ||
          buildingProduct.BuildingType == BuildingType.EnergyBlock)
        {
            isPowered = true;
            return;
        }

        // Buildings with 0 energy cost are always powered
        if (buildingProduct.EnergyCost == 0)
        {
            isPowered = true;
            return;
        }

        // Check if we have the required energy
        isPowered = resourceManager.HasAvailableEnergy(buildingProduct.EnergyCost);

        if (!isPowered)
        {
            Debug.LogWarning($"{buildingProduct.ProductName} is not powered! Needs {buildingProduct.EnergyCost} energy, but only {resourceManager.AvailableEnergy} available.");
            // TODO: Add visual feedback for unpowered state
        }
    }

    /// <summary>
    /// Called when building is destroyed
    /// </summary>
    private void OnBuildingDestroyed()
    {
        if (resourceManager == null || buildingProduct == null)
        {
            return;
        }

        // Release consumed energy
        if (buildingProduct.EnergyCost > 0)
        {
            resourceManager.ReleaseEnergy(buildingProduct.EnergyCost);
            Debug.Log($"{buildingProduct.ProductName} released {buildingProduct.EnergyCost} energy");
        }

        // Remove energy production
        if (buildingProduct.EnergyProduction > 0)
        {
            resourceManager.DecreaseMaxEnergy(buildingProduct.EnergyProduction);
            Debug.Log($"{buildingProduct.ProductName} removed {buildingProduct.EnergyProduction} energy production");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (healthComponent != null)
        {
            healthComponent.OnDeath.RemoveListener(OnBuildingDestroyed);
        }
    }

    void OnDrawGizmos()
    {
        // Draw building info
        if (buildingProduct != null)
        {
            Gizmos.color = isPowered ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.5f);
        }
    }
}
