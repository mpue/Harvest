using UnityEngine;
using System.Collections;

/// <summary>
/// Handles building placement preview and validation
/// </summary>
public class BuildingPlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private LayerMask groundLayer = -1; // Default: all layers
    [SerializeField] private float placementHeight = 0.1f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private Camera targetCamera;

    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

    [Header("Collision Check")]
    [SerializeField] private float collisionCheckRadius = 12f; // MASSIVELY INCREASED!
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float minDistanceFromHQ = 20f; // MASSIVELY INCREASED!

    [Header("UI Feedback")]
    [SerializeField] private bool showPlacementUI = true;

    [Header("Grid")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private PlacementGrid placementGrid;
    [SerializeField] private bool autoCreateGrid = true;

    [Header("Audio")]
    [SerializeField] private AudioClip placementStartSound;
    [SerializeField] private AudioClip placementSuccessSound;
    [SerializeField] private AudioClip placementCancelSound;
    [SerializeField] private AudioClip placementInvalidSound;
    [SerializeField] private AudioClip rotationSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private bool playRotationSound = false;

    private GameObject currentBuildingPreview;
    private Product currentProduct;
    private ResourceManager resourceManager;
    private bool isPlacing = false;
    private bool canPlace = false;
    private float currentRotation = 0f;
    private Renderer[] previewRenderers;
    private Material[][] originalMaterials;
    private Material[] previewMaterials;
    private float lastRotationSoundTime = 0f;
    private float rotationSoundCooldown = 0.1f;

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
                targetCamera = FindFirstObjectByType<Camera>();
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

        // Create default materials if not set
        if (validPlacementMaterial == null)
        {
            validPlacementMaterial = CreateTransparentMaterial(validColor);
        }

        if (invalidPlacementMaterial == null)
        {
            invalidPlacementMaterial = CreateTransparentMaterial(invalidColor);
        }

        // Setup grid
        if (autoCreateGrid && placementGrid == null)
        {
            GameObject gridObj = new GameObject("PlacementGrid");
            gridObj.transform.SetParent(transform);
            placementGrid = gridObj.AddComponent<PlacementGrid>();
        }

        if (placementGrid != null)
        {
            placementGrid.SetGridSize(gridSize);
            placementGrid.Hide(); // Start hidden
        }

        Debug.Log("BuildingPlacement: Initialized successfully");
    }

    void Update()
    {
        // Add safety check
        if (!isPlacing || currentBuildingPreview == null)
        {
            return;
        }

        // DEBUG: Check if currentProduct became null!
        if (currentProduct == null)
        {
            Debug.LogError($"CRITICAL: currentProduct is NULL during Update! isPlacing={isPlacing}, preview exists={currentBuildingPreview != null}");
            // Force cancel to prevent further errors
            CancelPlacement();
            return;
        }

        // Camera safety check
        if (targetCamera == null)
        {
            Debug.LogWarning("BuildingPlacement: Camera is null during placement! Trying to find camera...");
            targetCamera = Camera.main ?? FindFirstObjectByType<Camera>();
            if (targetCamera == null)
            {
                Debug.LogError("BuildingPlacement: Still no camera found! Cancelling placement.");
                CancelPlacement();
                return;
            }
        }

        UpdateBuildingPreview();
        HandleRotation();

        // Place on left click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"LEFT CLICK: canPlace={canPlace}, currentProduct={(currentProduct != null ? currentProduct.ProductName : "NULL")}");

            if (canPlace)
            {
                PlaceBuilding();
            }
            else
            {
                Debug.LogWarning("Cannot place building here!");
                PlaySound(placementInvalidSound);
            }
        }

        // Cancel on right click or ESC
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Cancelling placement (user input)");
            CancelPlacement();
        }
    }
    void OnGUI()
    {
        if (isPlacing && showPlacementUI && currentProduct != null)
        {
            // Show placement instructions
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 16;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = canPlace ? Color.green : Color.red;

            string instructions = $"Placing: {currentProduct.ProductName}\n";
            instructions += canPlace ? "Left Click to Place\n" : "Invalid Location!\n";
            instructions += "Q/E to Rotate | Right Click/ESC to Cancel";

            GUI.Box(new Rect(Screen.width / 2 - 200, 20, 400, 80), instructions, style);
        }

        // DEBUG OVERLAY for Runtime builds (helps diagnose issues!)
#if !UNITY_EDITOR
        if (Input.GetKey(KeyCode.F1)) // Press F1 to show debug info
   {
        GUIStyle debugStyle = new GUIStyle(GUI.skin.box);
 debugStyle.fontSize = 12;
debugStyle.normal.textColor = Color.yellow;
     debugStyle.alignment = TextAnchor.UpperLeft;
        
     string debugInfo = "=== BUILDING PLACEMENT DEBUG (F1) ===\n";
            debugInfo += $"Camera: {(targetCamera != null ? targetCamera.name : "NULL")}\n";
            debugInfo += $"IsPlacing: {isPlacing}\n";
    debugInfo += $"Preview Active: {(currentBuildingPreview != null)}\n";
         debugInfo += $"Can Place: {canPlace}\n";
          debugInfo += $"Mouse Pos: {Input.mousePosition}\n";
    debugInfo += $"Ground Layer: {groundLayer.value}\n";
            debugInfo += $"Current Product: {(currentProduct != null ? currentProduct.ProductName : "NULL")}\n";
  
      if (Input.GetMouseButton(0)) debugInfo += ">>> LEFT CLICK DETECTED <<<\n";
 if (Input.GetMouseButton(1)) debugInfo += ">>> RIGHT CLICK DETECTED <<<\n";
            
   GUI.Box(new Rect(10, 100, 350, 220), debugInfo, debugStyle);
            
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 10;
      hintStyle.normal.textColor = Color.gray;
         GUI.Label(new Rect(10, 330, 300, 20), "Hold F1 to show debug info", hintStyle);
        }
#endif
    }

    /// <summary>
    /// Start placing a building
    /// </summary>
    public void StartPlacement(Product product, ResourceManager manager)
    {
        Debug.Log($"=== StartPlacement called === Product: {(product != null ? product.ProductName : "NULL")}, Manager: {(manager != null ? manager.gameObject.name : "NULL")}");

        if (product == null || !product.IsBuilding || product.Prefab == null)
        {
            Debug.LogError($"Cannot start placement: Product={product != null}, IsBuilding={product?.IsBuilding}, Prefab={product?.Prefab != null}");
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError("No camera found for building placement!");
            return;
        }

        // Cancel any existing placement
        if (isPlacing)
        {
            Debug.Log("Cancelling existing placement before starting new one");
            CancelPlacement();
        }

        currentProduct = product;
        resourceManager = manager;

        Debug.Log($"? Set currentProduct to: {currentProduct.ProductName}");
        Debug.Log($"? Set resourceManager to: {resourceManager.gameObject.name}");

        // Create preview
        currentBuildingPreview = Instantiate(product.Prefab);
        currentRotation = 0f;
        currentBuildingPreview.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        Debug.Log($"? Created preview: {currentBuildingPreview.name}");

        // Disable all components on preview
        DisableComponentsOnPreview(currentBuildingPreview);

        // Get all renderers for material swapping
        previewRenderers = currentBuildingPreview.GetComponentsInChildren<Renderer>();
        StoreOriginalMaterials();

        // Create preview materials
        CreatePreviewMaterials();

        // Apply initial material
        UpdatePreviewMaterial(false);

        isPlacing = true;

        // Show grid
        if (showGrid && placementGrid != null)
        {
            placementGrid.Show();
        }

        // Play start sound
        PlaySound(placementStartSound);

        Debug.Log($"? Started placing {product.ProductName}. isPlacing={isPlacing}, currentProduct={currentProduct != null}");
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

        // SET TEAM FIRST! (Use reflection to set before Awake runs)
        TeamComponent teamComp = building.GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            // Use reflection to set team BEFORE Awake runs
            var teamField = typeof(TeamComponent).GetField("team",
 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (teamField != null)
            {
                teamField.SetValue(teamComp, buildingTeam);
            }
            teamComp.SetTeam(buildingTeam);
            Debug.Log($"? Set building team to {buildingTeam} (via reflection)");
        }
        else
        {
            Debug.LogWarning($"Building {product.ProductName} has no TeamComponent!");
        }

        // Ensure correct ResourceManager based on team
        ResourceManager correctManager = manager;
        if (manager == null || manager.gameObject.name.Contains("AI") != (buildingTeam != Team.Player))
        {
            // Find correct manager for this team
            ResourceManager[] allManagers = FindObjectsOfType<ResourceManager>();
            foreach (var mgr in allManagers)
            {
                bool isAIManager = mgr.gameObject.name.Contains("AI");
                bool needsAIManager = buildingTeam != Team.Player;
                
                if (isAIManager == needsAIManager)
                {
                    correctManager = mgr;
                    Debug.Log($"? Corrected ResourceManager to: {correctManager.gameObject.name} for team {buildingTeam}");
                    break;
                }
            }
        }

        // Pre-assign ResourceManager to ProductionComponent
        ProductionComponent prodComp = building.GetComponent<ProductionComponent>();
        if (prodComp != null && correctManager != null)
        {
            var field = typeof(ProductionComponent).GetField("resourceManager",
 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(prodComp, correctManager);
                Debug.Log($"? Pre-assigned ResourceManager to ProductionComponent");
            }
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

        Debug.Log($"Placed {product.ProductName} at {validPosition} for team {buildingTeam}");

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
        BuildingComponent[] allBuildings = FindObjectsByType<BuildingComponent>(FindObjectsSortMode.None);
        foreach (var building in allBuildings)
        {
            // Skip if it's the preview
            if (currentBuildingPreview != null && building.gameObject == currentBuildingPreview)
                continue;

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
                if (currentBuildingPreview == null ||
                         (collider.gameObject != currentBuildingPreview &&
              !collider.transform.IsChildOf(currentBuildingPreview.transform)))
                {
                    if (!collider.isTrigger)
                    {
                        Debug.Log($"Position {position} invalid: Physics collision with {collider.gameObject.name}");
                        return false;
                    }
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
    /// Update building preview position
    /// </summary>
    private void UpdateBuildingPreview()
    {
        // Safety check
        if (targetCamera == null)
        {
            Debug.LogError("BuildingPlacement.UpdateBuildingPreview: Camera is null!");
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        // DEBUG: Visualize raycast in Scene View (only visible during Play in Editor)
#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 0.016f);
#endif

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            // DEBUG: Visualize hit point
#if UNITY_EDITOR
            Debug.DrawLine(hit.point, hit.point + Vector3.up * 2f, Color.green, 0.016f);
#endif

            Vector3 position = hit.point;
            position.y += placementHeight;

            // Snap to grid if enabled
            if (snapToGrid)
            {
                position.x = Mathf.Round(position.x / gridSize) * gridSize;
                position.z = Mathf.Round(position.z / gridSize) * gridSize;
            }

            currentBuildingPreview.transform.position = position;

            // Update grid position
            if (showGrid && placementGrid != null)
            {
                placementGrid.SetPosition(position);
            }

            // Check if placement is valid
            canPlace = IsValidPlacement(position);
            UpdatePreviewMaterial(canPlace);
        }
        else
        {
            // No ground hit - show as invalid
            canPlace = false;
            UpdatePreviewMaterial(false);

            // DEBUG: Log raycast misses in builds
#if !UNITY_EDITOR
      if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
  {
          Debug.LogWarning($"BuildingPlacement: Raycast missing ground! Ray origin: {ray.origin}, direction: {ray.direction}, groundLayer: {groundLayer.value}");
         }
#endif
        }
    }

    /// <summary>
    /// Handle building rotation
    /// </summary>
    private void HandleRotation()
    {
        float rotationInput = 0f;
        bool rotated = false;

        if (Input.GetKey(KeyCode.Q))
        {
            rotationInput = -rotationSpeed * Time.deltaTime;
            rotated = true;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotationInput = rotationSpeed * Time.deltaTime;
            rotated = true;
        }

        if (rotationInput != 0f)
        {
            currentRotation += rotationInput;
            currentBuildingPreview.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

            // Play rotation sound (with cooldown to avoid spam)
            if (playRotationSound && Time.time - lastRotationSoundTime > rotationSoundCooldown)
            {
                PlaySound(rotationSound, 0.3f); // Quieter rotation sound
                lastRotationSoundTime = Time.time;
            }
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
            if (collider.gameObject != currentBuildingPreview &&
                !collider.transform.IsChildOf(currentBuildingPreview.transform))
            {
                return false;
            }
        }

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
                PlaySound(placementInvalidSound);
                return;
            }
        }

        // Create the actual building
        GameObject building = Instantiate(
            currentProduct.Prefab,
            currentBuildingPreview.transform.position,
            currentBuildingPreview.transform.rotation
        );

        // SET TEAM FIRST! Player buildings need team too!
        TeamComponent teamComp = building.GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            // Use reflection to set team BEFORE Awake runs
            var teamField = typeof(TeamComponent).GetField("team",
 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (teamField != null)
            {
                teamField.SetValue(teamComp, Team.Player);
            }
            teamComp.SetTeam(Team.Player);
            Debug.Log($"? Set player building team to Team.Player");
        }

        // Ensure correct ResourceManager is assigned (should be Player's)
        if (resourceManager != null)
        {
            ProductionComponent prodComp = building.GetComponent<ProductionComponent>();
            if (prodComp != null)
            {
                var field = typeof(ProductionComponent).GetField("resourceManager",
 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(prodComp, resourceManager);
                    Debug.Log($"? Pre-assigned Player ResourceManager to building ProductionComponent");
                }
            }
        }

        // Add BuildingComponent if it doesn't exist
        BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
        if (buildingComp == null)
        {
            buildingComp = building.AddComponent<BuildingComponent>();
        }
        buildingComp.Initialize(currentProduct, resourceManager);

        // Play success sound
        PlaySound(placementSuccessSound);

        Debug.Log($"? Placed {currentProduct.ProductName} at {building.transform.position}");

        // Cleanup and exit placement mode
        CancelPlacement();
    }

    /// <summary>
    /// Cancel building placement
    /// </summary>
    public void CancelPlacement()
    {
        Debug.Log($"=== CancelPlacement called === isPlacing={isPlacing}, preview={(currentBuildingPreview != null)}, product={(currentProduct != null ? currentProduct.ProductName : "NULL")}");

        if (currentBuildingPreview != null)
        {
            Destroy(currentBuildingPreview);
            Debug.Log("  ? Destroyed preview");
        }

        // Hide grid
        if (placementGrid != null)
        {
            placementGrid.Hide();
            Debug.Log("  ? Hid grid");
        }

        // Play cancel sound only if we were actually placing
        if (isPlacing)
        {
            PlaySound(placementCancelSound);
        }

        // IMPORTANT: Clear these in correct order to avoid null refs
        GameObject oldPreview = currentBuildingPreview;
        Product oldProduct = currentProduct;

        currentBuildingPreview = null;
        currentProduct = null;
        resourceManager = null;
        isPlacing = false;
        canPlace = false;
        originalMaterials = null;
        previewRenderers = null;
        previewMaterials = null;

        Debug.Log($"? Placement cancelled. Was placing: {oldProduct?.ProductName ?? "nothing"}");
    }

    /// <summary>
    /// Play a placement sound
    /// </summary>
    private void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        float finalVolume = soundVolume * volumeMultiplier;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot2D(clip, AudioManager.AudioCategory.UI, finalVolume);
        }
        else
        {
            // Fallback if no AudioManager exists
            AudioSource.PlayClipAtPoint(clip, targetCamera.transform.position, finalVolume);
        }
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
    /// Create preview materials with transparency
    /// </summary>
    private void CreatePreviewMaterials()
    {
        if (previewRenderers == null || previewRenderers.Length == 0) return;

        previewMaterials = new Material[previewRenderers.Length];
        for (int i = 0; i < previewRenderers.Length; i++)
        {
            // Create a transparent material based on the original
            Material[] originalMats = previewRenderers[i].materials;
            if (originalMats.Length > 0)
            {
                previewMaterials[i] = new Material(originalMats[0]);

                // Enable transparency
                previewMaterials[i].SetFloat("_Mode", 3); // Transparent mode
                previewMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                previewMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                previewMaterials[i].SetInt("_ZWrite", 0);
                previewMaterials[i].DisableKeyword("_ALPHATEST_ON");
                previewMaterials[i].EnableKeyword("_ALPHABLEND_ON");
                previewMaterials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                previewMaterials[i].renderQueue = 3000;
            }
        }
    }

    /// <summary>
    /// Update preview material based on placement validity
    /// </summary>
    private void UpdatePreviewMaterial(bool valid)
    {
        if (previewRenderers == null) return;

        Color targetColor = valid ? validColor : invalidColor;

        for (int i = 0; i < previewRenderers.Length; i++)
        {
            if (previewMaterials != null && i < previewMaterials.Length && previewMaterials[i] != null)
            {
                // Set color with transparency
                previewMaterials[i].color = targetColor;

                if (previewMaterials[i].HasProperty("_Color"))
                {
                    previewMaterials[i].SetColor("_Color", targetColor);
                }

                // Apply to renderer
                Material[] mats = new Material[previewRenderers[i].materials.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = previewMaterials[i];
                }
                previewRenderers[i].materials = mats;
            }
        }
    }

    /// <summary>
    /// Create a transparent material with a color
    /// </summary>
    private Material CreateTransparentMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = color;
        return mat;
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
