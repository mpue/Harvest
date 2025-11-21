using UnityEngine;

/// <summary>
/// Handles automatic building placement for AI
/// </summary>
public class AIBuildingPlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private float minBuildingDistance = 50f;
    [SerializeField] private float maxBuildingDistance = 150f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] private GameObject aiHeadquarters;
    [SerializeField] private BuildingPlacementAI buildingPlacement;

    /// <summary>
    /// Place a building at a suitable location
    /// </summary>
    public bool PlaceBuilding(Product buildingProduct)
    {
        if (buildingProduct == null || !buildingProduct.IsBuilding)
        {
            return false;
        }

        if (aiHeadquarters == null)
        {
            Debug.LogWarning("AI: No headquarters set for building placement");
            return false;
        }

        Vector3 placement = FindSuitableBuildingLocation(buildingProduct);
        if (placement != Vector3.zero)
        {
            // Use building placement system
            if (buildingPlacement != null)
            {
                // Simulate placement
                GameObject building = GameObject.Instantiate(
                buildingProduct.Prefab,
                placement,
                Quaternion.identity
                );

                // Initialize building
                BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
                if (buildingComp == null)
                {
                    buildingComp = building.AddComponent<BuildingComponent>();
                }

                // find resource manager

                ResourceManager[] managers = FindObjectsByType<ResourceManager>(FindObjectsSortMode.None);

                foreach (var rm in managers)
                {
                    if (rm.name.Contains("AI"))
                    {
                        buildingComp.Initialize(buildingProduct, rm);
                        Debug.Log($"AI: Placed {buildingProduct.ProductName} at {placement}");
                        return true;
                    }
                }
                
            }
        }

        return false;
    }

    /// <summary>
    /// Find suitable location for building
    /// </summary>
    private Vector3 FindSuitableBuildingLocation(Product buildingProduct)
    {
        Vector3 hqPosition = aiHeadquarters.transform.position;

        // Different placement strategies based on building type
        switch (buildingProduct.BuildingType)
        {
            case BuildingType.DefenseTower:
                return FindDefensivePosition(hqPosition);

            case BuildingType.ResourceCollector:
                return FindResourceCollectorPosition(hqPosition);

            case BuildingType.EnergyBlock:
                return FindEnergyBlockPosition(hqPosition);

            default:
                return FindGeneralPosition(hqPosition);
        }
    }

    /// <summary>
    /// Find position for defensive building
    /// </summary>
    private Vector3 FindDefensivePosition(Vector3 center)
    {
        // Place on perimeter
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minBuildingDistance, maxBuildingDistance * 0.7f);

        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 position = center + direction * distance;

        return ValidatePosition(position);
    }

    /// <summary>
    /// Find position for resource collector
    /// </summary>
    private Vector3 FindResourceCollectorPosition(Vector3 center)
    {
        // Find nearest resource
        Collectable[] collectables = FindObjectsByType<Collectable>(FindObjectsSortMode.None);
        if (collectables.Length == 0)
        {
            return FindGeneralPosition(center);
        }

        // Find closest collectable
        Collectable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var collectable in collectables)
        {
            if (collectable.IsDepleted) continue;

            float dist = Vector3.Distance(center, collectable.transform.position);
            if (dist < nearestDist)
            {
                nearest = collectable;
                nearestDist = dist;
            }
        }

        if (nearest != null)
        {
            // Place near resource
            Vector3 direction = (nearest.transform.position - center).normalized;
            Vector3 position = nearest.transform.position - direction * 5f; // 5 meters from resource
            return ValidatePosition(position);
        }

        return FindGeneralPosition(center);
    }

    /// <summary>
    /// Find position for energy block
    /// </summary>
    private Vector3 FindEnergyBlockPosition(Vector3 center)
    {
        // Place close to HQ
        float angle = Random.Range(0f, 360f);
        float distance = minBuildingDistance + 2f;

        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 position = center + direction * distance;

        return ValidatePosition(position);
    }

    /// <summary>
    /// Find general building position
    /// </summary>
    private Vector3 FindGeneralPosition(Vector3 center)
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minBuildingDistance, maxBuildingDistance);

        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 position = center + direction * distance;

        return ValidatePosition(position);
    }

    /// <summary>
    /// Validate and adjust position
    /// </summary>
    private Vector3 ValidatePosition(Vector3 position)
    {
        // Raycast to ground
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, groundLayer))
        {
            position = hit.point;
        }

        // Check for obstacles
        if (Physics.CheckSphere(position, 2f, obstacleLayer))
        {
            // Try nearby positions
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 3f;
                Vector3 newPos = position + offset;

                if (!Physics.CheckSphere(newPos, 2f, obstacleLayer))
                {
                    return newPos;
                }
            }

            // If all positions blocked, return original
            return position;
        }

        return position;
    }

    /// <summary>
    /// Set AI headquarters
    /// </summary>
    public void SetHeadquarters(GameObject hq)
    {
        aiHeadquarters = hq;
    }
}
