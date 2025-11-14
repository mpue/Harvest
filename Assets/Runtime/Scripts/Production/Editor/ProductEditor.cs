using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for Product ScriptableObject
/// </summary>
[CustomEditor(typeof(Product))]
public class ProductEditor : Editor
{
    private SerializedProperty productName;
    private SerializedProperty previewImage;
 private SerializedProperty description;
    private SerializedProperty prefab;
    private SerializedProperty productionDuration;
    private SerializedProperty foodCost;
    private SerializedProperty woodCost;
    private SerializedProperty stoneCost;
    private SerializedProperty goldCost;

    private void OnEnable()
    {
        productName = serializedObject.FindProperty("productName");
     previewImage = serializedObject.FindProperty("previewImage");
        description = serializedObject.FindProperty("description");
     prefab = serializedObject.FindProperty("prefab");
        productionDuration = serializedObject.FindProperty("productionDuration");
        foodCost = serializedObject.FindProperty("foodCost");
 woodCost = serializedObject.FindProperty("woodCost");
   stoneCost = serializedObject.FindProperty("stoneCost");
        goldCost = serializedObject.FindProperty("goldCost");
    }

    public override void OnInspectorGUI()
    {
     serializedObject.Update();

   // Header
        EditorGUILayout.Space();
   EditorGUILayout.LabelField("Product Configuration", EditorStyles.boldLabel);
    EditorGUILayout.Space();

    // Display Info
     EditorGUILayout.LabelField("Display Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(productName);
        EditorGUILayout.PropertyField(previewImage);
        EditorGUILayout.PropertyField(description);

     EditorGUILayout.Space();

// Production Settings
    EditorGUILayout.LabelField("Production Settings", EditorStyles.boldLabel);
EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(productionDuration);

        EditorGUILayout.Space();

   // Costs
   EditorGUILayout.LabelField("Costs", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
     EditorGUILayout.PropertyField(foodCost);
        EditorGUILayout.PropertyField(woodCost);
        EditorGUILayout.PropertyField(stoneCost);
        EditorGUILayout.PropertyField(goldCost);
   EditorGUILayout.EndVertical();

   EditorGUILayout.Space();

        // Preview
 Product product = (Product)target;
    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        
   if (product.PreviewImage != null)
        {
       Rect imageRect = EditorGUILayout.GetControlRect(false, 100);
      imageRect.width = 100;
            EditorGUI.DrawPreviewTexture(imageRect, product.PreviewImage.texture);
 GUILayout.Space(100);
      }
  else
        {
     EditorGUILayout.HelpBox("No preview image assigned", MessageType.Info);
        }

        EditorGUILayout.LabelField($"Name: {product.ProductName}");
        EditorGUILayout.LabelField($"Duration: {product.ProductionDuration}s");
    EditorGUILayout.LabelField($"Cost: {product.GetCostString()}");
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

     // Validation
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
 bool hasIssues = false;

        if (string.IsNullOrEmpty(product.ProductName))
        {
    EditorGUILayout.HelpBox("Product name is empty!", MessageType.Warning);
         hasIssues = true;
        }

        if (product.Prefab == null)
  {
       EditorGUILayout.HelpBox("No prefab assigned! This product cannot be produced.", MessageType.Error);
       hasIssues = true;
        }

     if (product.ProductionDuration <= 0)
        {
EditorGUILayout.HelpBox("Production duration should be greater than 0!", MessageType.Warning);
     hasIssues = true;
        }

        if (product.PreviewImage == null)
   {
            EditorGUILayout.HelpBox("No preview image assigned. UI will look better with an image.", MessageType.Info);
        }

        int totalCost = product.FoodCost + product.WoodCost + product.StoneCost + product.GoldCost;
        if (totalCost == 0)
 {
      EditorGUILayout.HelpBox("This product has no cost. Consider adding resource costs.", MessageType.Info);
        }

        if (!hasIssues && product.Prefab != null)
  {
    EditorGUILayout.HelpBox("? Product is properly configured!", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
