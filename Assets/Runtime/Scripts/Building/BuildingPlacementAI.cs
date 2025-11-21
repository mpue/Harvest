using System.Collections;
using UnityEngine;

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
        // nothing special here for AI placer
        Debug.Log("BuildingPlacementAI: Initialized");
    }

    void Update()
    {
        // AI placer doesn't handle user input
        if (!isPlacing) return;
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

        // Final safety check: ensure prefab won't overlap existing objects (prevents stacking on top)
        float checkRadius = 3f; // Use a reasonable default

        // Skip expensive prefab instantiation and use configured collision radius instead
        checkRadius = Mathf.Max(3f, collisionCheckRadius * 0.5f);

        // Perform overlap test ignoring triggers - check for BUILDINGS only
        BuildingComponent[] nearbyBuildings = FindObjectsByType<BuildingComponent>(FindObjectsSortMode.None);
        bool blocked = false;

        foreach (var existingBuilding in nearbyBuildings)
        {
            if (existingBuilding == null) continue;

            // Check distance to this building
            float distToBuilding = Vector3.Distance(validPosition, existingBuilding.transform.position);

            // Block if too close (use a minimum spacing)
            if (distToBuilding < checkRadius)
            {
                Debug.Log($"Placement blocked: Too close to existing building '{existingBuilding.gameObject.name}' at {existingBuilding.transform.position} (distance {distToBuilding:F1}m < {checkRadius}m)");
                blocked = true;
                break;
            }
        }

        if (blocked)
        {
            // Clear temporary values
            currentProduct = null;
            resourceManager = null;
            return false;
        }

        // Instantiate the building
        GameObject building = Instantiate(product.Prefab, validPosition, Quaternion.identity);

        // SET TEAM FIRST! (Before ANY other initialization - even before Awake components run)
        TeamComponent teamComp = building.GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            // Force-set team IMMEDIATELY using reflection to set private field, then call SetTeam
            teamComp.SetTeam(buildingTeam); // Also call SetTeam for proper initialization
            Debug.Log($"Set building team to {buildingTeam} BEFORE initialization");
        }
        else
        {
            Debug.LogWarning($"Building {product.ProductName} has no TeamComponent!");
        }

        // Now get/set the correct ResourceManager based on team
        ResourceManager correctManager = manager;
        if (correctManager == null || correctManager.gameObject.name.Contains("AI") != (buildingTeam != Team.Player))
        {
            // Wrong manager! Find the correct one for this team
            ResourceManager[] allManagers = FindObjectsByType<ResourceManager>(FindObjectsSortMode.None);
            foreach (var mgr in allManagers)
            {
                bool isAIManager = mgr.gameObject.name.Contains("AI");
                bool needsAIManager = buildingTeam != Team.Player;

                if (isAIManager == needsAIManager)
                {
                    correctManager = mgr;
                    Debug.Log($"Corrected ResourceManager to: {correctManager.gameObject.name} for team {buildingTeam}");
                    break;
                }
            }
        }

        // Force-assign correct ResourceManager to ProductionComponent BEFORE Initialize
        ProductionComponent prodComp = building.GetComponent<ProductionComponent>();
        if (prodComp != null)
        {
            prodComp.SetResourceManager(correctManager);
        }

        // Initialize building
        BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
        if (buildingComp != null)
        {
            buildingComp.Initialize(product, correctManager);
        }

        // Consume energy for the building (except energy blocks)
        if (product.BuildingType != BuildingType.EnergyBlock)
        {
            if (correctManager != null)
            {
                correctManager.ConsumeEnergy(product.EnergyCost);
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
        int maxAttempts = 50; // Reduced from 100 to make it faster but still thorough

        Debug.Log($"FindValidPlacementPosition: Starting search near {center}, radius {searchRadius}m");

        // STRATEGY: Place buildings in a SPIRAL pattern starting close to HQ, not randomly
        float angleStep = 45f; // 8 directions
        float radiusStep = 5f; // Move outward in 5m increments

        for (float currentRadius = minDistanceFromHQ; currentRadius <= searchRadius && attempts < maxAttempts; currentRadius += radiusStep)
        {
            for (float angle = 0f; angle < 360f; angle += angleStep)
            {
                attempts++;
                if (attempts >= maxAttempts) break;

                float angleRad = angle * Mathf.Deg2Rad;

                Vector3 testPosition = center + new Vector3(
                        Mathf.Cos(angleRad) * currentRadius,
                 0,
                   Mathf.Sin(angleRad) * currentRadius);

                // Snap to grid if enabled
                if (snapToGrid)
                {
                    testPosition.x = Mathf.Round(testPosition.x / gridSize) * gridSize;
                    testPosition.z = Mathf.Round(testPosition.z / gridSize) * gridSize;
                }

                // Raycast down to find ground - START FROM HIGH UP
                Ray ray = new Ray(testPosition + Vector3.up * 200f, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 400f, groundLayer))
                {
                    // Use EXACT ground position
                    testPosition = hit.point;

                    // Check if this position is valid with MODERATE radius
                    if (IsValidPlacementWithRadius(testPosition, collisionCheckRadius * 2.0f))
                    {
                        Debug.Log($"? Found valid position at {testPosition} (Y={testPosition.y:F2}) after {attempts} attempts (distance from center: {Vector3.Distance(center, testPosition):F1}m, angle: {angle}°)");
                        return testPosition;
                    }
                }
                else
                {
                    // Log raycast failures for debugging
                    if (attempts <= 5)
                    {
                        Debug.LogWarning($"Raycast MISS at attempt {attempts}: Position {testPosition}, GroundLayer mask: {groundLayer.value}");
                    }
                }
            }
        }

        Debug.LogWarning($"Failed to find valid position after {attempts} attempts using spiral pattern!");
        return Vector3.zero; // No valid position found
    }
    /// <summary>
    /// Check if position is valid with custom collision radius (for AI)
    /// </summary>
    private bool IsValidPlacementWithRadius(Vector3 position, float radius)
    {
        if (position.y != 0)
        {
            Debug.Log($"Position {position} invalid: Below or above ground level");
            return false;
        }

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
}
