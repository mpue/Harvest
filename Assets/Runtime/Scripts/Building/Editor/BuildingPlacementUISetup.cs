using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor window to automatically create a BuildingPlacement UI Panel
/// </summary>
public class BuildingPlacementUISetup : EditorWindow
{
    [Header("Settings")]
    private bool useTextMeshPro = true;
    private Color validColor = Color.green;
    private Color invalidColor = Color.red;
private string panelName = "BuildingPlacementPanel";

    [Header("References")]
    private Canvas targetCanvas;
    private BuildingPlacement buildingPlacement;

    [MenuItem("Tools/RTS/Setup Building Placement UI")]
    public static void ShowWindow()
    {
        BuildingPlacementUISetup window = GetWindow<BuildingPlacementUISetup>("Building Placement UI Setup");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    void OnEnable()
    {
      // Try to find existing references
        targetCanvas = FindObjectOfType<Canvas>();
        buildingPlacement = FindObjectOfType<BuildingPlacement>();
    }

    void OnGUI()
    {
        GUILayout.Label("Building Placement UI Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

  EditorGUILayout.HelpBox(
 "This tool creates a complete UI panel for building placement feedback.\n\n" +
            "It will create:\n" +
"• Canvas (if needed)\n" +
            "• Placement Panel with background\n" +
        "• Building name text\n" +
            "• Status text\n" +
     "• Instructions text\n" +
        "• Status icon\n" +
 "• BuildingPlacementUI component",
       MessageType.Info
     );

    EditorGUILayout.Space();

        // Settings
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        panelName = EditorGUILayout.TextField("Panel Name", panelName);
        useTextMeshPro = EditorGUILayout.Toggle("Use TextMeshPro", useTextMeshPro);
        validColor = EditorGUILayout.ColorField("Valid Color", validColor);
      invalidColor = EditorGUILayout.ColorField("Invalid Color", invalidColor);

        EditorGUILayout.Space();

    // References
        GUILayout.Label("References", EditorStyles.boldLabel);
   targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
      buildingPlacement = (BuildingPlacement)EditorGUILayout.ObjectField("Building Placement", buildingPlacement, typeof(BuildingPlacement), true);

    EditorGUILayout.Space();

 // Auto-find button
      if (GUILayout.Button("Auto-Find References"))
        {
 AutoFindReferences();
        }

        EditorGUILayout.Space();

        // Status
        if (targetCanvas == null)
   {
            EditorGUILayout.HelpBox("No Canvas found. A new Canvas will be created.", MessageType.Warning);
        }

        if (buildingPlacement == null)
        {
    EditorGUILayout.HelpBox("No BuildingPlacement found in scene. Please assign manually or create one first.", MessageType.Warning);
    }

    EditorGUILayout.Space();

        // Create button
        GUI.enabled = true;
        if (GUILayout.Button("Create Placement UI", GUILayout.Height(40)))
        {
      CreatePlacementUI();
        }
   GUI.enabled = true;

   EditorGUILayout.Space();

     // Additional tools
GUILayout.Label("Additional Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create BuildingPlacement System"))
   {
            CreateBuildingPlacementSystem();
        }

        if (GUILayout.Button("Create Complete Setup (All-in-One)"))
        {
     CreateCompleteSetup();
   }
    }

    private void AutoFindReferences()
    {
        targetCanvas = FindObjectOfType<Canvas>();
        buildingPlacement = FindObjectOfType<BuildingPlacement>();

        if (targetCanvas != null)
        {
            Debug.Log($"? Found Canvas: {targetCanvas.name}");
        }
      else
        {
   Debug.Log("? No Canvas found in scene");
        }

        if (buildingPlacement != null)
        {
            Debug.Log($"? Found BuildingPlacement: {buildingPlacement.name}");
    }
        else
        {
   Debug.Log("? No BuildingPlacement found in scene");
        }
    }

    private void CreatePlacementUI()
    {
     // Ensure we have a canvas
        if (targetCanvas == null)
        {
            targetCanvas = CreateCanvas();
        }

   // Create the UI hierarchy
        GameObject panelRoot = CreatePanelHierarchy();

        // Setup the BuildingPlacementUI component
      BuildingPlacementUI placementUI = panelRoot.GetComponent<BuildingPlacementUI>();
        if (placementUI != null && buildingPlacement != null)
        {
  // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(placementUI);
  so.FindProperty("buildingPlacement").objectReferenceValue = buildingPlacement;
            so.FindProperty("validColor").colorValue = validColor;
            so.FindProperty("invalidColor").colorValue = invalidColor;
      so.ApplyModifiedProperties();
        }

     // Select the created panel
        Selection.activeGameObject = panelRoot;
        EditorGUIUtility.PingObject(panelRoot);

        Debug.Log($"? Building Placement UI created successfully: {panelRoot.name}");
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

    CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem if needed
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
        GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
   eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        Debug.Log("? Created new Canvas");

        return canvas;
    }

    private GameObject CreatePanelHierarchy()
    {
        // Main Panel
   GameObject panelObj = new GameObject(panelName);
      panelObj.transform.SetParent(targetCanvas.transform, false);

  RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
    panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -20);
        panelRect.sizeDelta = new Vector2(400, 100);

   Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // Add BuildingPlacementUI component
        BuildingPlacementUI placementUI = panelObj.AddComponent<BuildingPlacementUI>();

        // Content Container (for padding)
        GameObject contentObj = new GameObject("Content");
   contentObj.transform.SetParent(panelObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(10, 10);
     contentRect.offsetMax = new Vector2(-10, -10);

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
     vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
  vlg.childForceExpandWidth = true;
 vlg.childForceExpandHeight = false;

  // Building Name Text
  GameObject buildingNameObj = CreateTextElement("BuildingName", contentObj.transform, "Placing: Building Name", 18, TextAlignmentOptions.Center);
   
        // Status Container (Icon + Text)
        GameObject statusContainer = new GameObject("StatusContainer");
        statusContainer.transform.SetParent(contentObj.transform, false);
 
        RectTransform statusRect = statusContainer.AddComponent<RectTransform>();
  statusRect.sizeDelta = new Vector2(0, 30);

        HorizontalLayoutGroup hlg = statusContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
     hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

   // Status Icon
        GameObject statusIconObj = new GameObject("StatusIcon");
        statusIconObj.transform.SetParent(statusContainer.transform, false);

  RectTransform iconRect = statusIconObj.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(20, 20);

Image statusIcon = statusIconObj.AddComponent<Image>();
      statusIcon.color = validColor;
        // Create a simple circle sprite
        statusIcon.sprite = CreateCircleSprite();

        LayoutElement iconLayout = statusIconObj.AddComponent<LayoutElement>();
        iconLayout.minWidth = 20;
        iconLayout.minHeight = 20;

        // Status Text
        GameObject statusTextObj = CreateTextElement("StatusText", statusContainer.transform, "Ready to Place", 16, TextAlignmentOptions.Center);
      
        // Instructions Text
        GameObject instructionsObj = CreateTextElement("Instructions", contentObj.transform, 
  "Q/E: Rotate | Right Click/ESC: Cancel", 12, TextAlignmentOptions.Center);

        // Wire up references using SerializedObject
SerializedObject so = new SerializedObject(placementUI);
        so.FindProperty("placementPanel").objectReferenceValue = panelObj;
   
        if (useTextMeshPro)
        {
            so.FindProperty("buildingNameText").objectReferenceValue = buildingNameObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("statusText").objectReferenceValue = statusTextObj.GetComponent<TextMeshProUGUI>();
            so.FindProperty("instructionsText").objectReferenceValue = instructionsObj.GetComponent<TextMeshProUGUI>();
        }
      else
        {
  so.FindProperty("buildingNameText").objectReferenceValue = buildingNameObj.GetComponent<Text>();
            so.FindProperty("statusText").objectReferenceValue = statusTextObj.GetComponent<Text>();
      so.FindProperty("instructionsText").objectReferenceValue = instructionsObj.GetComponent<Text>();
        }
        
        so.FindProperty("statusIcon").objectReferenceValue = statusIcon;
        so.ApplyModifiedProperties();

        // Start hidden
        panelObj.SetActive(false);

        Undo.RegisterCreatedObjectUndo(panelObj, "Create Placement Panel");

        return panelObj;
    }

    private GameObject CreateTextElement(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(0, fontSize + 10);

        if (useTextMeshPro)
    {
          TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
  tmp.fontSize = fontSize;
    tmp.alignment = alignment;
            tmp.color = Color.white;
        }
  else
        {
         Text uiText = textObj.AddComponent<Text>();
            uiText.text = text;
     uiText.fontSize = fontSize;
            uiText.alignment = ConvertTMPAlignment(alignment);
   uiText.color = Color.white;
         uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return textObj;
    }

    private TextAnchor ConvertTMPAlignment(TextAlignmentOptions tmpAlignment)
    {
        switch (tmpAlignment)
        {
            case TextAlignmentOptions.TopLeft: return TextAnchor.UpperLeft;
            case TextAlignmentOptions.Top: return TextAnchor.UpperCenter;
case TextAlignmentOptions.TopRight: return TextAnchor.UpperRight;
            case TextAlignmentOptions.Left: return TextAnchor.MiddleLeft;
            case TextAlignmentOptions.Center: return TextAnchor.MiddleCenter;
   case TextAlignmentOptions.Right: return TextAnchor.MiddleRight;
        case TextAlignmentOptions.BottomLeft: return TextAnchor.LowerLeft;
            case TextAlignmentOptions.Bottom: return TextAnchor.LowerCenter;
     case TextAlignmentOptions.BottomRight: return TextAnchor.LowerRight;
 default: return TextAnchor.MiddleCenter;
  }
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
 int size = 64;
     Texture2D texture = new Texture2D(size, size);
      Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
 {
   for (int x = 0; x < size; x++)
            {
 float distance = Vector2.Distance(new Vector2(x, y), center);
    if (distance <= radius)
 {
              pixels[y * size + x] = Color.white;
    }
     else
           {
         pixels[y * size + x] = Color.clear;
    }
    }
        }

     texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void CreateBuildingPlacementSystem()
    {
        // Check if already exists
        BuildingPlacement existing = FindObjectOfType<BuildingPlacement>();
     if (existing != null)
        {
            if (EditorUtility.DisplayDialog("BuildingPlacement exists", 
  $"A BuildingPlacement already exists on '{existing.gameObject.name}'.\n\nSelect it instead?", 
  "Select", "Create New"))
            {
           Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }
  }

        // Create GameObject
        GameObject systemObj = new GameObject("BuildingPlacementSystem");
  
        // Add components
        BuildingPlacement placement = systemObj.AddComponent<BuildingPlacement>();
  systemObj.AddComponent<BuildingPlacementDebug>();

        Undo.RegisterCreatedObjectUndo(systemObj, "Create Building Placement System");

        buildingPlacement = placement;

        Selection.activeGameObject = systemObj;
        EditorGUIUtility.PingObject(systemObj);

        Debug.Log("? BuildingPlacement System created successfully");
    }

    private void CreateCompleteSetup()
    {
        Debug.Log("=== Creating Complete Building Placement Setup ===");

    // 1. Create BuildingPlacement System
        if (buildingPlacement == null)
        {
  CreateBuildingPlacementSystem();
        }

        // 2. Create Canvas if needed
        if (targetCanvas == null)
        {
      targetCanvas = CreateCanvas();
      }

        // 3. Create UI Panel
        CreatePlacementUI();

  // 4. Create test ground if needed
        if (GameObject.Find("Ground") == null && GameObject.Find("Plane") == null)
        {
        if (EditorUtility.DisplayDialog("Create Test Ground?", 
                "No ground found in scene. Create a test ground plane?", 
    "Yes", "No"))
            {
          CreateTestGround();
         }
      }

        Debug.Log("=== Complete Setup Finished ===");
        EditorUtility.DisplayDialog("Setup Complete", 
 "Building Placement System setup is complete!\n\n" +
            "Created:\n" +
    "• BuildingPlacement System\n" +
     "• Canvas with UI Panel\n" +
            "• BuildingPlacementUI component\n\n" +
            "Next steps:\n" +
            "1. Create Product assets\n" +
          "2. Setup your Headquarter\n" +
 "3. Test building placement", 
   "OK");
    }

    private void CreateTestGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
    ground.transform.localScale = new Vector3(10, 1, 10);

  // Try to set ground layer
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex != -1)
        {
          ground.layer = groundLayerIndex;
        }

        Undo.RegisterCreatedObjectUndo(ground, "Create Test Ground");

        Debug.Log("? Test ground created");
    }
}
