using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor window to quickly setup the Production System UI
/// </summary>
public class ProductionSystemSetupWindow : EditorWindow
{
    [MenuItem("Tools/RTS/Production System Setup")]
    public static void ShowWindow()
    {
        GetWindow<ProductionSystemSetupWindow>("Production System Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Production System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool helps you quickly setup the Production System UI.",
      MessageType.Info
        );

        EditorGUILayout.Space();

  // Step 3: Prefab Templates
        EditorGUILayout.LabelField("Create Slot Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates template prefabs for ProductSlot and QueueSlot.", MessageType.None);
    
        EditorGUILayout.BeginHorizontal();
      
    if (GUILayout.Button("Create ProductSlot Prefab"))
        {
            CreateProductSlotPrefab();
        }
        
        if (GUILayout.Button("Create QueueSlot Prefab"))
        {
       CreateQueueSlotPrefab();
        }
  
 EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
  if (GUILayout.Button("Create Both Slot Prefabs"))
    {
     CreateSlotPrefabs();
        }
    }

    private void CreateSlotPrefabs()
    {
        string path = "Assets/Prefabs/UI";
    if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
      {
        AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(path))
        {
    AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // Create ProductSlot Prefab
        GameObject productSlot = CreateProductSlotPrefabObject();
        string productSlotPath = $"{path}/ProductSlot.prefab";
   PrefabUtility.SaveAsPrefabAsset(productSlot, productSlotPath);
        Object.DestroyImmediate(productSlot);

        // Create QueueSlot Prefab
        GameObject queueSlot = CreateQueueSlotPrefabObject();
   string queueSlotPath = $"{path}/QueueSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(queueSlot, queueSlotPath);
  Object.DestroyImmediate(queueSlot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Ping the created prefabs
        Object productSlotAsset = AssetDatabase.LoadAssetAtPath<Object>(productSlotPath);
        Object queueSlotAsset = AssetDatabase.LoadAssetAtPath<Object>(queueSlotPath);
        EditorGUIUtility.PingObject(productSlotAsset);
        Selection.activeObject = queueSlotAsset;

        Debug.Log($"Created ProductSlot and QueueSlot prefabs at {path}");
        EditorUtility.DisplayDialog("Success", 
   $"Created ProductSlot.prefab and QueueSlot.prefab at {path}!", 
            "OK");
 }

 private void CreateProductSlotPrefab()
  {
        string path = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
}
  if (!AssetDatabase.IsValidFolder(path))
      {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

  // Create ProductSlot Prefab
        GameObject productSlot = CreateProductSlotPrefabObject();
        string productSlotPath = $"{path}/ProductSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(productSlot, productSlotPath);
        Object.DestroyImmediate(productSlot);

      AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Ping the created prefab
        Object productSlotAsset = AssetDatabase.LoadAssetAtPath<Object>(productSlotPath);
        EditorGUIUtility.PingObject(productSlotAsset);
      Selection.activeObject = productSlotAsset;

        Debug.Log($"Created ProductSlot prefab at {path}");
        EditorUtility.DisplayDialog("Success", 
            $"Created ProductSlot.prefab at {path}!", 
            "OK");
    }

    private void CreateQueueSlotPrefab()
    {
        string path = "Assets/Prefabs/UI";
    if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
       AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(path))
        {
      AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

    // Create QueueSlot Prefab
   GameObject queueSlot = CreateQueueSlotPrefabObject();
 string queueSlotPath = $"{path}/QueueSlot.prefab";
  PrefabUtility.SaveAsPrefabAsset(queueSlot, queueSlotPath);
        Object.DestroyImmediate(queueSlot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Ping the created prefab
    Object queueSlotAsset = AssetDatabase.LoadAssetAtPath<Object>(queueSlotPath);
        EditorGUIUtility.PingObject(queueSlotAsset);
     Selection.activeObject = queueSlotAsset;

        Debug.Log($"Created QueueSlot prefab at {path}");
        EditorUtility.DisplayDialog("Success", 
 $"Created QueueSlot.prefab at {path}!", 
            "OK");
    }

    private GameObject CreateProductSlotPrefabObject()
    {
        // Root with ProductionSlot component
    GameObject slotObj = new GameObject("ProductSlot");
  RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(100, 120);
     ProductionSlot slotScript = slotObj.AddComponent<ProductionSlot>();

      // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(slotObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
    bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgObj.AddComponent<Image>();
    bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Overlay Dimmer (for production visual feedback)
    GameObject dimmerObj = new GameObject("OverlayDimmer");
 dimmerObj.transform.SetParent(slotObj.transform, false);
        RectTransform dimmerRect = dimmerObj.AddComponent<RectTransform>();
    dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
        dimmerRect.offsetMin = Vector2.zero;
      dimmerRect.offsetMax = Vector2.zero;
        Image dimmerImage = dimmerObj.AddComponent<Image>();
        dimmerImage.color = new Color(0f, 0f, 0f, 0.3f);
        dimmerObj.SetActive(false);

        // Product Image
      GameObject imgObj = new GameObject("ProductImage");
        imgObj.transform.SetParent(slotObj.transform, false);
      RectTransform imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.5f);
    imgRect.anchorMax = new Vector2(0.9f, 0.9f);
    imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
      Image productImage = imgObj.AddComponent<Image>();

        // Production Indicator (rotating icon)
  GameObject indicatorObj = new GameObject("ProductionIndicator");
    indicatorObj.transform.SetParent(imgObj.transform, false);
        RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0.7f, 0.7f);
  indicatorRect.anchorMax = new Vector2(1f, 1f);
     indicatorRect.offsetMin = Vector2.zero;
        indicatorRect.offsetMax = Vector2.zero;
        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = Color.yellow;
indicatorObj.SetActive(false);

// Product Name
        GameObject nameObj = new GameObject("ProductName");
     nameObj.transform.SetParent(slotObj.transform, false);
  RectTransform nameRect = nameObj.AddComponent<RectTransform>();
  nameRect.anchorMin = new Vector2(0.05f, 0.35f);
        nameRect.anchorMax = new Vector2(0.95f, 0.45f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
 nameText.text = "Unit";
      nameText.fontSize = 12;
  nameText.alignment = TextAlignmentOptions.Center;

    // Cost Text
        GameObject costObj = new GameObject("CostText");
    costObj.transform.SetParent(slotObj.transform, false);
      RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0.05f, 0.2f);
        costRect.anchorMax = new Vector2(0.95f, 0.3f);
        costRect.offsetMin = Vector2.zero;
        costRect.offsetMax = Vector2.zero;
        TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.text = "100g";
     costText.fontSize = 10;
     costText.alignment = TextAlignmentOptions.Center;

   // Duration Text
      GameObject durationObj = new GameObject("DurationText");
        durationObj.transform.SetParent(slotObj.transform, false);
     RectTransform durationRect = durationObj.AddComponent<RectTransform>();
        durationRect.anchorMin = new Vector2(0.05f, 0.05f);
        durationRect.anchorMax = new Vector2(0.95f, 0.15f);
        durationRect.offsetMin = Vector2.zero;
        durationRect.offsetMax = Vector2.zero;
        TextMeshProUGUI durationText = durationObj.AddComponent<TextMeshProUGUI>();
        durationText.text = "30s";
     durationText.fontSize = 10;
        durationText.alignment = TextAlignmentOptions.Center;

        // Button
        Button button = slotObj.AddComponent<Button>();
        button.targetGraphic = bgImage;

        // Progress Bar Container
        GameObject progressBarObj = new GameObject("ProgressBar");
   progressBarObj.transform.SetParent(slotObj.transform, false);
 RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
        progressBarRect.anchorMin = new Vector2(0.05f, 0.46f);
 progressBarRect.anchorMax = new Vector2(0.95f, 0.54f);
        progressBarRect.offsetMin = Vector2.zero;
        progressBarRect.offsetMax = Vector2.zero;

        // Progress Bar Background
      GameObject progressBgObj = new GameObject("Background");
   progressBgObj.transform.SetParent(progressBarObj.transform, false);
      RectTransform progressBgRect = progressBgObj.AddComponent<RectTransform>();
        progressBgRect.anchorMin = Vector2.zero;
        progressBgRect.anchorMax = Vector2.one;
        progressBgRect.offsetMin = Vector2.zero;
        progressBgRect.offsetMax = Vector2.zero;
        Image progressBgImage = progressBgObj.AddComponent<Image>();
        progressBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Progress Fill
  GameObject progressFillObj = new GameObject("ProgressFill");
        progressFillObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressFillRect = progressFillObj.AddComponent<RectTransform>();
        progressFillRect.anchorMin = Vector2.zero;
      progressFillRect.anchorMax = Vector2.one;
   progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
        Image progressFillImage = progressFillObj.AddComponent<Image>();
        progressFillImage.color = Color.yellow;
        progressFillImage.type = Image.Type.Filled;
        progressFillImage.fillMethod = Image.FillMethod.Horizontal;

 // Progress Glow Effect
        GameObject progressGlowObj = new GameObject("ProgressGlow");
        progressGlowObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressGlowRect = progressGlowObj.AddComponent<RectTransform>();
  progressGlowRect.anchorMin = new Vector2(-0.1f, -0.5f);
      progressGlowRect.anchorMax = new Vector2(1.1f, 1.5f);
        progressGlowRect.offsetMin = Vector2.zero;
        progressGlowRect.offsetMax = Vector2.zero;
        Image progressGlowImage = progressGlowObj.AddComponent<Image>();
        progressGlowImage.color = new Color(1f, 1f, 0f, 0.3f);
        progressGlowObj.SetActive(false);

        // Progress Text
        GameObject progressTextObj = new GameObject("ProgressText");
        progressTextObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressTextRect = progressTextObj.AddComponent<RectTransform>();
        progressTextRect.anchorMin = Vector2.zero;
   progressTextRect.anchorMax = Vector2.one;
        progressTextRect.offsetMin = Vector2.zero;
        progressTextRect.offsetMax = Vector2.zero;
   TextMeshProUGUI progressTextTMP = progressTextObj.AddComponent<TextMeshProUGUI>();
        progressTextTMP.text = "50%";
        progressTextTMP.fontSize = 8;
        progressTextTMP.alignment = TextAlignmentOptions.Center;
        progressTextTMP.color = Color.white;
        progressTextTMP.fontStyle = FontStyles.Bold;
    progressTextObj.SetActive(false);

      // Wire up references
        SerializedObject so = new SerializedObject(slotScript);
  so.FindProperty("productImage").objectReferenceValue = productImage;
        so.FindProperty("productNameText").objectReferenceValue = nameText;
   so.FindProperty("costText").objectReferenceValue = costText;
        so.FindProperty("durationText").objectReferenceValue = durationText;
        so.FindProperty("productButton").objectReferenceValue = button;
        so.FindProperty("progressFill").objectReferenceValue = progressFillImage;
        so.FindProperty("progressBar").objectReferenceValue = progressBarObj;
    so.FindProperty("progressText").objectReferenceValue = progressTextTMP;
    so.FindProperty("progressGlow").objectReferenceValue = progressGlowImage;
        so.FindProperty("overlayDimmer").objectReferenceValue = dimmerImage;
   so.FindProperty("productionIndicator").objectReferenceValue = indicatorObj;
    so.ApplyModifiedProperties();

        progressBarObj.SetActive(false);

     return slotObj;
    }

    private GameObject CreateQueueSlotPrefabObject()
    {
        // Root with ProductionSlot component (reused for queue slots)
        GameObject slotObj = new GameObject("QueueSlot");
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
 slotRect.sizeDelta = new Vector2(80, 100);
  ProductionSlot slotScript = slotObj.AddComponent<ProductionSlot>();

        // Background
        GameObject bgObj = new GameObject("Background");
bgObj.transform.SetParent(slotObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
     bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
   Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

   // Overlay Dimmer (for production visual feedback)
    GameObject dimmerObj = new GameObject("OverlayDimmer");
        dimmerObj.transform.SetParent(slotObj.transform, false);
        RectTransform dimmerRect = dimmerObj.AddComponent<RectTransform>();
     dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
   dimmerRect.offsetMin = Vector2.zero;
        dimmerRect.offsetMax = Vector2.zero;
  Image dimmerImage = dimmerObj.AddComponent<Image>();
        dimmerImage.color = new Color(0f, 0f, 0f, 0.3f);
        dimmerObj.SetActive(false);

        // Product Image (smaller for queue)
        GameObject imgObj = new GameObject("ProductImage");
imgObj.transform.SetParent(slotObj.transform, false);
        RectTransform imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.4f);
        imgRect.anchorMax = new Vector2(0.9f, 0.9f);
   imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        Image productImage = imgObj.AddComponent<Image>();

        // Production Indicator (rotating icon)
        GameObject indicatorObj = new GameObject("ProductionIndicator");
        indicatorObj.transform.SetParent(imgObj.transform, false);
        RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0.65f, 0.65f);
        indicatorRect.anchorMax = new Vector2(1f, 1f);
        indicatorRect.offsetMin = Vector2.zero;
   indicatorRect.offsetMax = Vector2.zero;
    Image indicatorImage = indicatorObj.AddComponent<Image>();
    indicatorImage.color = Color.yellow;
        indicatorObj.SetActive(false);

        // Product Name (smaller text)
        GameObject nameObj = new GameObject("ProductName");
   nameObj.transform.SetParent(slotObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
nameRect.anchorMin = new Vector2(0.05f, 0.25f);
        nameRect.anchorMax = new Vector2(0.95f, 0.35f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Unit";
        nameText.fontSize = 10;
        nameText.alignment = TextAlignmentOptions.Center;

        // No cost text for queue slots
    // No duration text for queue slots
        // No button for queue slots (not clickable)

        // Progress Bar Container (more prominent for queue)
    GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(slotObj.transform, false);
    RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
  progressBarRect.anchorMin = new Vector2(0.05f, 0.05f);
     progressBarRect.anchorMax = new Vector2(0.95f, 0.2f);
   progressBarRect.offsetMin = Vector2.zero;
        progressBarRect.offsetMax = Vector2.zero;

        // Progress Bar Background
        GameObject progressBgObj = new GameObject("Background");
   progressBgObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressBgRect = progressBgObj.AddComponent<RectTransform>();
    progressBgRect.anchorMin = Vector2.zero;
        progressBgRect.anchorMax = Vector2.one;
        progressBgRect.offsetMin = Vector2.zero;
        progressBgRect.offsetMax = Vector2.zero;
        Image progressBgImage = progressBgObj.AddComponent<Image>();
        progressBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

  // Progress Fill
        GameObject progressFillObj = new GameObject("ProgressFill");
        progressFillObj.transform.SetParent(progressBarObj.transform, false);
    RectTransform progressFillRect = progressFillObj.AddComponent<RectTransform>();
        progressFillRect.anchorMin = Vector2.zero;
 progressFillRect.anchorMax = Vector2.one;
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
        Image progressFillImage = progressFillObj.AddComponent<Image>();
        progressFillImage.color = new Color(0f, 0.8f, 1f, 1f); // Cyan color for queue
        progressFillImage.type = Image.Type.Filled;
    progressFillImage.fillMethod = Image.FillMethod.Horizontal;

  // Progress Glow Effect
        GameObject progressGlowObj = new GameObject("ProgressGlow");
        progressGlowObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressGlowRect = progressGlowObj.AddComponent<RectTransform>();
  progressGlowRect.anchorMin = new Vector2(-0.1f, -0.5f);
        progressGlowRect.anchorMax = new Vector2(1.1f, 1.5f);
        progressGlowRect.offsetMin = Vector2.zero;
 progressGlowRect.offsetMax = Vector2.zero;
        Image progressGlowImage = progressGlowObj.AddComponent<Image>();
        progressGlowImage.color = new Color(0f, 0.8f, 1f, 0.3f); // Cyan glow
        progressGlowObj.SetActive(false);

        // Progress Text
        GameObject progressTextObj = new GameObject("ProgressText");
        progressTextObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressTextRect = progressTextObj.AddComponent<RectTransform>();
        progressTextRect.anchorMin = Vector2.zero;
        progressTextRect.anchorMax = Vector2.one;
     progressTextRect.offsetMin = Vector2.zero;
        progressTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI progressTextTMP = progressTextObj.AddComponent<TextMeshProUGUI>();
        progressTextTMP.text = "0%";
     progressTextTMP.fontSize = 9;
        progressTextTMP.alignment = TextAlignmentOptions.Center;
        progressTextTMP.color = Color.white;
    progressTextTMP.fontStyle = FontStyles.Bold;
 progressTextObj.SetActive(false);

        // Wire up references
        SerializedObject so = new SerializedObject(slotScript);
        so.FindProperty("productImage").objectReferenceValue = productImage;
        so.FindProperty("productNameText").objectReferenceValue = nameText;
      so.FindProperty("costText").objectReferenceValue = null; // No cost text for queue
      so.FindProperty("durationText").objectReferenceValue = null; // No duration text for queue
        so.FindProperty("productButton").objectReferenceValue = null; // No button for queue
        so.FindProperty("progressFill").objectReferenceValue = progressFillImage;
      so.FindProperty("progressBar").objectReferenceValue = progressBarObj;
        so.FindProperty("progressText").objectReferenceValue = progressTextTMP;
   so.FindProperty("progressGlow").objectReferenceValue = progressGlowImage;
        so.FindProperty("overlayDimmer").objectReferenceValue = dimmerImage;
 so.FindProperty("productionIndicator").objectReferenceValue = indicatorObj;
        so.ApplyModifiedProperties();

        // Progress bar starts visible for queue slots (will be controlled by script)
      progressBarObj.SetActive(true);

        return slotObj;
    }
}
