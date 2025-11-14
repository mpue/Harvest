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
        
        if (GUILayout.Button("Create Slot Prefabs"))
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
 GameObject productSlot = CreateProductSlotPrefab();
        string productSlotPath = $"{path}/ProductSlot.prefab";
        PrefabUtility.SaveAsPrefabAsset(productSlot, productSlotPath);
        Object.DestroyImmediate(productSlot);

     AssetDatabase.SaveAssets();
  AssetDatabase.Refresh();

       // Ping the created prefab
        Object productSlotAsset = AssetDatabase.LoadAssetAtPath<Object>(productSlotPath);
        EditorGUIUtility.PingObject(productSlotAsset);

        Debug.Log($"Created ProductSlot prefab at {path}");
        EditorUtility.DisplayDialog("Success", 
        $"Created ProductSlot.prefab at {path}!", 
            "OK");
    }

 private GameObject CreateProductSlotPrefab()
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

        // Product Image
        GameObject imgObj = new GameObject("ProductImage");
        imgObj.transform.SetParent(slotObj.transform, false);
   RectTransform imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.5f);
   imgRect.anchorMax = new Vector2(0.9f, 0.9f);
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        Image productImage = imgObj.AddComponent<Image>();

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

        // Progress Bar
        GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(slotObj.transform, false);
    RectTransform progressBarRect = progressBarObj.AddComponent<RectTransform>();
        progressBarRect.anchorMin = new Vector2(0.05f, 0.48f);
  progressBarRect.anchorMax = new Vector2(0.95f, 0.52f);
        progressBarRect.offsetMin = Vector2.zero;
    progressBarRect.offsetMax = Vector2.zero;

        // Progress Fill
        GameObject progressFillObj = new GameObject("ProgressFill");
        progressFillObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform progressFillRect = progressFillObj.AddComponent<RectTransform>();
        progressFillRect.anchorMin = Vector2.zero;
        progressFillRect.anchorMax = Vector2.one;
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
        Image progressFillImage = progressFillObj.AddComponent<Image>();
        progressFillImage.color = Color.green;
  progressFillImage.type = Image.Type.Filled;
   progressFillImage.fillMethod = Image.FillMethod.Horizontal;

   // Wire up references
 SerializedObject so = new SerializedObject(slotScript);
        so.FindProperty("productImage").objectReferenceValue = productImage;
        so.FindProperty("productNameText").objectReferenceValue = nameText;
        so.FindProperty("costText").objectReferenceValue = costText;
        so.FindProperty("durationText").objectReferenceValue = durationText;
   so.FindProperty("productButton").objectReferenceValue = button;
        so.FindProperty("progressFill").objectReferenceValue = progressFillImage;
        so.FindProperty("progressBar").objectReferenceValue = progressBarObj;
  so.ApplyModifiedProperties();

    progressBarObj.SetActive(false);

        return slotObj;
    }
}
