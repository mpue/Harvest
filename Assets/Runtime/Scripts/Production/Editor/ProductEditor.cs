using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for Product ScriptableObject
/// </summary>
[CustomEditor(typeof(Product))]
public class ProductEditor : Editor
{
    // Display properties
    private SerializedProperty productName;
    private SerializedProperty previewImage;
    private SerializedProperty description;

    // Product Type properties
    private SerializedProperty isBuilding;
    private SerializedProperty buildingType;

    // Production properties
    private SerializedProperty prefab;
    private SerializedProperty productionDuration;

    // Cost properties
    private SerializedProperty foodCost;
    private SerializedProperty woodCost;
    private SerializedProperty stoneCost;
    private SerializedProperty goldCost;

    // Energy properties
    private SerializedProperty energyCost;
    private SerializedProperty energyProduction;

    private void OnEnable()
    {
        // Display
        productName = serializedObject.FindProperty("productName");
        previewImage = serializedObject.FindProperty("previewImage");
        description = serializedObject.FindProperty("description");

        // Product Type
        isBuilding = serializedObject.FindProperty("isBuilding");
        buildingType = serializedObject.FindProperty("buildingType");

        // Production
        prefab = serializedObject.FindProperty("prefab");
        productionDuration = serializedObject.FindProperty("productionDuration");

        // Costs
        foodCost = serializedObject.FindProperty("foodCost");
        woodCost = serializedObject.FindProperty("woodCost");
        stoneCost = serializedObject.FindProperty("stoneCost");
        goldCost = serializedObject.FindProperty("goldCost");

        // Energy
        energyCost = serializedObject.FindProperty("energyCost");
        energyProduction = serializedObject.FindProperty("energyProduction");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Product product = (Product)target;

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

        // ===== PRODUCT TYPE (WICHTIG!) =====
        EditorGUILayout.LabelField("Product Type", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        // Is Building checkbox - GROSS und DEUTLICH
        GUIStyle boldToggleStyle = new GUIStyle(EditorStyles.toggle);
        boldToggleStyle.fontStyle = FontStyle.Bold;
        boldToggleStyle.fontSize = 12;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(isBuilding, new GUIContent("??? Is Building", "Check this if this product is a building that needs to be placed"));
        EditorGUILayout.EndHorizontal();

        // Show building type only if isBuilding is true
        if (isBuilding.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(buildingType, new GUIContent("Building Type", "Type of building"));

            // Show info box based on building type
            switch ((BuildingType)buildingType.enumValueIndex)
            {
                case BuildingType.Headquarter:
                    EditorGUILayout.HelpBox("? Headquarter: Can produce buildings. Only one per player.", MessageType.Info);
                    break;
                case BuildingType.EnergyBlock:
                    EditorGUILayout.HelpBox("? Energy Block: Provides energy. Can be built without energy.", MessageType.Info);
                    break;
                case BuildingType.ProductionFacility:
                    EditorGUILayout.HelpBox("?? Production Facility: Produces units/resources.", MessageType.Info);
                    break;
                case BuildingType.DefenseTower:
                    EditorGUILayout.HelpBox("?? Defense Tower: Attacks enemies automatically.", MessageType.Info);
                    break;
                case BuildingType.ResourceCollector:
                    EditorGUILayout.HelpBox("?? Resource Collector: Gathers resources over time.", MessageType.Info);
                    break;
            }
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("This is a regular unit (not a building).\nIt will spawn directly at the spawn point.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

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

        // Energy Settings (only show for buildings)
        if (isBuilding.boolValue)
        {
            EditorGUILayout.LabelField("Energy Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(energyCost, new GUIContent("Energy Cost", "Energy consumed when building is active"));
            EditorGUILayout.PropertyField(energyProduction, new GUIContent("Energy Production", "Energy provided by this building"));

            // Show energy info
            if (energyProduction.intValue > 0)
            {
                EditorGUILayout.HelpBox($"? This building PROVIDES {energyProduction.intValue} energy", MessageType.Info);
            }
            if (energyCost.intValue > 0)
            {
                EditorGUILayout.HelpBox($"? This building CONSUMES {energyCost.intValue} energy", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Preview
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
        EditorGUILayout.LabelField($"Type: {(product.IsBuilding ? $"Building ({product.BuildingType})" : "Unit")}");
        EditorGUILayout.LabelField($"Duration: {product.ProductionDuration}s");
        EditorGUILayout.LabelField($"Cost: {product.GetCostString()}");

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Validation
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        bool hasIssues = false;

        if (string.IsNullOrEmpty(product.ProductName))
        {
            EditorGUILayout.HelpBox("? Product name is empty!", MessageType.Error);
            hasIssues = true;
        }

        if (product.Prefab == null)
        {
            EditorGUILayout.HelpBox("? No prefab assigned!", MessageType.Error);
            hasIssues = true;
        }

        if (product.IsBuilding && product.BuildingType == BuildingType.None)
        {
            EditorGUILayout.HelpBox("?? Building type is set to 'None'. Choose a building type!", MessageType.Warning);
            hasIssues = true;
        }

        if (product.IsBuilding && product.BuildingType == BuildingType.EnergyBlock && product.EnergyProduction <= 0)
        {
            EditorGUILayout.HelpBox("?? Energy Block should have Energy Production > 0", MessageType.Warning);
            hasIssues = true;
        }

        if (!hasIssues)
        {
            EditorGUILayout.HelpBox("? Product configuration looks good!", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        // Quick Actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("??? Make Building"))
        {
            isBuilding.boolValue = true;
            if (buildingType.enumValueIndex == 0) // If None
            {
                buildingType.enumValueIndex = 2; // Set to EnergyBlock by default
            }
        }

        if (GUILayout.Button("?? Make Unit"))
        {
            isBuilding.boolValue = false;
            buildingType.enumValueIndex = 0; // Set to None
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }
}
