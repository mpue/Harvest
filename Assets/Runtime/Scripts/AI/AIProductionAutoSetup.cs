using UnityEngine;

/// <summary>
/// Auto-setup for AI production buildings - ensures Factory/Barracks have correct products assigned
/// </summary>
[ExecuteAlways]
public class AIProductionAutoSetup : MonoBehaviour
{
 [Header("Auto-Setup")]
 [SerializeField] private bool runSetupOnAwake = true;

 void Awake()
 {
 if (!runSetupOnAwake) return;

 ProductionComponent production = GetComponent<ProductionComponent>();
 BuildingComponent building = GetComponent<BuildingComponent>();

 if (production == null || building == null || building.BuildingProduct == null)
 return;

 string buildingName = building.BuildingProduct.ProductName;

 // Only setup if AvailableProducts is empty
 if (production.AvailableProducts == null || production.AvailableProducts.Count >0)
 return;

 Debug.Log($"AI ProductionAutoSetup: Setting up products for {buildingName}");

 // Load products from Resources folder
 if (buildingName.Contains("Factory"))
 {
 AddProductIfExists(production, "Products/Harvester");
 AddProductIfExists(production, "Products/MK3");
 AddProductIfExists(production, "Products/Tank");
 }
 else if (buildingName.Contains("Barracks"))
 {
 AddProductIfExists(production, "Products/Soldier");
 AddProductIfExists(production, "Products/Infantry");
 }

 Debug.Log($"AI ProductionAutoSetup: {buildingName} now has {production.AvailableProducts.Count} products");
 }

 private void AddProductIfExists(ProductionComponent production, string resourcePath)
 {
 Product product = Resources.Load<Product>(resourcePath);
 if (product != null)
 {
 production.AvailableProducts.Add(product);
 Debug.Log($" + Added product: {product.ProductName}");
 }
 }
}
