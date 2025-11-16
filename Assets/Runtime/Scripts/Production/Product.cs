using UnityEngine;

/// <summary>
/// ScriptableObject that defines a producible entity
/// </summary>
[CreateAssetMenu(fileName = "New Product", menuName = "RTS/Production/Product")]
public class Product : ScriptableObject
{
    [Header("Display Info")]
    [SerializeField] private string productName = "New Unit";
    [SerializeField] private Sprite previewImage;
    [SerializeField, TextArea(3, 5)] private string description;

    [Header("Product Type")]
    [SerializeField] private bool isBuilding = false;
    [SerializeField] private BuildingType buildingType = BuildingType.None;

    [Header("Production Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private float productionDuration = 10f;

    [Header("Costs")]
    [SerializeField] private int foodCost = 0;
    [SerializeField] private int woodCost = 0;
    [SerializeField] private int stoneCost = 0;
    [SerializeField] private int goldCost = 0;

    [Header("Energy Settings")]
    [SerializeField] private int energyCost = 0; // Energy consumed when placed
    [SerializeField] private int energyProduction = 0; // Energy provided (for power plants)

    // Properties
    public string ProductName => productName;
    public Sprite PreviewImage => previewImage;
    public string Description => description;
    public bool IsBuilding => isBuilding;
    public BuildingType BuildingType => buildingType;
    public GameObject Prefab => prefab;
    public float ProductionDuration => productionDuration;
    public int FoodCost => foodCost;
    public int WoodCost => woodCost;
    public int StoneCost => stoneCost;
    public int GoldCost => goldCost;
    public int EnergyCost => energyCost;
    public int EnergyProduction => energyProduction;

    /// <summary>
    /// Check if the player can afford this product
    /// </summary>
    /// </summary>
    public bool CanAfford(int availableFood, int availableWood, int availableStone, int availableGold)
    {
        return availableFood >= foodCost &&
        availableWood >= woodCost &&
        availableStone >= stoneCost &&
        availableGold >= goldCost;
    }

    /// <summary>
    /// Check if there is enough energy available
    /// </summary>
    public bool HasEnoughEnergy(ResourceManager resourceManager)
    {
        if (!isBuilding || buildingType == BuildingType.EnergyBlock)
        {
            return true; // Energy blocks can always be built
        }

        return resourceManager != null && resourceManager.HasAvailableEnergy(energyCost);
    }

    /// <summary>
    /// Get a formatted cost string
    /// </summary>
    public string GetCostString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (foodCost > 0) sb.Append($"Food: {foodCost} ");
        if (woodCost > 0) sb.Append($"Wood: {woodCost} ");
        if (stoneCost > 0) sb.Append($"Stone: {stoneCost} ");
        if (goldCost > 0) sb.Append($"Gold: {goldCost} ");

        if (isBuilding)
        {
            if (energyCost > 0) sb.Append($"Energy: {energyCost} ");
            if (energyProduction > 0) sb.Append($"[+{energyProduction} Energy] ");
        }

        return sb.ToString().TrimEnd();
    }
}

/// <summary>
/// Type of building
/// </summary>
public enum BuildingType
{
    None,
    Headquarter,
    EnergyBlock,
    ProductionFacility,
    DefenseTower,
    ResourceCollector
}
