using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor window with quick fixes for common building placement UI issues
/// </summary>
public class BuildingPlacementQuickFix : EditorWindow
{
    [MenuItem("Tools/RTS/Building Placement Quick Fix")]
    public static void ShowWindow()
    {
        BuildingPlacementQuickFix window = GetWindow<BuildingPlacementQuickFix>("Placement Quick Fix");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private BuildingPlacement buildingPlacement;
    private BuildingPlacementUI buildingPlacementUI;
    private GameObject placementPanel;
    private Canvas canvas;
    private string diagnosticResult = "";

    void OnEnable()
    {
        Refresh();
    }

    void OnGUI()
    {
        GUILayout.Label("Building Placement Quick Fix", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool diagnoses and fixes common issues with Building Placement UI.\n\n" +
   "Use this if your placement panel doesn't appear when buildings are ready.",
    MessageType.Info
  );

        EditorGUILayout.Space();

        // Refresh button
        if (GUILayout.Button("🔄 Refresh Diagnostics", GUILayout.Height(30)))
        {
            Refresh();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Diagnostic Results:", EditorStyles.boldLabel);

        // Show diagnostics
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(diagnosticResult, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        GUILayout.Label("Quick Fixes:", EditorStyles.boldLabel);

        // Quick fix buttons
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Fix 1: Create missing components
        if (buildingPlacement == null || buildingPlacementUI == null || placementPanel == null)
        {
            EditorGUILayout.HelpBox("Missing components detected!", MessageType.Error);

            if (GUILayout.Button("🔧 Create Missing Components"))
            {
                FixMissingComponents();
            }
        }

        EditorGUILayout.Space();

        // Fix 2: Link components
        if (buildingPlacement != null && buildingPlacementUI != null)
        {
            if (GUILayout.Button("🔗 Link BuildingPlacementUI to BuildingPlacement"))
            {
                LinkComponents();
            }
        }

        EditorGUILayout.Space();

        // Fix 3: Assign placement panel
        if (buildingPlacementUI != null && placementPanel != null)
        {
            if (GUILayout.Button("📋 Assign Placement Panel to UI"))
            {
                AssignPlacementPanel();
            }
        }

        EditorGUILayout.Space();

        // Fix 4: Create complete setup
        if (GUILayout.Button("🚀 Create Complete Setup (Recommended)", GUILayout.Height(40)))
        {
            CreateCompleteSetup();
        }

        EditorGUILayout.Space();

        // Fix 5: Enable components
        if (GUILayout.Button("✓ Enable All Components"))
        {
            EnableAllComponents();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        GUILayout.Label("Manual References:", EditorStyles.boldLabel);

        // Manual reference fields
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        buildingPlacement = (BuildingPlacement)EditorGUILayout.ObjectField("Building Placement", buildingPlacement, typeof(BuildingPlacement), true);
        buildingPlacementUI = (BuildingPlacementUI)EditorGUILayout.ObjectField("Building Placement UI", buildingPlacementUI, typeof(BuildingPlacementUI), true);
        placementPanel = (GameObject)EditorGUILayout.ObjectField("Placement Panel", placementPanel, typeof(GameObject), true);
        canvas = (Canvas)EditorGUILayout.ObjectField("Canvas", canvas, typeof(Canvas), true);
        EditorGUILayout.EndVertical();
    }

    private void Refresh()
    {
        diagnosticResult = "";

        // Find components
        buildingPlacement = FindObjectOfType<BuildingPlacement>();
        buildingPlacementUI = FindObjectOfType<BuildingPlacementUI>();
        canvas = FindObjectOfType<Canvas>();

        // Try to find placement panel
        placementPanel = GameObject.Find("BuildingPlacementPanel");

        // Build diagnostic report
        diagnosticResult += "🔍 DIAGNOSTIC REPORT:\n\n";

        // Check BuildingPlacement
        diagnosticResult += "1. BuildingPlacement: ";
        if (buildingPlacement != null)
        {
            diagnosticResult += $"✓ Found on '{buildingPlacement.gameObject.name}'\n";
        }
        else
        {
            diagnosticResult += "❌ NOT FOUND\n";
            diagnosticResult += "   → Need to create BuildingPlacement system\n";
        }

        // Check BuildingPlacementUI
        diagnosticResult += "\n2. BuildingPlacementUI: ";
        if (buildingPlacementUI != null)
        {
            diagnosticResult += $"✓ Found on '{buildingPlacementUI.gameObject.name}'\n";

            // Check if it's enabled
            if (!buildingPlacementUI.enabled)
            {
                diagnosticResult += "   ⚠ Component is DISABLED\n";
            }

            // Check reference to BuildingPlacement
            var field = typeof(BuildingPlacementUI).GetField("buildingPlacement",
       System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var reference = field.GetValue(buildingPlacementUI) as BuildingPlacement;
                if (reference == null)
                {
                    diagnosticResult += "   ⚠ Not linked to BuildingPlacement\n";
                }
            }
        }
        else
        {
            diagnosticResult += "❌ NOT FOUND\n";
            diagnosticResult += "   → Need to create BuildingPlacementUI\n";
        }

        // Check Placement Panel
        diagnosticResult += "\n3. Placement Panel: ";
        if (placementPanel != null)
        {
            diagnosticResult += $"✓ Found '{placementPanel.name}'\n";
            diagnosticResult += $"   Active: {placementPanel.activeSelf}\n";

            if (buildingPlacementUI != null)
            {
                var field = typeof(BuildingPlacementUI).GetField("placementPanel",
             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var assignedPanel = field.GetValue(buildingPlacementUI) as GameObject;
                    if (assignedPanel == null)
                    {
                        diagnosticResult += "   ⚠ Not assigned to BuildingPlacementUI\n";
                    }
                    else if (assignedPanel != placementPanel)
                    {
                        diagnosticResult += "   ⚠ Different panel assigned to UI\n";
                    }
                }
            }
        }
        else
        {
            diagnosticResult += "❌ NOT FOUND\n";
            diagnosticResult += "   → Need to create Placement Panel UI\n";
        }

        // Check Canvas
        diagnosticResult += "\n4. Canvas: ";
        if (canvas != null)
        {
            diagnosticResult += $"✓ Found '{canvas.gameObject.name}'\n";
        }
        else
        {
            diagnosticResult += "❌ NOT FOUND\n";
            diagnosticResult += "   → Need to create Canvas\n";
        }

        // Summary
        diagnosticResult += "\n📊 SUMMARY:\n";
        int issueCount = 0;
        if (buildingPlacement == null) issueCount++;
        if (buildingPlacementUI == null) issueCount++;
        if (placementPanel == null) issueCount++;
        if (canvas == null) issueCount++;

        if (issueCount == 0)
        {
            diagnosticResult += "✓ All components found!\n";
            diagnosticResult += "If panel still doesn't show, check:\n";
            diagnosticResult += "  - Console for errors\n";
            diagnosticResult += "  - Product.IsBuilding = true\n";
            diagnosticResult += "  - BuildingPlacement.StartPlacement() is called\n";
        }
        else
        {
            diagnosticResult += $"❌ {issueCount} issue(s) found\n";
            diagnosticResult += "Use the quick fix buttons below!\n";
        }

        Repaint();
    }

    private void FixMissingComponents()
    {
        Debug.Log("=== Creating Missing Components ===");

        // Create BuildingPlacement if missing
        if (buildingPlacement == null)
        {
            GameObject placementSystem = new GameObject("BuildingPlacementSystem");
            buildingPlacement = placementSystem.AddComponent<BuildingPlacement>();
            placementSystem.AddComponent<BuildingPlacementDebug>();
            Undo.RegisterCreatedObjectUndo(placementSystem, "Create BuildingPlacement System");
            Debug.Log("✓ Created BuildingPlacement System");
        }

        // Create Canvas if missing
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Debug.Log("✓ Created Canvas");

            // Create EventSystem if needed
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
            }
        }

        // Create Placement Panel if missing
        if (placementPanel == null && canvas != null)
        {
            // Use the setup tool to create the panel
            Debug.Log("⚠ Placement Panel missing - Use Tools > RTS > Setup Building Placement UI");
            EditorUtility.DisplayDialog("Create Placement Panel",
   "Placement Panel is missing.\n\n" +
         "Please use:\n" +
         "Tools > RTS > Setup Building Placement UI\n\n" +
         "And click 'Create Placement UI'",
       "OK");
        }

        Refresh();
    }

    private void LinkComponents()
    {
        if (buildingPlacement == null || buildingPlacementUI == null)
        {
            Debug.LogWarning("Cannot link - components missing!");
            return;
        }

        SerializedObject so = new SerializedObject(buildingPlacementUI);
        so.FindProperty("buildingPlacement").objectReferenceValue = buildingPlacement;
        so.ApplyModifiedProperties();

        Debug.Log("✓ Linked BuildingPlacementUI to BuildingPlacement");
        Refresh();
    }

    private void AssignPlacementPanel()
    {
        if (buildingPlacementUI == null || placementPanel == null)
        {
            Debug.LogWarning("Cannot assign - components missing!");
            return;
        }

        SerializedObject so = new SerializedObject(buildingPlacementUI);
        so.FindProperty("placementPanel").objectReferenceValue = placementPanel;
        so.ApplyModifiedProperties();

        Debug.Log("✓ Assigned Placement Panel to BuildingPlacementUI");
        Refresh();
    }

    private void EnableAllComponents()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.enabled = true;
            buildingPlacement.gameObject.SetActive(true);
        }

        if (buildingPlacementUI != null)
        {
            buildingPlacementUI.enabled = true;
            buildingPlacementUI.gameObject.SetActive(true);
        }

        Debug.Log("✓ Enabled all components");
        Refresh();
    }

    private void CreateCompleteSetup()
    {
        if (EditorUtility.DisplayDialog("Create Complete Setup",
            "This will create a complete Building Placement UI setup.\n\n" +
            "This is the recommended way to fix all issues at once.\n\n" +
     "Continue?",
            "Yes", "Cancel"))
        {
            // Open the setup window
            EditorWindow.GetWindow(typeof(BuildingPlacementUISetup), false, "Building Placement UI Setup");

            EditorUtility.DisplayDialog("Next Step",
     "The Building Placement UI Setup window is now open.\n\n" +
      "Click 'Create Complete Setup (All-in-One)' to create everything automatically.",
     "OK");
        }
    }
}
