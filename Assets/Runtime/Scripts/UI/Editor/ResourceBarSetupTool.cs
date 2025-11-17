using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor tool to automatically create a ResourceBar UI
/// </summary>
public class ResourceBarSetupTool : EditorWindow
{
    [MenuItem("Tools/RTS/Create Resource Bar UI")]
    public static void ShowWindow()
    {
        ResourceBarSetupTool window = GetWindow<ResourceBarSetupTool>("Resource Bar Setup");
        window.minSize = new Vector2(450, 650);
        window.Show();
    }

    private Canvas targetCanvas;
    private ResourceManager resourceManager;
    private bool useTextMeshPro = true;
    private bool includeIcons = true;
    private bool includeEnergyBar = true;
    private float barHeight = 50f;
    private string goldFormat = "{0}";
    private string energyFormat = "{0}/{1}";

    void OnEnable()
    {
        targetCanvas = FindObjectOfType<Canvas>();
        resourceManager = FindObjectOfType<ResourceManager>();
    }

    void OnGUI()
    {
        GUILayout.Label("🎯 Resource Bar Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
 "Erstellt automatisch eine Resource Bar für Gold und Energie am oberen Bildschirmrand.\n\n" +
            "Features:\n" +
            "• Gold-Anzeige mit Icon\n" +
   "• Energie-Anzeige mit Fortschrittsbalken\n" +
            "• Animierte Werte-Änderungen\n" +
       "• Farb-Kodierung für Energie-Level\n" +
      "• Automatische ResourceManager-Anbindung",
     MessageType.Info
        );

        EditorGUILayout.Space();

        // Settings
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        useTextMeshPro = EditorGUILayout.Toggle("Use TextMeshPro", useTextMeshPro);
        includeIcons = EditorGUILayout.Toggle("Include Icons", includeIcons);
        includeEnergyBar = EditorGUILayout.Toggle("Include Energy Bar", includeEnergyBar);
        barHeight = EditorGUILayout.Slider("Bar Height", barHeight, 30f, 100f);

        EditorGUILayout.Space();

        goldFormat = EditorGUILayout.TextField("Gold Format", goldFormat);
        energyFormat = EditorGUILayout.TextField("Energy Format", energyFormat);

        EditorGUILayout.Space();

        // References
        GUILayout.Label("References", EditorStyles.boldLabel);
        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        resourceManager = (ResourceManager)EditorGUILayout.ObjectField("Resource Manager", resourceManager, typeof(ResourceManager), true);

        EditorGUILayout.Space();

        // Auto-find button
        if (GUILayout.Button("Auto-Find References"))
        {
            targetCanvas = FindObjectOfType<Canvas>();
            resourceManager = FindObjectOfType<ResourceManager>();
        }

        EditorGUILayout.Space();

        // Status
        if (targetCanvas == null)
        {
            EditorGUILayout.HelpBox("No Canvas found. A new Canvas will be created.", MessageType.Warning);
        }

        if (resourceManager == null)
        {
            EditorGUILayout.HelpBox("No ResourceManager found. Please create one first or it will be created automatically.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // Create button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎯 Create Resource Bar", GUILayout.Height(40)))
        {
            CreateResourceBar();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();

        // Preview
        GUILayout.Label("Preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
         "Resource Bar Layout:\n\n" +
            "[💰 Gold: 500]    [⚡ Energy: 15/30]\n" +
            "       [████████░░]",
       MessageType.None
        );
    }

    private void CreateResourceBar()
    {
        Debug.Log("=== Creating Resource Bar UI ===");

        // Ensure we have a canvas
        if (targetCanvas == null)
        {
            targetCanvas = CreateCanvas();
        }

        // Ensure we have a ResourceManager
        if (resourceManager == null)
        {
            resourceManager = CreateResourceManager();
        }

        // Create the ResourceBar hierarchy
        GameObject resourceBar = CreateResourceBarHierarchy();

        // Select the created object
        Selection.activeGameObject = resourceBar;
        EditorGUIUtility.PingObject(resourceBar);

        Debug.Log("✓ Resource Bar created successfully!");

        EditorUtility.DisplayDialog("Success",
             "Resource Bar UI created successfully!\n\n" +
        "The bar is now at the top of your screen.\n" +
      "It will automatically update when resources change.",
             "OK");
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
        Debug.Log("✓ Created new Canvas");

        return canvas;
    }

    private ResourceManager CreateResourceManager()
    {
        GameObject rmObj = new GameObject("ResourceManager");
        ResourceManager rm = rmObj.AddComponent<ResourceManager>();

        Undo.RegisterCreatedObjectUndo(rmObj, "Create ResourceManager");
        Debug.Log("✓ Created ResourceManager");

        return rm;
    }

    private GameObject CreateResourceBarHierarchy()
    {
        // Main Panel
        GameObject barObj = new GameObject("ResourceBar");
        barObj.transform.SetParent(targetCanvas.transform, false);

        RectTransform barRect = barObj.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0, barHeight);

        Image barImage = barObj.AddComponent<Image>();
        barImage.color = new Color(0f, 0f, 0f, 0.8f);

        // Add ResourceBarUI component
        ResourceBarUI resourceBarUI = barObj.AddComponent<ResourceBarUI>();

        // Content container (with padding)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(barObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(20, 5);
        contentRect.offsetMax = new Vector2(-20, -5);

        HorizontalLayoutGroup hlg = contentObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Gold Section
        GameObject goldSection = CreateResourceSection(contentObj.transform, "GoldSection", "💰", goldFormat);

        // Energy Section
        GameObject energySection = CreateEnergySection(contentObj.transform, "EnergySection", "⚡", energyFormat);

        // Wire up references using SerializedObject
        SerializedObject so = new SerializedObject(resourceBarUI);

        so.FindProperty("resourceManager").objectReferenceValue = resourceManager;
        so.FindProperty("goldFormat").stringValue = goldFormat;
        so.FindProperty("energyFormat").stringValue = energyFormat;

        // Wire up gold references
        Transform goldTextTransform = goldSection.transform.Find("GoldText");
        Transform goldIconTransform = goldSection.transform.Find("GoldIcon");

        if (useTextMeshPro && goldTextTransform != null)
        {
            so.FindProperty("goldText").objectReferenceValue = goldTextTransform.GetComponent<TextMeshProUGUI>();
        }
        else if (goldTextTransform != null)
        {
            so.FindProperty("goldText").objectReferenceValue = goldTextTransform.GetComponent<Text>();
        }

        if (goldIconTransform != null)
        {
            so.FindProperty("goldIcon").objectReferenceValue = goldIconTransform.GetComponent<Image>();
        }

        // Wire up energy references
        Transform topRow = energySection.transform.Find("TopRow");
    Transform energyTextTransform = topRow != null ? topRow.Find("EnergyText") : null;
  Transform energyIconTransform = topRow != null ? topRow.Find("EnergyIcon") : null;
        Transform energyBarBg = energySection.transform.Find("EnergyBarBackground");
        Transform energyBarFill = energyBarBg != null ? energyBarBg.Find("EnergyBarFill") : null;

        if (useTextMeshPro && energyTextTransform != null)
        {
            so.FindProperty("energyText").objectReferenceValue = energyTextTransform.GetComponent<TextMeshProUGUI>();
        }
        else if (energyTextTransform != null)
        {
            so.FindProperty("energyText").objectReferenceValue = energyTextTransform.GetComponent<Text>();
        }

        if (energyIconTransform != null)
        {
            so.FindProperty("energyIcon").objectReferenceValue = energyIconTransform.GetComponent<Image>();
        }

        if (energyBarFill != null)
        {
            so.FindProperty("energyFillBar").objectReferenceValue = energyBarFill.GetComponent<Image>();
        }

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(barObj, "Create Resource Bar");

        return barObj;
    }

    private GameObject CreateResourceSection(Transform parent, string name, string iconText, string format)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);

        RectTransform sectionRect = section.AddComponent<RectTransform>();
        sectionRect.sizeDelta = new Vector2(200, 40);

        HorizontalLayoutGroup hlg = section.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Icon
        if (includeIcons)
        {
            GameObject iconObj = new GameObject(name.Replace("Section", "Icon"));
            iconObj.transform.SetParent(section.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30, 30);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.yellow;

            LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.minWidth = 30;
            iconLayout.minHeight = 30;
        }

        // Text
        GameObject textObj = CreateTextElement(name.Replace("Section", "Text"), section.transform, "0", 20);

        return section;
    }

    private GameObject CreateEnergySection(Transform parent, string name, string iconText, string format)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);

        RectTransform sectionRect = section.AddComponent<RectTransform>();
        sectionRect.sizeDelta = new Vector2(250, 40);

        VerticalLayoutGroup vlg = section.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Top row (Icon + Text)
        GameObject topRow = new GameObject("TopRow");
        topRow.transform.SetParent(section.transform, false);

        RectTransform topRowRect = topRow.AddComponent<RectTransform>();
        topRowRect.sizeDelta = new Vector2(0, 25);

        HorizontalLayoutGroup topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
        topHlg.spacing = 10;
        topHlg.childAlignment = TextAnchor.MiddleLeft;
        topHlg.childControlWidth = false;
        topHlg.childControlHeight = true;

        // Icon
        if (includeIcons)
        {
            GameObject iconObj = new GameObject("EnergyIcon");
            iconObj.transform.SetParent(topRow.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(25, 25);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.cyan;

            LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.minWidth = 25;
            iconLayout.minHeight = 25;
        }

        // Text
        GameObject textObj = CreateTextElement("EnergyText", topRow.transform, "0/0", 18);

        // Energy Bar
        if (includeEnergyBar)
        {
            GameObject barBg = new GameObject("EnergyBarBackground");
            barBg.transform.SetParent(section.transform, false);

            RectTransform barBgRect = barBg.AddComponent<RectTransform>();
            barBgRect.sizeDelta = new Vector2(0, 10);

            Image barBgImage = barBg.AddComponent<Image>();
            barBgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill
            GameObject barFill = new GameObject("EnergyBarFill");
            barFill.transform.SetParent(barBg.transform, false);

            RectTransform barFillRect = barFill.AddComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = Vector2.zero;
            barFillRect.offsetMax = Vector2.zero;

            Image barFillImage = barFill.AddComponent<Image>();
            barFillImage.color = Color.green;
            barFillImage.type = Image.Type.Filled;
            barFillImage.fillMethod = Image.FillMethod.Horizontal;
            barFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFillImage.fillAmount = 1f;
        }

        return section;
    }

    private GameObject CreateTextElement(string name, Transform parent, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150, fontSize + 10);

        if (useTextMeshPro)
        {
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
        }
        else
        {
            Text uiText = textObj.AddComponent<Text>();
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.alignment = TextAnchor.MiddleLeft;
            uiText.color = Color.white;
            uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        LayoutElement layout = textObj.AddComponent<LayoutElement>();
        layout.minWidth = 80;

        return textObj;
    }
}
