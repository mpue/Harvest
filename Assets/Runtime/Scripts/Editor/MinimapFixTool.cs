using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Tool to diagnose and fix minimap issues
/// </summary>
public class MinimapFixTool : EditorWindow
{
    [MenuItem("Tools/RTS/Minimap Fix & Setup")]
    public static void ShowWindow()
    {
        MinimapFixTool window = GetWindow<MinimapFixTool>("Minimap Fix");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }

    private Camera minimapCamera;
    private RenderTexture minimapRT;
    private UnityEngine.UI.RawImage minimapImage;
    private string diagnosticResult = "";
    private Vector2 scrollPos;

    void OnEnable()
    {
        Refresh();
    }

    void OnGUI()
    {
        GUILayout.Label("??? Minimap Fix & Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
       "Dieses Tool hilft bei schwarzen Minimap-Problemen.\n\n" +
            "Häufigste Ursachen:\n" +
  "• Minimap Camera rendert nur 'Minimap' Layer\n" +
            "• Terrain/Objekte sind nicht auf Minimap Layer\n" +
            "• RenderTexture fehlt oder falsch zugewiesen\n" +
"• Camera Settings sind falsch",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // Refresh button
        if (GUILayout.Button("?? Refresh Diagnostics", GUILayout.Height(30)))
        {
            Refresh();
        }

        EditorGUILayout.Space();

        // Diagnostic Results
        GUILayout.Label("?? Diagnostic Report:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.Height(300));
        EditorGUILayout.LabelField(diagnosticResult, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        GUILayout.Label("?? Quick Fixes:", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Fix 1: Create Minimap Layer
        if (!LayerExists("Minimap"))
        {
            EditorGUILayout.HelpBox("? 'Minimap' Layer existiert nicht!", MessageType.Error);
            if (GUILayout.Button("Create 'Minimap' Layer"))
            {
                CreateMinimapLayer();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("? 'Minimap' Layer existiert", MessageType.Info);
        }

        EditorGUILayout.Space();

        // Fix 2: Change Camera Culling Mask
        if (minimapCamera != null)
        {
            if (GUILayout.Button("?? Change Camera to Render All Layers (Quick Fix)"))
            {
                SetCameraToRenderAll();
            }

            if (GUILayout.Button("?? Configure Camera for Minimap Layer Only"))
            {
                SetCameraToMinimapLayer();
            }
        }

        EditorGUILayout.Space();

        // Fix 3: Add Terrain to Minimap Layer
        if (GUILayout.Button("??? Add Terrain to Minimap Layer"))
        {
            AddTerrainToMinimapLayer();
        }

        EditorGUILayout.Space();

        // Fix 4: Add All Units to Minimap Layer
        if (GUILayout.Button("?? Add All Units to Minimap Layer"))
        {
            AddAllUnitsToMinimapLayer();
        }

        EditorGUILayout.Space();

        // Fix 5: Create Render Texture
        if (minimapRT == null && minimapImage != null)
        {
            if (GUILayout.Button("?? Create & Assign RenderTexture"))
            {
                CreateRenderTexture();
            }
        }

        EditorGUILayout.Space();

        // Fix 6: Complete Setup
        if (GUILayout.Button("?? Complete Auto-Fix (Recommended)", GUILayout.Height(40)))
        {
            CompleteAutoFix();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        GUILayout.Label("Manual References:", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        minimapCamera = (Camera)EditorGUILayout.ObjectField("Minimap Camera", minimapCamera, typeof(Camera), true);
        minimapRT = (RenderTexture)EditorGUILayout.ObjectField("Render Texture", minimapRT, typeof(RenderTexture), false);
        minimapImage = (UnityEngine.UI.RawImage)EditorGUILayout.ObjectField("RawImage", minimapImage, typeof(UnityEngine.UI.RawImage), true);
        EditorGUILayout.EndVertical();
    }

    private void Refresh()
    {
        diagnosticResult = "?? MINIMAP DIAGNOSTICS\n\n";

        // Find minimap camera
        Camera[] cameras = FindObjectsOfType<Camera>();
        minimapCamera = null;
        foreach (var cam in cameras)
        {
            if (cam.name.ToLower().Contains("minimap") || cam.targetTexture != null)
            {
                minimapCamera = cam;
                break;
            }
        }

        // Check camera
        diagnosticResult += "1. Minimap Camera: ";
        if (minimapCamera != null)
        {
            diagnosticResult += $"? Found '{minimapCamera.name}'\n";
            diagnosticResult += $"   Position: {minimapCamera.transform.position}\n";
            diagnosticResult += $"   Orthographic: {minimapCamera.orthographic}\n";
            diagnosticResult += $"   Ortho Size: {minimapCamera.orthographicSize}\n";
            diagnosticResult += $"   Culling Mask: {LayerMaskToString(minimapCamera.cullingMask)}\n";
            diagnosticResult += $"   Clear Flags: {minimapCamera.clearFlags}\n";
            diagnosticResult += $"   Background: {minimapCamera.backgroundColor}\n";

            if (minimapCamera.cullingMask == 0)
            {
                diagnosticResult += "   ?? CULLING MASK IS NOTHING - CAMERA RENDERS NOTHING!\n";
            }
            else if (minimapCamera.cullingMask == LayerMask.GetMask("Minimap"))
            {
                diagnosticResult += "   ?? Camera only renders 'Minimap' layer\n";
                diagnosticResult += "   ? Objects must be on Minimap layer!\n";
            }
        }
        else
        {
            diagnosticResult += "? NOT FOUND\n";
        }

        diagnosticResult += "\n2. Render Texture: ";
        if (minimapCamera != null && minimapCamera.targetTexture != null)
        {
            minimapRT = minimapCamera.targetTexture;
            diagnosticResult += $"? Found '{minimapRT.name}'\n";
            diagnosticResult += $"   Size: {minimapRT.width}x{minimapRT.height}\n";
            diagnosticResult += $"   Format: {minimapRT.format}\n";
        }
        else
        {
            diagnosticResult += "? NOT ASSIGNED\n";
        }

        // Check RawImage
        diagnosticResult += "\n3. UI RawImage: ";
        UnityEngine.UI.RawImage[] rawImages = FindObjectsOfType<UnityEngine.UI.RawImage>();
        minimapImage = null;
        foreach (var img in rawImages)
        {
            if (img.name.ToLower().Contains("minimap"))
            {
                minimapImage = img;
                break;
            }
        }

        if (minimapImage != null)
        {
            diagnosticResult += $"? Found '{minimapImage.name}'\n";
            diagnosticResult += $"   Texture: {(minimapImage.texture != null ? minimapImage.texture.name : "NULL")}\n";

            if (minimapImage.texture == null)
            {
                diagnosticResult += "   ?? NO TEXTURE ASSIGNED!\n";
            }
        }
        else
        {
            diagnosticResult += "? NOT FOUND\n";
        }

        // Check Minimap Layer
        diagnosticResult += "\n4. 'Minimap' Layer: ";
        if (LayerExists("Minimap"))
        {
            int layerIndex = LayerMask.NameToLayer("Minimap");
            diagnosticResult += $"? Exists (Layer {layerIndex})\n";

            // Count objects on minimap layer
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int objectsOnLayer = 0;
            foreach (var obj in allObjects)
            {
                if (obj.layer == layerIndex)
                {
                    objectsOnLayer++;
                }
            }
            diagnosticResult += $"   Objects on layer: {objectsOnLayer}\n";

            if (objectsOnLayer == 0)
            {
                diagnosticResult += "   ?? NO OBJECTS ON MINIMAP LAYER!\n";
                diagnosticResult += "   ? This is why minimap is black!\n";
            }
        }
        else
        {
            diagnosticResult += "? DOES NOT EXIST\n";
            diagnosticResult += "   ? Create the layer first!\n";
        }

        // Check Terrain
        diagnosticResult += "\n5. Terrain: ";
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null)
        {
            diagnosticResult += $"? Found '{terrain.name}'\n";
            diagnosticResult += $"   Layer: {LayerMask.LayerToName(terrain.gameObject.layer)}\n";

            if (LayerExists("Minimap"))
            {
                int minimapLayer = LayerMask.NameToLayer("Minimap");
                if (terrain.gameObject.layer != minimapLayer)
                {
                    diagnosticResult += "   ?? Terrain is NOT on Minimap layer!\n";
                }
            }
        }
        else
        {
            diagnosticResult += "?? No Terrain found\n";
        }

        // Summary
        diagnosticResult += "\n?? SUMMARY:\n";
        List<string> issues = new List<string>();

        if (minimapCamera == null) issues.Add("No Minimap Camera");
        if (minimapRT == null) issues.Add("No RenderTexture");
        if (minimapImage == null) issues.Add("No UI RawImage");
        if (!LayerExists("Minimap")) issues.Add("No Minimap Layer");
        if (minimapCamera != null && minimapCamera.cullingMask == 0) issues.Add("Camera renders nothing");

        if (issues.Count == 0)
        {
            diagnosticResult += "?? Setup looks OK, but check:\n";
            diagnosticResult += "  • Are objects on Minimap layer?\n";
            diagnosticResult += "  • Is Camera culling mask correct?\n";
            diagnosticResult += "  • Is RenderTexture assigned to RawImage?\n";
        }
        else
        {
            diagnosticResult += $"? {issues.Count} issue(s) found:\n";
            foreach (var issue in issues)
            {
                diagnosticResult += $"  • {issue}\n";
            }
        }

        Repaint();
    }

    private bool LayerExists(string layerName)
    {
        return LayerMask.NameToLayer(layerName) != -1;
    }

    private void CreateMinimapLayer()
    {
        EditorUtility.DisplayDialog("Create Layer",
              "Unity doesn't allow creating layers programmatically.\n\n" +
                "Please create the layer manually:\n\n" +
                "1. Edit > Project Settings > Tags and Layers\n" +
            "2. Find an empty Layer slot\n" +
                "3. Name it 'Minimap'\n" +
              "4. Click this button again after creating",
                "OK");
    }

    private void SetCameraToRenderAll()
    {
        if (minimapCamera == null)
        {
            Debug.LogWarning("No minimap camera found!");
            return;
        }

        Undo.RecordObject(minimapCamera, "Change Camera Culling Mask");
        minimapCamera.cullingMask = -1; // Render everything
        EditorUtility.SetDirty(minimapCamera);

        Debug.Log("? Minimap Camera now renders ALL layers");
        Refresh();
    }

    private void SetCameraToMinimapLayer()
    {
        if (minimapCamera == null)
        {
            Debug.LogWarning("No minimap camera found!");
            return;
        }

        if (!LayerExists("Minimap"))
        {
            EditorUtility.DisplayDialog("Layer Missing",
                "The 'Minimap' layer doesn't exist!\n\n" +
               "Create it first using Edit > Project Settings > Tags and Layers",
              "OK");
            return;
        }

        Undo.RecordObject(minimapCamera, "Change Camera Culling Mask");
        minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
        EditorUtility.SetDirty(minimapCamera);

        Debug.Log("? Minimap Camera now renders only 'Minimap' layer");
        Refresh();
    }

    private void AddTerrainToMinimapLayer()
    {
        if (!LayerExists("Minimap"))
        {
            EditorUtility.DisplayDialog("Layer Missing", "Create 'Minimap' layer first!", "OK");
            return;
        }

        int minimapLayer = LayerMask.NameToLayer("Minimap");
        Terrain[] terrains = FindObjectsOfType<Terrain>();

        if (terrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrain", "No Terrain found in scene!", "OK");
            return;
        }

        foreach (var terrain in terrains)
        {
            Undo.RecordObject(terrain.gameObject, "Set Terrain Layer");
            terrain.gameObject.layer = minimapLayer;
            EditorUtility.SetDirty(terrain.gameObject);
        }

        Debug.Log($"? Added {terrains.Length} terrain(s) to Minimap layer");
        Refresh();
    }

    private void AddAllUnitsToMinimapLayer()
    {
        if (!LayerExists("Minimap"))
        {
            EditorUtility.DisplayDialog("Layer Missing", "Create 'Minimap' layer first!", "OK");
            return;
        }

        int minimapLayer = LayerMask.NameToLayer("Minimap");
        BaseUnit[] units = FindObjectsOfType<BaseUnit>();

        if (units.Length == 0)
        {
            EditorUtility.DisplayDialog("No Units", "No BaseUnits found in scene!", "OK");
            return;
        }

        int count = 0;
        foreach (var unit in units)
        {
            Undo.RecordObject(unit.gameObject, "Set Unit Layer");
            unit.gameObject.layer = minimapLayer;
            EditorUtility.SetDirty(unit.gameObject);
            count++;
        }

        Debug.Log($"? Added {count} unit(s) to Minimap layer");
        Refresh();
    }

    private void CreateRenderTexture()
    {
        if (minimapCamera == null || minimapImage == null)
        {
            Debug.LogWarning("Need both Camera and RawImage!");
            return;
        }

        // Create RenderTexture
        RenderTexture rt = new RenderTexture(512, 512, 16);
        rt.name = "MinimapRT";

        // Save as asset
        string path = "Assets/MinimapRT.renderTexture";
        AssetDatabase.CreateAsset(rt, path);
        AssetDatabase.SaveAssets();

        // Assign to camera
        Undo.RecordObject(minimapCamera, "Assign RenderTexture");
        minimapCamera.targetTexture = rt;
        EditorUtility.SetDirty(minimapCamera);

        // Assign to RawImage
        Undo.RecordObject(minimapImage, "Assign Texture");
        minimapImage.texture = rt;
        EditorUtility.SetDirty(minimapImage);

        minimapRT = rt;

        Debug.Log("? Created and assigned RenderTexture");
        Refresh();
    }

    private void CompleteAutoFix()
    {
        Debug.Log("=== Starting Complete Minimap Auto-Fix ===");

        // Step 1: Check/Create Layer
        if (!LayerExists("Minimap"))
        {
            EditorUtility.DisplayDialog("Manual Step Required",
             "Please create 'Minimap' layer:\n\n" +
           "1. Edit > Project Settings > Tags and Layers\n" +
           "2. Add 'Minimap' layer\n" +
          "3. Run Auto-Fix again",
            "OK");
            return;
        }

        // Step 2: Fix Camera
        if (minimapCamera != null)
        {
            SetCameraToRenderAll(); // Quick fix: render everything
            Debug.Log("? Fixed camera culling mask");
        }

        // Step 3: Add Terrain
        AddTerrainToMinimapLayer();

        // Step 4: Add Units (optional)
        if (EditorUtility.DisplayDialog("Add Units?",
            "Do you want to add all units to Minimap layer?\n\n" +
            "(Only needed if using layer-specific rendering)",
     "Yes", "Skip"))
        {
            AddAllUnitsToMinimapLayer();
        }

        // Step 5: Create RenderTexture if needed
        if (minimapRT == null && minimapCamera != null && minimapImage != null)
        {
            CreateRenderTexture();
        }

        Refresh();

        EditorUtility.DisplayDialog("Auto-Fix Complete",
               "Minimap auto-fix completed!\n\n" +
        "Check the diagnostic report for any remaining issues.\n\n" +
               "If minimap is still black, try:\n" +
       "• Enter Play Mode\n" +
        "• Check Camera position/rotation\n" +
        "• Verify objects are visible in Scene view",
         "OK");
    }

    private string LayerMaskToString(int mask)
    {
        if (mask == -1) return "Everything";
        if (mask == 0) return "Nothing";

        List<string> layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                }
            }
        }

        return layers.Count > 0 ? string.Join(", ", layers) : "Nothing";
    }
}
