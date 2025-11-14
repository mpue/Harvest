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

    // Production queue
    private Queue<ProductionQueueItem> productionQueue = new Queue<ProductionQueueItem>();
    private ProductionQueueItem currentProduction;
    private float currentProductionProgress = 0f;

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
        // Create default spawn point if none is set
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = Vector3.forward * 3f;
            spawnPoint = spawnObj.transform;
            
            // Add SpawnPoint component for visualization
            SpawnPoint spawnPointComponent = spawnObj.AddComponent<SpawnPoint>();
        }
        else
        {
            // Ensure existing spawn point has the component
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
          
            // Add RallyPoint component for visualization
            RallyPoint rallyPointComponent = rallyObj.AddComponent<RallyPoint>();
        }
        else
        {
            // Ensure existing rally point has the component
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

     // TODO: Check resources here when resource system is implemented
 // For now, just add to queue

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
    OnProductionCancelled?.Invoke(currentProduction.product);
            
    // TODO: Refund resources when resource system is implemented
        
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
    /// Complete the current production and spawn the unit
    /// </summary>
    private void CompleteProduction()
    {
        if (currentProduction == null || currentProduction.product.Prefab == null)
        {
     Debug.LogWarning("Cannot complete production: invalid product or prefab");
      currentProduction = null;
          return;
 }

        // Spawn the unit at spawn point
  Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
        GameObject spawnedUnit = Instantiate(currentProduction.product.Prefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Completed production of {currentProduction.product.ProductName}");

        // Move unit to rally point if it's controllable
        if (rallyPoint != null)
     {
   Controllable controllable = spawnedUnit.GetComponent<Controllable>();
            if (controllable != null)
   {
        controllable.MoveTo(rallyPoint.position);
            }
        }

OnProductionCompleted?.Invoke(currentProduction.product, spawnedUnit);
      
      currentProduction = null;
  currentProductionProgress = 0f;
        OnQueueChanged?.Invoke();
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
