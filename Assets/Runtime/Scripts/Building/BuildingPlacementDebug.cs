using UnityEngine;

/// <summary>
/// Helper script to debug building placement issues
/// </summary>
public class BuildingPlacementDebug : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private Camera targetCamera;

    [Header("Debug Settings")]
    [SerializeField] private bool showRaycastDebug = true;
    [SerializeField] private bool showGroundHitPoint = true;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private float debugRayLength = 1000f;

    [Header("Debug Colors")]
    [SerializeField] private Color rayHitColor = Color.green;
    [SerializeField] private Color rayMissColor = Color.red;
    [SerializeField] private float hitPointSize = 0.5f;

    private Vector3 lastHitPoint;
    private bool lastRaycastHit = false;

    void Awake()
    {
        if (buildingPlacement == null)
        {
            buildingPlacement = GetComponent<BuildingPlacement>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
    }

    void Update()
    {
        if (!showRaycastDebug || targetCamera == null) return;

        // Perform raycast from mouse position
        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        lastRaycastHit = Physics.Raycast(ray, out hit, debugRayLength, groundLayer);

        if (lastRaycastHit)
        {
            lastHitPoint = hit.point;

            if (showGroundHitPoint)
            {
                Debug.DrawLine(ray.origin, hit.point, rayHitColor);
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 2f, Color.yellow);
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * debugRayLength, rayMissColor);
        }
    }

    void OnGUI()
    {
        if (!showRaycastDebug) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 12;
        style.alignment = TextAnchor.UpperLeft;

        string debugInfo = "=== Building Placement Debug ===\n";
        debugInfo += $"Camera: {(targetCamera != null ? targetCamera.name : "NULL")}\n";
        debugInfo += $"Raycast Hit: {lastRaycastHit}\n";

        if (lastRaycastHit)
        {
            debugInfo += $"Hit Point: {lastHitPoint}\n";
            debugInfo += $"Ground Layer: {groundLayer.value}\n";
        }
        else
        {
            debugInfo += "No ground detected!\n";
            debugInfo += "Check:\n";
            debugInfo += "- Ground has collider?\n";
            debugInfo += "- Ground layer matches?\n";
            debugInfo += "- Camera is active?\n";
        }

        if (buildingPlacement != null)
        {
            debugInfo += $"Is Placing: {buildingPlacement.IsPlacing}\n";
            if (buildingPlacement.CurrentProduct != null)
            {
                debugInfo += $"Product: {buildingPlacement.CurrentProduct.ProductName}\n";
            }
        }

        GUI.Box(new Rect(10, 10, 300, 200), debugInfo, style);
    }

    void OnDrawGizmos()
    {
        if (showGroundHitPoint && lastRaycastHit)
        {
            Gizmos.color = rayHitColor;
            Gizmos.DrawWireSphere(lastHitPoint, hitPointSize);
            Gizmos.DrawLine(lastHitPoint, lastHitPoint + Vector3.up * 2f);
        }
    }

    /// <summary>
    /// Test building placement with a sample product
    /// </summary>
    [ContextMenu("Test Placement (requires Product in scene)")]
    public void TestPlacement()
    {
        if (buildingPlacement == null)
        {
            Debug.LogError("BuildingPlacement not found!");
            return;
        }

        // Try to find a product to test with
        Product testProduct = FindObjectOfType<Product>();
        if (testProduct == null)
        {
            Debug.LogWarning("No Product found in scene. Create a Product asset first.");
            return;
        }

        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogWarning("No ResourceManager found. Creating temporary one.");
            GameObject rmObj = new GameObject("TempResourceManager");
            resourceManager = rmObj.AddComponent<ResourceManager>();
        }

        buildingPlacement.StartPlacement(testProduct, resourceManager);
        Debug.Log("Started test placement. Move mouse to see raycast.");
    }

    /// <summary>
    /// Create a test ground plane
    /// </summary>
    [ContextMenu("Create Test Ground")]
    public void CreateTestGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "TestGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        // Set to ground layer if it exists
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex != -1)
        {
            ground.layer = groundLayerIndex;
            Debug.Log("Ground created on 'Ground' layer");
        }
        else
        {
            Debug.Log("Ground created on Default layer. Consider creating a 'Ground' layer.");
        }

        Debug.Log("Test ground created successfully!");
    }
}
