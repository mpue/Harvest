using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Component that enables a BaseUnit to produce entities
/// </summary>
public class ProductionComponent : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] private List<Product> availableProducts = new List<Product>();
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int maxQueueSize = 5;

    [Header("Rally Point")]
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private bool showRallyPointGizmo = true;

    [Header("Resource Management")]
    [SerializeField] private ResourceManager resourceManager;

    [Header("Building Placement")]
    [SerializeField] private BuildingPlacement buildingPlacement;

    // Production queue
    private Queue<ProductionQueueItem> productionQueue = new Queue<ProductionQueueItem>();
    private ProductionQueueItem currentProduction;
    private float currentProductionProgress = 0f;
    private BuildingComponent buildingComponent;

    // Events
    public event Action<Product> OnProductionStarted;
    public event Action<Product, GameObject> OnProductionCompleted;
    public event Action<Product> OnProductionCancelled;
    public event Action OnQueueChanged;

    // Properties
    public List<Product> AvailableProducts => availableProducts;
    public int QueueCount => productionQueue.Count + (currentProduction != null ? 1 : 0);
    public int MaxQueueSize => maxQueueSize;
    public bool IsProducing => currentProduction != null;
    public float CurrentProductionProgress => currentProductionProgress;
    public Product CurrentProduct => currentProduction?.product;
    public Transform RallyPoint => rallyPoint;
    public ResourceManager ResourceManager => resourceManager;
    public bool IsHeadquarter => buildingComponent != null && buildingComponent.IsHeadquarter;

    private class ProductionQueueItem
    {
        public Product product;
        public float startTime;

        public ProductionQueueItem(Product product)
        {
            this.product = product;
            this.startTime = Time.time;
        }
    }

    void Awake()
    {
        buildingComponent = GetComponent<BuildingComponent>();

        // Auto-find resource manager if not set - WITH TEAM CHECK!
        if (resourceManager == null)
        {
            TeamComponent myTeam = GetComponent<TeamComponent>();

            if (myTeam != null)
            {
                // Find ALL resource managers
                ResourceManager[] allManagers = FindObjectsOfType<ResourceManager>();

                // Try to find one with matching team name in GameObject name
                foreach (var manager in allManagers)
                {
                    // Check if manager GameObject name matches team
                    // e.g., "ResourceManagerAI" for Team.Enemy
                    if (myTeam.CurrentTeam != Team.Player && manager.gameObject.name.Contains("AI"))
                    {
                        resourceManager = manager;
                        Debug.Log($"Found AI ResourceManager: {manager.gameObject.name}");
                        break;
                    }
                    else if (myTeam.CurrentTeam == Team.Player && !manager.gameObject.name.Contains("AI"))
                    {
                        resourceManager = manager;
                        Debug.Log($"Found Player ResourceManager: {manager.gameObject.name}");
                        break;
                    }
                }
            }

            // Fallback: Use first found (old behavior)
            if (resourceManager == null)
            {
                resourceManager = FindObjectOfType<ResourceManager>();
                Debug.LogWarning($"Using fallback ResourceManager: {(resourceManager != null ? resourceManager.gameObject.name : "NONE")}");
            }
        }

        // Auto-find building placement if not set
        if (buildingPlacement == null)
        {
            buildingPlacement = FindObjectOfType<BuildingPlacement>();
        }

        // Create default spawn point if none is set
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = Vector3.forward * 3f;
            spawnPoint = spawnObj.transform;

            SpawnPoint spawnPointComponent = spawnObj.AddComponent<SpawnPoint>();
        }
        else
        {
            if (spawnPoint.GetComponent<SpawnPoint>() == null)
            {
                spawnPoint.gameObject.AddComponent<SpawnPoint>();
            }
        }

        // Create default rally point if none is set
        if (rallyPoint == null)
        {
            GameObject rallyObj = new GameObject("RallyPoint");
            rallyObj.transform.SetParent(transform);
            rallyObj.transform.localPosition = Vector3.forward * 5f;
            rallyPoint = rallyObj.transform;

            RallyPoint rallyPointComponent = rallyObj.AddComponent<RallyPoint>();
        }
        else
        {
            if (rallyPoint.GetComponent<RallyPoint>() == null)
            {
                rallyPoint.gameObject.AddComponent<RallyPoint>();
            }
        }
    }

    void Update()
    {
        if (currentProduction != null)
        {
            ProcessCurrentProduction();
        }
        else if (productionQueue.Count > 0)
        {
            StartNextProduction();
        }
    }

    /// <summary>
    /// Add a product to the production queue
    /// </summary>
    public bool AddToQueue(Product product)
    {
        if (product == null)
        {
            Debug.LogWarning("Cannot add null product to queue");
            return false;
        }

        if (QueueCount >= maxQueueSize)
        {
            Debug.LogWarning("Production queue is full");
            return false;
        }

        // Check if this is a building and only headquarters can produce buildings
        if (product.IsBuilding && !IsHeadquarter)
        {
            Debug.LogWarning("Only headquarters can produce buildings!");
            return false;
        }

        // Check resources
        if (resourceManager != null)
        {
            // Debug: Log current resources
            Debug.Log($"Checking resources for {product.ProductName}: Need Gold={product.GoldCost}, Have Gold={resourceManager.Gold}");

            if (!resourceManager.CanAfford(product.GoldCost))
            {
                Debug.LogWarning($"Cannot afford {product.ProductName}: Need Gold={product.GoldCost} but have Gold={resourceManager.Gold}");
                return false;
            }

            // For buildings (except energy blocks), check energy availability
            if (product.IsBuilding && product.BuildingType != BuildingType.EnergyBlock)
            {
                if (!product.HasEnoughEnergy(resourceManager))
                {
                    Debug.LogWarning($"Not enough energy for {product.ProductName}. Build more energy blocks!");
                    return false;
                }
            }

            // Spend resources
            if (!resourceManager.SpendResources(product.GoldCost))
            {
                Debug.LogWarning($"Failed to spend resources for {product.ProductName}");
                return false;
            }
        }

        productionQueue.Enqueue(new ProductionQueueItem(product));
        OnQueueChanged?.Invoke();

        Debug.Log($"Added {product.ProductName} to production queue. Queue size: {QueueCount}");

        return true;
    }

    /// <summary>
    /// Cancel the current production
    /// </summary>
    public void CancelCurrentProduction()
    {
        if (currentProduction != null)
        {
            Debug.Log($"Cancelled production of {currentProduction.product.ProductName}");

            // Refund resources (gold only)
            if (resourceManager != null)
            {
                Product p = currentProduction.product;
                resourceManager.AddResources(0, 0, 0, p.GoldCost);
            }

            OnProductionCancelled?.Invoke(currentProduction.product);
            currentProduction = null;
            currentProductionProgress = 0f;
            OnQueueChanged?.Invoke();
        }
    }

    /// <summary>
    /// Cancel all queued productions
    /// </summary>
    public void CancelQueue()
    {
        while (productionQueue.Count > 0)
        {
            var item = productionQueue.Dequeue();

            // Refund resources for queued items (gold only)
            if (resourceManager != null)
            {
                Product p = item.product;
                resourceManager.AddResources(0, 0, 0, p.GoldCost);
            }

            OnProductionCancelled?.Invoke(item.product);
        }

        OnQueueChanged?.Invoke();
        Debug.Log("Cleared production queue");
    }

    /// <summary>
    /// Start the next production in queue
    /// </summary>
    private void StartNextProduction()
    {
        if (productionQueue.Count > 0)
        {
            currentProduction = productionQueue.Dequeue();
            // Reset startTime to NOW when production actually starts
            currentProduction.startTime = Time.time;
            currentProductionProgress = 0f;
            OnProductionStarted?.Invoke(currentProduction.product);
            OnQueueChanged?.Invoke();

            Debug.Log($"Started production of {currentProduction.product.ProductName}");
        }
    }

    /// <summary>
    /// Process the current production
    /// </summary>
    private void ProcessCurrentProduction()
    {
        if (currentProduction == null) return;

        float elapsedTime = Time.time - currentProduction.startTime;
        currentProductionProgress = Mathf.Clamp01(elapsedTime / currentProduction.product.ProductionDuration);

        if (currentProductionProgress >= 1f)
        {
            CompleteProduction();
        }
    }

    /// <summary>
    /// Complete the current production and spawn the unit or start building placement
    /// </summary>
    private void CompleteProduction()
    {
        if (currentProduction == null || currentProduction.product.Prefab == null)
        {
            Debug.LogWarning("Cannot complete production: invalid product or prefab");
            currentProduction = null;
            return;
        }

        Product completedProduct = currentProduction.product;

        // Handle building placement
        if (completedProduct.IsBuilding)
        {
            // Check if BuildingPlacement system exists
            if (buildingPlacement == null)
            {
                Debug.LogError($"Cannot place building {completedProduct.ProductName} - No BuildingPlacement system found in scene!");
                currentProduction = null;
                currentProductionProgress = 0f;
                return;
            }

            // Check if this is an AI building (by checking team)
            TeamComponent team = GetComponent<TeamComponent>();
            bool isAI = team != null && team.CurrentTeam != Team.Player;

            if (isAI)
            {
                // AI: Place automatically near headquarters
                Debug.Log($"AI Building {completedProduct.ProductName} - placing automatically");

                // Get our team to pass to placement
                TeamComponent producerTeam = GetComponent<TeamComponent>();
                Team buildingTeam = producerTeam != null ? producerTeam.CurrentTeam : Team.Enemy;

                bool success = buildingPlacement.PlaceBuildingAutomatic(
                   completedProduct,
                       transform.position,
                 resourceManager,
                 buildingTeam, // Pass the team!
                    50f); // INCREASED search radius from 30f to 50f!

                if (success)
                {
                    Debug.Log($"✓ AI successfully placed {completedProduct.ProductName}");
                    OnProductionCompleted?.Invoke(completedProduct, null);
                }
                else
                {
                    Debug.LogWarning($"AI failed to place {completedProduct.ProductName} - no valid position found");
                    // Refund resources since placement failed (gold only)
                    if (resourceManager != null)
                    {
                        resourceManager.AddResources(0, 0, 0, completedProduct.GoldCost);
                    }
                }
            }
            else
            {
                // Player: Manual placement
                Debug.Log($"Building {completedProduct.ProductName} ready for placement - calling StartPlacement()");
                Debug.Log($"  Product: {completedProduct.ProductName}");
    Debug.Log($"  ResourceManager: {(resourceManager != null ? resourceManager.gameObject.name : "NULL")}");
        Debug.Log($"  BuildingPlacement: {(buildingPlacement != null ? buildingPlacement.gameObject.name : "NULL")}");
        
    buildingPlacement.StartPlacement(completedProduct, resourceManager);
    
      Debug.Log($"  StartPlacement() returned");
                
                OnProductionCompleted?.Invoke(completedProduct, null);
            }
        }
        // Handle normal unit spawning
        else
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
            GameObject spawnedUnit = Instantiate(completedProduct.Prefab, spawnPosition, Quaternion.identity);

            Debug.Log($"Completed production of {completedProduct.ProductName}");

            // Set team for spawned unit FIRST (before movement)
            TeamComponent unitTeam = spawnedUnit.GetComponent<TeamComponent>();
            TeamComponent producerTeam = GetComponent<TeamComponent>();
            if (unitTeam != null && producerTeam != null)
            {
                unitTeam.SetTeam(producerTeam.CurrentTeam);
            }

            // Move unit to rally point if it's controllable
            Controllable controllable = spawnedUnit.GetComponent<Controllable>();
            if (controllable != null && rallyPoint != null)
            {
                controllable.MoveTo(rallyPoint.position);
            }

            OnProductionCompleted?.Invoke(completedProduct, spawnedUnit);
        }

        // Clear current production AFTER everything is done
   // IMPORTANT: Don't clear completedProduct reference before this!
        currentProduction = null;
        currentProductionProgress = 0f;

        // Start next in queue
        if (productionQueue.Count > 0)
        {
            currentProduction = productionQueue.Dequeue();
            OnProductionStarted?.Invoke(currentProduction.product);
            OnQueueChanged?.Invoke();
        }
    }

    /// <summary>
    /// Set the rally point position
    /// </summary>
    public void SetRallyPoint(Vector3 position)
    {
        if (rallyPoint != null)
        {
            rallyPoint.position = position;
        }
    }

    /// <summary>
    /// Get the queue as a list for UI display
    /// </summary>
    public List<Product> GetQueuedProducts()
    {
        List<Product> products = new List<Product>();

        if (currentProduction != null)
        {
            products.Add(currentProduction.product);
        }

        foreach (var item in productionQueue)
        {
            products.Add(item.product);
        }

        return products;
    }

    void OnDrawGizmosSelected()
    {
        if (!showRallyPointGizmo) return;

        // Draw spawn point
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }

        // Draw rally point
        if (rallyPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rallyPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, rallyPoint.position);

            // Draw flag icon
            Vector3 flagBase = rallyPoint.position;
            Vector3 flagTop = flagBase + Vector3.up * 2f;
            Gizmos.DrawLine(flagBase, flagTop);
        }
    }
}
