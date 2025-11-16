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

   // Add energy production if applicable
 if (IsEnergyProducer && resourceManager != null)
    {
    resourceManager.IncreaseMaxEnergy(buildingProduct.EnergyProduction);
          Debug.Log($"{buildingProduct.ProductName} providing {buildingProduct.EnergyProduction} energy");
        }

        // Check if we have enough energy
        UpdatePowerStatus();
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

        // Energy blocks are always powered
 if (buildingProduct.BuildingType == BuildingType.EnergyBlock)
  {
            isPowered = true;
  return;
 }

        // Check if we have the required energy
        isPowered = resourceManager.HasAvailableEnergy(buildingProduct.EnergyCost);

        if (!isPowered)
        {
            Debug.LogWarning($"{buildingProduct.ProductName} is not powered!");
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
