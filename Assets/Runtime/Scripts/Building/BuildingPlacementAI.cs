using UnityEngine;
using System.Collections;

/// <summary>
/// Handles building placement preview and validation
/// </summary>
public class BuildingPlacementAI : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer = -1; // Default: all layers
    [SerializeField] private float placementHeight = 0.1f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private Camera targetCamera;

    [Header("Collision Check")]
    [SerializeField] private float collisionCheckRadius = 12f; // MASSIVELY INCREASED!
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float minDistanceFromHQ = 20f; // MASSIVELY INCREASED!


    private Product currentProduct;
    private ResourceManager resourceManager;
    private bool isPlacing = false;
    private bool canPlace = false;

    public bool IsPlacing => isPlacing;
    public Product CurrentProduct => currentProduct;
    public bool CanPlace => canPlace;

    void Awake()
    {
        // Find camera if not set - IMPROVED for builds!
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                // Fallback: Find by tag
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj != null)
                {
                    targetCamera = camObj.GetComponent<Camera>();
                    Debug.Log("BuildingPlacement: Found camera by tag");
                }
            }

            if (targetCamera == null)
            {
                // Last resort: Find any camera
                targetCamera = FindObjectOfType<Camera>();
                Debug.Log("BuildingPlacement: Found camera using FindObjectOfType");
            }

            if (targetCamera == null)
            {
                Debug.LogError("BuildingPlacement: NO CAMERA FOUND! Building placement will not work!");
            }
            else
            {
                Debug.Log($"BuildingPlacement: Using camera '{targetCamera.gameObject.name}'");
            }
        }


        Debug.Log("BuildingPlacement: Initialized successfully");
    }

    void Update()
    {
        // Add safety check
        if (!isPlacing)
        {
            return;
        }

        // DEBUG: Check if currentProduct became null!
        if (currentProduct == null)
        {
            // Force cancel to prevent further errors
            CancelPlacement();
            return;
        }

        // Camera safety check
        if (targetCamera == null)
        {
            Debug.LogWarning("BuildingPlacement: Camera is null during placement! Trying to find camera...");
            targetCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (targetCamera == null)
            {
                Debug.LogError("BuildingPlacement: Still no camera found! Cancelling placement.");
                CancelPlacement();
                return;
            }
        }
    }

    /// <summary>
    /// Place building automatically at a valid position (for AI)
    /// </summary>
    public bool PlaceBuildingAutomatic(Product product, Vector3 centerPosition, ResourceManager manager, Team buildingTeam, float searchRadius = 30f)
    {
        if (product == null || !product.IsBuilding || product.Prefab == null)
        {
            Debug.LogWarning("Cannot place building automatically: invalid product");
            return false;
        }

        // IMPORTANT: Set these for IsValidPlacement() to work!
        currentProduct = product;
        resourceManager = manager;

        // Try to find a valid position near the center
        Vector3 validPosition = FindValidPlacementPosition(centerPosition, searchRadius);
        if (validPosition == Vector3.zero)
        {
            Debug.LogWarning($"Could not find valid placement position for {product.ProductName} near {centerPosition}");
            // Clear temporary values
            currentProduct = null;
            resourceManager = null;
            return false;
        }

        // Instantiate the building
        GameObject building = Instantiate(product.Prefab, validPosition, Quaternion.identity);

        // SET TEAM FIRST! (Before Initialize)
        TeamComponent teamComp = building.GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            teamComp.SetTeam(buildingTeam);
            Debug.Log($"Set building team to {buildingTeam}");
        }
        else
        {
            Debug.LogWarning($"Building {product.ProductName} has no TeamComponent!");
        }

        // Initialize building
        BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
        if (buildingComp != null)
        {
            buildingComp.Initialize(product, manager);
        }

        // Consume energy for the building (except energy blocks)
        if (product.BuildingType != BuildingType.EnergyBlock)
        {
            if (manager != null)
            {
                manager.ConsumeEnergy(product.EnergyCost);
            }
        }

        Debug.Log($"AI placed {product.ProductName} at {validPosition} for team {buildingTeam}");

        // Clear temporary values
        currentProduct = null;
        resourceManager = null;

        return true;
    }

    /// <summary>
    /// Find a valid position for building placement near a center point
    /// </summary>
    private Vector3 FindValidPlacementPosition(Vector3 center, float searchRadius)
    {
        int attempts = 0;
        int maxAttempts = 100; // Increased from 50 to 100 for better chance

        Debug.Log($"FindValidPlacementPosition: Starting search near {center}, radius {searchRadius}m, groundLayer={groundLayer.value}, collisionRadius={collisionCheckRadius * 3.0f}m, minDistanceFromHQ={minDistanceFromHQ}m");

        while (attempts < maxAttempts)
        {
            // Try random positions in a circle around center
            // Use OUTER ring (not inner) to keep buildings away from center
            float minRadius = searchRadius * 0.4f; // Start at 40% of radius (not center!)
            float maxRadius = searchRadius;

            float randomRadius = Random.Range(minRadius, maxRadius);
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            Vector3 testPosition = center + new Vector3(
                    Mathf.Cos(randomAngle) * randomRadius,
             0,
               Mathf.Sin(randomAngle) * randomRadius);

            // Snap to grid if enabled
            if (snapToGrid)
            {
                testPosition.x = Mathf.Round(testPosition.x / gridSize) * gridSize;
                testPosition.z = Mathf.Round(testPosition.z / gridSize) * gridSize;
            }

            // Raycast down to find ground
            Ray ray = new Ray(testPosition + Vector3.up * 100f, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200f, groundLayer))
            {
                testPosition = hit.point;
                testPosition.y += placementHeight;

                // Check if this position is valid
                // Use VERY LARGE radius for AI placement (4x base radius!)
                if (IsValidPlacementWithRadius(testPosition, collisionCheckRadius * 4.0f))
                {
                    Debug.Log($"Found valid position at {testPosition} after {attempts + 1} attempts (distance from center: {Vector3.Distance(center, testPosition):F1}m)");
                    return testPosition;
                }
                else
                {
                    if (attempts < 5 || attempts % 20 == 0) // Log first 5 and every 20th attempt
                    {
                        Debug.Log($"Attempt {attempts + 1}: Position {testPosition} invalid (distance from center: {Vector3.Distance(center, testPosition):F1}m)");
                    }
                }
            }
            else
            {
                if (attempts < 5 || attempts % 20 == 0)
                {
                    Debug.Log($"Attempt {attempts + 1}: Raycast failed at {testPosition + Vector3.up * 100f} (no ground hit)");
                }
            }

            attempts++;
        }

        Debug.LogWarning($"Failed to find valid position after {maxAttempts} attempts!");
        return Vector3.zero; // No valid position found
    }
    /// <summary>
    /// Check if position is valid with custom collision radius (for AI)
    /// </summary>
    private bool IsValidPlacementWithRadius(Vector3 position, float radius)
    {
        // === METHOD 1: Check ALL Buildings directly (ignore layers!) ===
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (var building in allBuildings)
        {
            
            float distance = Vector3.Distance(position, building.transform.position);
            if (distance < radius)
            {
                Debug.Log($"Position {position} invalid: Too close to building {building.gameObject.name} (distance {distance:F1}m < {radius}m)");
                return false;
            }
        }

        // === METHOD 2: Check minimum distance from HQ (VERY IMPORTANT!) ===
        foreach (var building in allBuildings)
        {
            if (building.IsHeadquarter)
            {
                float distanceToHQ = Vector3.Distance(position, building.transform.position);
                if (distanceToHQ < minDistanceFromHQ)
                {
                    Debug.Log($"Position {position} invalid: Too close to HQ {building.gameObject.name} (distance {distanceToHQ:F1}m < {minDistanceFromHQ}m)");
                    return false;
                }
            }
        }

        // === METHOD 3: Check Physics OverlapSphere (if layers configured) ===
        if (obstacleLayer != 0) // Only if layer is set
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius, obstacleLayer);

            foreach (var collider in colliders)
            {
                if (!collider.isTrigger)
                {
                    Debug.Log($"Position {position} invalid: Physics collision with {collider.gameObject.name}");
                    return false;
                }
            }
        }

        // === METHOD 4: Check energy requirements ===
        if (currentProduct != null && currentProduct.BuildingType != BuildingType.EnergyBlock)
        {
            if (resourceManager != null && !resourceManager.HasAvailableEnergy(currentProduct.EnergyCost))
            {
                Debug.Log($"Position {position} invalid: Not enough energy");
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Check if current position is valid for placement
    /// </summary>
    private bool IsValidPlacement(Vector3 position)
    {
        // Check for collisions with other buildings/obstacles
        Collider[] colliders = Physics.OverlapSphere(position, collisionCheckRadius, obstacleLayer);

        // Check energy requirements (except for energy blocks)
        // IMPORTANT: Check if currentProduct is not null first!
        if (currentProduct != null && currentProduct.BuildingType != BuildingType.EnergyBlock)
        {
            if (resourceManager != null && !resourceManager.HasAvailableEnergy(currentProduct.EnergyCost))
            {
                return false;
            }
        }

        return true;
    }



    /// <summary>
    /// Cancel building placement
    /// </summary>
    public void CancelPlacement()
    {

        Product oldProduct = currentProduct;

        currentProduct = null;
        resourceManager = null;
        isPlacing = false;
        canPlace = false;

        Debug.Log($"? Placement cancelled. Was placing: {oldProduct?.ProductName ?? "nothing"}");
    }



}
