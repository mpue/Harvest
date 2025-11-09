using UnityEngine;

/// <summary>
/// Quick Setup Helper Script
/// Füge dieses Script zu einem leeren GameObject hinzu, um ein Test-Setup zu erstellen
/// </summary>
public class RTSSetupHelper : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool createSelectionManager = true;
    [SerializeField] private bool createTestUnits = true;
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

    // Gizmos für Spawn-Bereich
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnAreaCenter, spawnRadius);

        Gizmos.color = new Color(1, 1, 0, 0.1f);
        Gizmos.DrawSphere(spawnAreaCenter, spawnRadius);
    }
}
