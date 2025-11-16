using UnityEngine;
using System.Collections;

/// <summary>
/// Handles building placement preview and validation
/// </summary>
public class BuildingPlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float placementHeight = 0.1f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool snapToGrid = true;

    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Collision Check")]
    [SerializeField] private float collisionCheckRadius = 2f;
    [SerializeField] private LayerMask obstacleLayer;

    private GameObject currentBuildingPreview;
    private Product currentProduct;
    private ResourceManager resourceManager;
    private bool isPlacing = false;
    private bool canPlace = false;
    private float currentRotation = 0f;
    private Renderer[] previewRenderers;
    private Material[][] originalMaterials;

    public bool IsPlacing => isPlacing;

    void Update()
    {
        if (isPlacing && currentBuildingPreview != null)
        {
  UpdateBuildingPreview();
    HandleRotation();

            // Place on left click
     if (Input.GetMouseButtonDown(0) && canPlace)
        {
                PlaceBuilding();
        }

   // Cancel on right click or ESC
       if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
 {
                CancelPlacement();
            }
        }
    }

    /// <summary>
    /// Start placing a building
    /// </summary>
    public void StartPlacement(Product product, ResourceManager manager)
    {
        if (product == null || !product.IsBuilding || product.Prefab == null)
   {
   Debug.LogWarning("Cannot start placement: invalid product");
       return;
        }

        currentProduct = product;
        resourceManager = manager;

      // Create preview
        currentBuildingPreview = Instantiate(product.Prefab);
   currentRotation = 0f;
   currentBuildingPreview.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        // Disable all components on preview
        DisableComponentsOnPreview(currentBuildingPreview);

        // Get all renderers for material swapping
        previewRenderers = currentBuildingPreview.GetComponentsInChildren<Renderer>();
        StoreOriginalMaterials();

        // Make preview semi-transparent
        SetPreviewTransparency(0.5f);

        isPlacing = true;
        Debug.Log($"Started placing {product.ProductName}");
    }

  /// <summary>
    /// Update building preview position
    /// </summary>
    private void UpdateBuildingPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

 if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
    Vector3 position = hit.point;
       position.y = placementHeight;

            // Snap to grid if enabled
            if (snapToGrid)
            {
    position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
            }

 currentBuildingPreview.transform.position = position;

     // Check if placement is valid
            canPlace = IsValidPlacement(position);
            UpdatePreviewMaterial(canPlace);
        }
}

    /// <summary>
    /// Handle building rotation
    /// </summary>
    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
    {
         currentRotation -= rotationSpeed * Time.deltaTime;
            currentBuildingPreview.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
     else if (Input.GetKey(KeyCode.E))
        {
        currentRotation += rotationSpeed * Time.deltaTime;
            currentBuildingPreview.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    /// <summary>
    /// Check if current position is valid for placement
    /// </summary>
    private bool IsValidPlacement(Vector3 position)
    {
        // Check for collisions with other buildings/obstacles
    Collider[] colliders = Physics.OverlapSphere(position, collisionCheckRadius, obstacleLayer);
     
   // Filter out the preview itself
  foreach (var collider in colliders)
        {
            if (collider.gameObject != currentBuildingPreview)
    {
     return false;
        }
        }

        // Check energy requirements (except for energy blocks)
        if (currentProduct.BuildingType != BuildingType.EnergyBlock)
  {
          if (resourceManager != null && !resourceManager.HasAvailableEnergy(currentProduct.EnergyCost))
       {
       return false;
            }
    }

        return true;
    }

    /// <summary>
    /// Place the building
    /// </summary>
    private void PlaceBuilding()
    {
        if (!canPlace || currentProduct == null)
        {
     return;
        }

    // Consume energy
        if (currentProduct.BuildingType != BuildingType.EnergyBlock)
        {
            if (resourceManager != null && !resourceManager.ConsumeEnergy(currentProduct.EnergyCost))
    {
   Debug.LogWarning("Not enough energy to place building");
   return;
            }
        }

        // Create the actual building
        GameObject building = Instantiate(
       currentProduct.Prefab, 
        currentBuildingPreview.transform.position,
    currentBuildingPreview.transform.rotation
        );

        // Add BuildingComponent if it doesn't exist
        BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
  if (buildingComp == null)
        {
         buildingComp = building.AddComponent<BuildingComponent>();
      }
buildingComp.Initialize(currentProduct, resourceManager);

        Debug.Log($"Placed {currentProduct.ProductName} at {building.transform.position}");

        // Cleanup and exit placement mode
      CancelPlacement();
    }

    /// <summary>
 /// Cancel building placement
    /// </summary>
    public void CancelPlacement()
    {
   if (currentBuildingPreview != null)
  {
            Destroy(currentBuildingPreview);
        }

        currentBuildingPreview = null;
    currentProduct = null;
        resourceManager = null;
        isPlacing = false;
  canPlace = false;
        originalMaterials = null;
  previewRenderers = null;

     Debug.Log("Placement cancelled");
    }

    /// <summary>
    /// Disable gameplay components on preview
    /// </summary>
    private void DisableComponentsOnPreview(GameObject preview)
    {
        // Disable all MonoBehaviours except Transform
        MonoBehaviour[] components = preview.GetComponentsInChildren<MonoBehaviour>();
   foreach (var comp in components)
        {
         comp.enabled = false;
        }

        // Disable colliders
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
       col.enabled = false;
  }

     // Keep rigidbodies kinematic
    Rigidbody[] rigidbodies = preview.GetComponentsInChildren<Rigidbody>();
     foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
 }
    }

    /// <summary>
    /// Store original materials for later restoration
    /// </summary>
    private void StoreOriginalMaterials()
    {
        if (previewRenderers == null) return;

        originalMaterials = new Material[previewRenderers.Length][];
        for (int i = 0; i < previewRenderers.Length; i++)
   {
     originalMaterials[i] = previewRenderers[i].materials;
        }
    }

    /// <summary>
    /// Update preview material based on placement validity
    /// </summary>
    private void UpdatePreviewMaterial(bool valid)
    {
  if (previewRenderers == null) return;

        Material material = valid ? validPlacementMaterial : invalidPlacementMaterial;

     if (material != null)
        {
            foreach (var renderer in previewRenderers)
            {
  Material[] mats = new Material[renderer.materials.Length];
        for (int i = 0; i < mats.Length; i++)
 {
            mats[i] = material;
  }
 renderer.materials = mats;
      }
        }
    }

    /// <summary>
    /// Set transparency on preview
    /// </summary>
    private void SetPreviewTransparency(float alpha)
    {
        if (previewRenderers == null) return;

        foreach (var renderer in previewRenderers)
    {
            foreach (var mat in renderer.materials)
   {
       if (mat.HasProperty("_Color"))
       {
        Color color = mat.color;
       color.a = alpha;
            mat.color = color;
         }
        }
   }
    }

    void OnDrawGizmosSelected()
    {
        if (isPlacing && currentBuildingPreview != null)
    {
            // Draw collision check radius
     Gizmos.color = canPlace ? Color.green : Color.red;
            Gizmos.DrawWireSphere(currentBuildingPreview.transform.position, collisionCheckRadius);
        }
    }
}
