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
    [SerializeField] private float collisionCheckRadius = 2f;
    [SerializeField] private LayerMask obstacleLayer;

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
        // Find camera if not set
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
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
    }

    void Update()
    {
        if (isPlacing && currentBuildingPreview != null)
        {
            UpdateBuildingPreview();
            HandleRotation();

            // Place on left click
            if (Input.GetMouseButtonDown(0))
            {
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
                CancelPlacement();
            }
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

        if (targetCamera == null)
        {
            Debug.LogError("No camera found for building placement!");
            return;
        }

        // Cancel any existing placement
        if (isPlacing)
        {
            CancelPlacement();
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

        Debug.Log($"Started placing {product.ProductName}. Use mouse to position, Q/E to rotate, Left Click to place, Right Click to cancel.");
    }

    /// <summary>
    /// Update building preview position
    /// </summary>
    private void UpdateBuildingPreview()
    {
        if (targetCamera == null) return;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
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
        if (currentBuildingPreview != null)
        {
            Destroy(currentBuildingPreview);
        }

        // Hide grid
        if (placementGrid != null)
        {
            placementGrid.Hide();
        }

        // Play cancel sound only if we were actually placing
        if (isPlacing)
        {
            PlaySound(placementCancelSound);
        }

        currentBuildingPreview = null;
        currentProduct = null;
        resourceManager = null;
        isPlacing = false;
        canPlace = false;
        originalMaterials = null;
        previewRenderers = null;
        previewMaterials = null;

        Debug.Log("Placement cancelled");
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
