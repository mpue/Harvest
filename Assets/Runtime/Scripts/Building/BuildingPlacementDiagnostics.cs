using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Diagnostic tool to debug building placement UI issues
/// Attach this to any GameObject to see what's going on
/// </summary>
public class BuildingPlacementDiagnostics : MonoBehaviour
{
    [Header("Auto-Find Components")]
    [SerializeField] private bool autoFind = true;

    [Header("References to Check")]
    [SerializeField] private BuildingPlacement buildingPlacement;
 [SerializeField] private BuildingPlacementUI buildingPlacementUI;
    [SerializeField] private ProductionComponent productionComponent;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameObject placementPanel;

    [Header("Display Settings")]
    [SerializeField] private bool showOnScreenDebug = true;
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private float updateInterval = 0.5f;

    private float lastUpdateTime;
    private string diagnosticInfo = "";
    private bool lastIsPlacing = false;

    void Start()
{
        if (autoFind)
  {
    FindComponents();
  }

    LogDiagnostics("=== Initial Diagnostics ===");
    }

  void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDiagnostics();
            lastUpdateTime = Time.time;
        }

        // Detect state changes
      if (buildingPlacement != null)
        {
        bool currentIsPlacing = buildingPlacement.IsPlacing;
  if (currentIsPlacing != lastIsPlacing)
   {
          LogDiagnostics($"? IsPlacing changed: {lastIsPlacing} ? {currentIsPlacing}");
 lastIsPlacing = currentIsPlacing;

         if (currentIsPlacing)
         {
       CheckPlacementUIState();
        }
     }
    }
    }

    void FindComponents()
    {
      buildingPlacement = FindObjectOfType<BuildingPlacement>();
    buildingPlacementUI = FindObjectOfType<BuildingPlacementUI>();
        productionComponent = FindObjectOfType<ProductionComponent>();
   resourceManager = FindObjectOfType<ResourceManager>();

   // Try to find placement panel
      if (buildingPlacementUI != null)
      {
            // Use reflection to get private field
         var field = typeof(BuildingPlacementUI).GetField("placementPanel", 
           System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
     if (field != null)
 {
      placementPanel = field.GetValue(buildingPlacementUI) as GameObject;
   }
        }

    LogDiagnostics("=== Auto-Find Complete ===");
    }

  void UpdateDiagnostics()
    {
        diagnosticInfo = "=== Building Placement Diagnostics ===\n\n";

        // Check BuildingPlacement
      diagnosticInfo += "?? BUILDING PLACEMENT:\n";
        if (buildingPlacement != null)
      {
            diagnosticInfo += $"  ? Found: {buildingPlacement.gameObject.name}\n";
      diagnosticInfo += $"  IsPlacing: {buildingPlacement.IsPlacing}\n";
      diagnosticInfo += $"  CanPlace: {buildingPlacement.CanPlace}\n";
     if (buildingPlacement.CurrentProduct != null)
            {
           diagnosticInfo += $"  Product: {buildingPlacement.CurrentProduct.ProductName}\n";
      }
 else
  {
        diagnosticInfo += $"  Product: None\n";
 }
        }
        else
   {
            diagnosticInfo += "  ? NOT FOUND!\n";
   }

        diagnosticInfo += "\n?? BUILDING PLACEMENT UI:\n";
    if (buildingPlacementUI != null)
    {
          diagnosticInfo += $"  ? Found: {buildingPlacementUI.gameObject.name}\n";
            diagnosticInfo += $"  Enabled: {buildingPlacementUI.enabled}\n";
            diagnosticInfo += $"  GameObject Active: {buildingPlacementUI.gameObject.activeInHierarchy}\n";
        }
        else
        {
            diagnosticInfo += "  ? NOT FOUND!\n";
        }

        diagnosticInfo += "\n?? PLACEMENT PANEL:\n";
  if (placementPanel != null)
        {
          diagnosticInfo += $"  ? Found: {placementPanel.name}\n";
            diagnosticInfo += $"  Active: {placementPanel.activeSelf}\n";
            diagnosticInfo += $"  In Hierarchy: {placementPanel.activeInHierarchy}\n";
 diagnosticInfo += $"  Parent: {(placementPanel.transform.parent != null ? placementPanel.transform.parent.name : "None")}\n";
        }
        else
    {
            diagnosticInfo += "  ? NOT FOUND!\n";
        }

diagnosticInfo += "\n?? PRODUCTION COMPONENT:\n";
        if (productionComponent != null)
  {
        diagnosticInfo += $"  ? Found: {productionComponent.gameObject.name}\n";
            diagnosticInfo += $"  IsProducing: {productionComponent.IsProducing}\n";
        diagnosticInfo += $"  Queue Count: {productionComponent.QueueCount}\n";
   if (productionComponent.CurrentProduct != null)
            {
      diagnosticInfo += $"  Current Product: {productionComponent.CurrentProduct.ProductName}\n";
                diagnosticInfo += $"  Progress: {(productionComponent.CurrentProductionProgress * 100f):F1}%\n";
       }
}
   else
   {
            diagnosticInfo += "  ? NOT FOUND (optional)\n";
        }

    diagnosticInfo += "\n?? RESOURCE MANAGER:\n";
        if (resourceManager != null)
        {
            diagnosticInfo += $"  ? Found\n";
        diagnosticInfo += $"  Energy: {resourceManager.CurrentEnergy}/{resourceManager.MaxEnergy}\n";
    diagnosticInfo += $"  Available: {resourceManager.AvailableEnergy}\n";
        }
        else
        {
  diagnosticInfo += "  ? NOT FOUND (optional)\n";
        }

        diagnosticInfo += "\n?? CANVAS:\n";
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
     {
    diagnosticInfo += $"  ? Found: {canvas.gameObject.name}\n";
            diagnosticInfo += $"  Render Mode: {canvas.renderMode}\n";
            diagnosticInfo += $"  Active: {canvas.gameObject.activeInHierarchy}\n";
 }
 else
        {
        diagnosticInfo += "  ? NOT FOUND!\n";
        }
    }

    void CheckPlacementUIState()
    {
  string info = "\n? PLACEMENT STARTED - Checking UI State:\n";
        
        if (buildingPlacementUI == null)
     {
        info += "  ? BuildingPlacementUI is NULL!\n";
            info += "  ? Create BuildingPlacementUI component\n";
        }
        else
        {
   info += $"  ? BuildingPlacementUI exists on: {buildingPlacementUI.gameObject.name}\n";
      
          if (!buildingPlacementUI.enabled)
     {
           info += "  ? BuildingPlacementUI is DISABLED!\n";
           info += "  ? Enable the component\n";
            }
            
         if (!buildingPlacementUI.gameObject.activeInHierarchy)
        {
        info += "  ? BuildingPlacementUI GameObject is INACTIVE!\n";
           info += "  ? Activate the GameObject\n";
    }
        }

        if (placementPanel == null)
        {
 info += "  ? PlacementPanel is NULL!\n";
            info += "  ? Assign placementPanel in BuildingPlacementUI\n";
        }
        else
 {
info += $"  ? PlacementPanel exists: {placementPanel.name}\n";
         info += $"  Active: {placementPanel.activeSelf}\n";
        
          if (!placementPanel.activeSelf)
  {
  info += "  ? Panel is inactive (will be activated by BuildingPlacementUI)\n";
        }
        }

  LogDiagnostics(info);
    }

    void LogDiagnostics(string message)
    {
      if (logToConsole)
        {
            Debug.Log($"[Building Placement Diagnostics]\n{message}\n{diagnosticInfo}");
        }
    }

    void OnGUI()
    {
        if (!showOnScreenDebug) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
 style.fontSize = 11;
    style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;
        style.wordWrap = false;

     // Calculate box size based on content
        float width = 450;
   float height = 400;

        GUI.Box(new Rect(10, 10, width, height), diagnosticInfo, style);

   // Instructions
        GUIStyle instructionStyle = new GUIStyle(GUI.skin.box);
        instructionStyle.fontSize = 10;
        instructionStyle.alignment = TextAnchor.UpperLeft;
      instructionStyle.normal.textColor = Color.yellow;

string instructions = "Press R to Refresh | Press L to Log to Console";
        GUI.Box(new Rect(10, height + 20, width, 40), instructions, instructionStyle);

  // Handle input
        if (Input.GetKeyDown(KeyCode.R))
        {
            FindComponents();
            UpdateDiagnostics();
        }

        if (Input.GetKeyDown(KeyCode.L))
  {
   LogDiagnostics("=== Manual Log Request ===");
        }
    }

    [ContextMenu("Force Find Components")]
    public void ForceFindComponents()
    {
        FindComponents();
     UpdateDiagnostics();
        LogDiagnostics("=== Force Find Complete ===");
    }

    [ContextMenu("Log Current State")]
    public void LogCurrentState()
    {
        UpdateDiagnostics();
        LogDiagnostics("=== Current State ===");
    }

    [ContextMenu("Fix Common Issues")]
    public void FixCommonIssues()
    {
        string fixes = "=== Attempting Fixes ===\n";

        // Find components if missing
        if (buildingPlacement == null || buildingPlacementUI == null)
        {
            FindComponents();
          fixes += "? Re-scanned for components\n";
    }

        // Enable BuildingPlacementUI if disabled
   if (buildingPlacementUI != null && !buildingPlacementUI.enabled)
        {
            buildingPlacementUI.enabled = true;
        fixes += "? Enabled BuildingPlacementUI component\n";
        }

  // Activate GameObject if inactive
if (buildingPlacementUI != null && !buildingPlacementUI.gameObject.activeInHierarchy)
        {
    buildingPlacementUI.gameObject.SetActive(true);
    fixes += "? Activated BuildingPlacementUI GameObject\n";
        }

        // Try to find and assign placement panel
     if (placementPanel == null)
        {
    placementPanel = GameObject.Find("BuildingPlacementPanel");
            if (placementPanel != null)
    {
    fixes += "? Found and assigned BuildingPlacementPanel\n";
   }
    else
       {
        fixes += "? Could not find BuildingPlacementPanel\n";
          fixes += "   ? Use Tools > RTS > Setup Building Placement UI\n";
            }
        }

        LogDiagnostics(fixes);
        UpdateDiagnostics();
    }
}
