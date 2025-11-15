using UnityEngine;
using UnityEngine.UI; // Add UI namespace
using Harvest.Minimap; // Add Minimap namespace

/// <summary>
/// Quick Setup Helper Script
/// Füge dieses Script zu einem leeren GameObject hinzu, um ein Test-Setup zu erstellen
/// </summary>
public class RTSSetupHelper : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool createSelectionManager = true;
    [SerializeField] private bool createTestUnits = true;
    [SerializeField] private bool createMinimap = true; // New
    [SerializeField] private int numberOfTestUnits = 5;
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private float spawnRadius = 10f;

    [Header("Prefabs (Optional)")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Material unitMaterial;

    void Start()
    {
        // Automatisches Setup beim Start (nur im Editor)
#if UNITY_EDITOR
        if (createSelectionManager && FindObjectOfType<UnitSelector>() == null)
        {
            CreateSelectionManager();
        }

        if (createTestUnits && unitPrefab == null)
        {
            CreateTestUnits();
        }

        if (createMinimap) // Minimap-Setup hinzufügen
        {
            CreateMinimapSystem();
        }
#endif
    }

    [ContextMenu("Create Selection Manager")]
    public void CreateSelectionManager()
    {
        // Prüfe ob bereits vorhanden
        if (FindObjectOfType<UnitSelector>() != null)
        {
            Debug.LogWarning("UnitSelector already exists in scene!");
            return;
        }

        // Erstelle Selection Manager GameObject
        GameObject selectionManager = new GameObject("SelectionManager");
        UnitSelector selector = selectionManager.AddComponent<UnitSelector>();

        Debug.Log("✓ Selection Manager created! Configure layers in Inspector.");
    }

    [ContextMenu("Create Test Units")]
    public void CreateTestUnits()
    {
        if (unitPrefab != null)
        {
            // Verwende Prefab
            CreateTestUnitsFromPrefab();
        }
        else
        {
            // Erstelle einfache Test-Units
            CreateSimpleTestUnits();
        }
    }

    private void CreateSimpleTestUnits()
    {
        GameObject unitsParent = new GameObject("Test Units");

        for (int i = 0; i < numberOfTestUnits; i++)
        {
            // Zufällige Position im Spawn-Bereich
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnAreaCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Erstelle Unit GameObject
            GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unit.name = $"Test Unit {i + 1}";
            unit.transform.position = spawnPos;
            unit.transform.parent = unitsParent.transform;

            // Füge Komponenten hinzu
            BaseUnit baseUnit = unit.AddComponent<BaseUnit>();
            Controllable controllable = unit.AddComponent<Controllable>();

            // Optional: Material anwenden
            if (unitMaterial != null)
            {
                unit.GetComponent<Renderer>().material = unitMaterial;
            }
            else
            {
                // Standard Material mit Farbe
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.5f, 0.8f);
                unit.GetComponent<Renderer>().material = mat;
            }

            // Erstelle einfachen Selection Indicator
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SelectionIndicator";
            indicator.transform.SetParent(unit.transform);
            indicator.transform.localPosition = new Vector3(0, -0.9f, 0);
            indicator.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);

            Material indicatorMat = new Material(Shader.Find("Standard"));
            indicatorMat.color = Color.green;
            indicatorMat.EnableKeyword("_EMISSION");
            indicatorMat.SetColor("_EmissionColor", Color.green * 0.5f);
            indicator.GetComponent<Renderer>().material = indicatorMat;

            // Entferne Collider vom Indicator
            Destroy(indicator.GetComponent<Collider>());
            indicator.SetActive(false);

            Debug.Log($"✓ Created {unit.name}");
        }

        Debug.Log($"✓ Created {numberOfTestUnits} test units!");
        Debug.Log("📝 Next steps:");
        Debug.Log("   1. Create ground plane (scale 10,1,10)");
        Debug.Log("   2. Setup layers: 'Selectable' for units, 'Ground' for terrain");
        Debug.Log("   3. Configure UnitSelector with correct layers");
        Debug.Log("   4. For NavMesh: Make ground Navigation Static and bake");
    }

    private void CreateTestUnitsFromPrefab()
    {
        GameObject unitsParent = new GameObject("Test Units");

        for (int i = 0; i < numberOfTestUnits; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnAreaCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity, unitsParent.transform);
            unit.name = $"Unit {i + 1}";
        }

        Debug.Log($"✓ Spawned {numberOfTestUnits} units from prefab!");
    }

    [ContextMenu("Create Ground Plane")]
    public void CreateGroundPlane()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10);

        // Material
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.3f, 0.3f, 0.3f);
        ground.GetComponent<Renderer>().material = groundMat;

        Debug.Log("✓ Ground plane created!");
        Debug.Log(" Don't forget to:");
        Debug.Log("   1. Assign 'Ground' layer to this object");
        Debug.Log("   2. Make it Navigation Static (for NavMesh)");
        Debug.Log("   3. Bake NavMesh: Window → AI → Navigation → Bake");
    }

    [ContextMenu("Create Minimap System")]
    public void CreateMinimapSystem()
    {
        Debug.Log("=== Creating Minimap System ===");

        // 1. Create Canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MinimapCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✓ Created Canvas");
        }

        // 2. Create Minimap Container
        GameObject minimapContainer = new GameObject("MinimapContainer");
        minimapContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = minimapContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0); // Bottom-right corner
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(1, 0);
        containerRect.anchoredPosition = new Vector2(-20, 20);
        containerRect.sizeDelta = new Vector2(250, 250);

        // Add background
        Image containerBg = minimapContainer.AddComponent<Image>();
        containerBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // 3. Create Minimap Image (for camera render texture)
        GameObject minimapImageObj = new GameObject("MinimapImage");
        minimapImageObj.transform.SetParent(minimapContainer.transform, false);

        RectTransform imageRect = minimapImageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.sizeDelta = Vector2.zero;
        imageRect.anchoredPosition = Vector2.zero;

        RawImage minimapImage = minimapImageObj.AddComponent<RawImage>();
        minimapImage.color = Color.white;

        // 4. Create Icon Container
        GameObject iconContainer = new GameObject("IconContainer");
        iconContainer.transform.SetParent(minimapContainer.transform, false);

        RectTransform iconRect = iconContainer.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        // 5. Create Camera View Indicator
        GameObject viewIndicator = new GameObject("CameraViewIndicator");
        viewIndicator.transform.SetParent(iconContainer.transform, false);

        RectTransform indicatorRect = viewIndicator.AddComponent<RectTransform>();
        indicatorRect.sizeDelta = new Vector2(50, 50);

        Image indicatorImage = viewIndicator.AddComponent<Image>();
        indicatorImage.color = new Color(1f, 1f, 1f, 0.3f);

        // 6. Create Minimap Controller
        GameObject controllerObj = new GameObject("MinimapController");
        MinimapController controller = controllerObj.AddComponent<MinimapController>();

        // Setup controller references via reflection (since fields are serialized)
        var controllerType = typeof(MinimapController);

        controllerType.GetField("minimapImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(controller, minimapImage);
        controllerType.GetField("minimapContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(controller, containerRect);
        controllerType.GetField("iconContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(controller, iconRect);
        controllerType.GetField("cameraViewIndicator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(controller, indicatorRect);
        controllerType.GetField("mainCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(controller, Camera.main);

        Debug.Log("✓ Minimap System created!");
        Debug.Log("📝 Next steps:");
        Debug.Log("   1. Select MinimapController in hierarchy");
        Debug.Log("   2. Assign missing references if needed");
        Debug.Log("   3. Adjust World Size to match your terrain");
        Debug.Log("   4. Test in Play Mode - units with TeamComponent get automatic icons!");

        return;
    }

    [ContextMenu("Add Minimap To Existing Units")]
    public void AddMinimapToExistingUnits()
    {
        TeamComponent[] allTeams = FindObjectsOfType<TeamComponent>();
        int count = 0;

        foreach (TeamComponent team in allTeams)
        {
            if (team.GetComponent<MinimapUnit>() == null)
            {
                MinimapUnit minimapUnit = team.gameObject.AddComponent<MinimapUnit>();

                // Set icon shape based on unit type
                if (team.GetComponent<BaseUnit>() != null)
                {
                    minimapUnit.SetIconShape(MinimapIcon.IconShape.Circle);
                }

                count++;
            }
        }

        Debug.Log($"✓ Added MinimapUnit to {count} units");
    }

    [ContextMenu("Full Auto Setup")]
    public void FullAutoSetup()
    {
        Debug.Log("=== Starting Full RTS Setup ===");

        CreateGroundPlane();
        CreateSelectionManager();
        CreateTestUnits();

        Debug.Log("=== Setup Complete! ===");
        Debug.Log("   Manual steps required:");
        Debug.Log("   1. Create layers: 'Selectable' (Layer 7) and 'Ground' (Layer 6)");
        Debug.Log("   2. Assign layers to units and ground");
        Debug.Log("   3. Configure UnitSelector layers");
        Debug.Log("   4. Make ground Navigation Static and bake NavMesh");
        Debug.Log("   5. Test: Left-click to select, Right-click to move");
    }

    [ContextMenu("Full Auto Setup (with Minimap)")]
    public void FullAutoSetupWithMinimap()
    {
        Debug.Log("=== Starting Full RTS Setup (with Minimap) ===");

        CreateGroundPlane();
        CreateSelectionManager();
        CreateTestUnits();
        CreateMinimapSystem();

        Debug.Log("=== Setup Complete! ===");
        Debug.Log("   Manual steps required:");
        Debug.Log("   1. Create layers: 'Selectable' (Layer 7) and 'Ground' (Layer 6)");
        Debug.Log("   2. Assign layers to units and ground");
        Debug.Log("   3. Configure UnitSelector layers");
        Debug.Log("   4. Make ground Navigation Static and bake NavMesh");
        Debug.Log("   5. Configure MinimapController world bounds");
        Debug.Log("   6. Test: Minimap should show colored unit dots!");
    }

    // Gizmos für Spawn-Bereich
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnAreaCenter, spawnRadius);

        Gizmos.color = new Color(1, 1, 0, 0.1f);
        Gizmos.DrawSphere(spawnAreaCenter, spawnRadius);
    }
}
