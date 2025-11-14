using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for ProductionComponent to make setup easier
/// </summary>
[CustomEditor(typeof(ProductionComponent))]
public class ProductionComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

     ProductionComponent production = (ProductionComponent)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Production Info", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
      EditorGUILayout.LabelField($"Queue Count: {production.QueueCount}/{production.MaxQueueSize}");
            EditorGUILayout.LabelField($"Is Producing: {production.IsProducing}");
     
    if (production.IsProducing && production.CurrentProduct != null)
      {
        EditorGUILayout.LabelField($"Current: {production.CurrentProduct.ProductName}");
        EditorGUILayout.LabelField($"Progress: {production.CurrentProductionProgress:P0}");
      
                // Progress bar
  Rect rect = EditorGUILayout.GetControlRect(false, 20);
 EditorGUI.ProgressBar(rect, production.CurrentProductionProgress, "Production Progress");
        }

         EditorGUILayout.Space();
 
            // Runtime controls
         EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
            
     if (GUILayout.Button("Cancel Current Production"))
            {
      production.CancelCurrentProduction();
            }
          
      if (GUILayout.Button("Clear Queue"))
     {
      production.CancelQueue();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see production status and controls", MessageType.Info);
        }

   EditorGUILayout.Space();

      // Setup helpers
        EditorGUILayout.LabelField("Setup Helpers", EditorStyles.boldLabel);
        
  if (GUILayout.Button("Create Spawn Point"))
  {
      Transform spawnPoint = production.transform.Find("SpawnPoint");
       if (spawnPoint == null)
 {
        GameObject spawnObj = new GameObject("SpawnPoint");
       spawnObj.transform.SetParent(production.transform);
    spawnObj.transform.localPosition = Vector3.forward * 3f;
   
      // Add SpawnPoint component for visualization
      spawnObj.AddComponent<SpawnPoint>();
    
        Undo.RegisterCreatedObjectUndo(spawnObj, "Create Spawn Point");
    EditorUtility.SetDirty(production);
      }
    else
    {
          EditorUtility.DisplayDialog("Info", "Spawn Point already exists!", "OK");
 }
        }

   if (GUILayout.Button("Create Rally Point"))
      {
       Transform rallyPoint = production.transform.Find("RallyPoint");
     if (rallyPoint == null)
        {
          GameObject rallyObj = new GameObject("RallyPoint");
          rallyObj.transform.SetParent(production.transform);
  rallyObj.transform.localPosition = Vector3.forward * 5f;
        
        // Add RallyPoint component for visualization
    rallyObj.AddComponent<RallyPoint>();
             
    Undo.RegisterCreatedObjectUndo(rallyObj, "Create Rally Point");
  EditorUtility.SetDirty(production);
    }
        else
         {
       EditorUtility.DisplayDialog("Info", "Rally Point already exists!", "OK");
            }
   }

     // Add components to existing points
        EditorGUILayout.Space();
     
        SerializedProperty spawnPointProp = serializedObject.FindProperty("spawnPoint");
    SerializedProperty rallyPointProp = serializedObject.FindProperty("rallyPoint");
      
        if (spawnPointProp.objectReferenceValue != null)
        {
            Transform sp = (Transform)spawnPointProp.objectReferenceValue;
    if (sp.GetComponent<SpawnPoint>() == null)
 {
      EditorGUILayout.HelpBox("Spawn Point is missing SpawnPoint component for visualization", MessageType.Info);
    if (GUILayout.Button("Add SpawnPoint Component"))
         {
          Undo.AddComponent<SpawnPoint>(sp.gameObject);
                }
      }
        }
        
        if (rallyPointProp.objectReferenceValue != null)
      {
     Transform rp = (Transform)rallyPointProp.objectReferenceValue;
 if (rp.GetComponent<RallyPoint>() == null)
   {
 EditorGUILayout.HelpBox("Rally Point is missing RallyPoint component for visualization", MessageType.Info);
       if (GUILayout.Button("Add RallyPoint Component"))
         {
       Undo.AddComponent<RallyPoint>(rp.gameObject);
  }
            }
        }

        // Validation
        EditorGUILayout.Space();
   EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        
        bool hasIssues = false;
        
   if (production.AvailableProducts == null || production.AvailableProducts.Count == 0)
     {
        EditorGUILayout.HelpBox("No products assigned! Add products to the Available Products list.", MessageType.Warning);
     hasIssues = true;
        }
     else
        {
    int invalidProducts = 0;
            foreach (var product in production.AvailableProducts)
        {
        if (product == null || product.Prefab == null)
          {
              invalidProducts++;
}
            }
            
    if (invalidProducts > 0)
         {
       EditorGUILayout.HelpBox($"{invalidProducts} product(s) have missing or invalid prefabs!", MessageType.Warning);
    hasIssues = true;
        }
}
        
        if (!hasIssues)
        {
            EditorGUILayout.HelpBox("? Production Component is properly configured!", MessageType.Info);
        }
  }
}
