using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Comprehensive Unit Editor Window
/// Allows editing all unit components in one place:
/// - BaseUnit
/// - TeamComponent
/// - Controllable
/// - Health
/// - NavMeshAgent (for non-buildings)
/// - WeaponController with Weapons
/// - Audio Settings
/// - Preview Image
/// </summary>
public class UnitEditorWindow : EditorWindow
{
    private GameObject selectedUnit;
    private BaseUnit baseUnit;
    private TeamComponent teamComponent;
    private Controllable controllable;
    private Health health;
    private NavMeshAgent navAgent;
    private WeaponController weaponController;

    private Vector2 scrollPosition;
    private Texture2D previewTexture;
    private Editor gameObjectEditor;
    private bool showPreview = true;
    private bool showBaseUnitSettings = true;
    private bool showTeamSettings = true;
    private bool showControllableSettings = true;
    private bool showHealthSettings = true;
    private bool showNavMeshSettings = true;
    private bool showWeaponSettings = true;
    private bool showQuickActions = true;
    private bool showAudioSettings = true;
    private bool showCreateNewUnit = true;
    private bool showModelImport = true;

    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle tabButtonStyle;

    // Tab system for better organization
    private enum EditorTab
    {
        CreateUnit,
        ImportModel,
        EditComponents,
        Audio
    }
    private EditorTab currentTab = EditorTab.CreateUnit;

    // Create New Unit fields
    private string newUnitName = "New Unit";
    private UnitType newUnitType = UnitType.Infantry;
    private Team newUnitTeam = Team.Player;
    private bool newUnitIsBuilding = false;
    private PrimitiveType newUnitMeshType = PrimitiveType.Capsule;
    private bool createWithVisuals = true;

    // Model Import fields
    private GameObject importedModel;
    private List<Transform> weaponPoints = new List<Transform>();
    private List<Transform> turretTransforms = new List<Transform>();
    private Transform selectionIndicatorTransform;
    private Vector2 modelScrollPosition;
    private bool showWeaponPointsList = true;
    private bool showTurretsList = true;
    private bool addMovementComponents = true;
    private bool addWeaponController = false;

    public enum UnitType
    {
        Infantry,
        Vehicle,
        Archer,
        Building,
        Worker,
        Custom
    }

    public enum TransformRole
    {
        None,
        WeaponPoint,
        Turret,
        SelectionIndicator
    }

    [MenuItem("Tools/Unit Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<UnitEditorWindow>("Unit Editor");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    void OnEnable()
    {
        if (Selection.activeGameObject != null)
        {
            TryLoadUnit(Selection.activeGameObject);
        }

        Selection.selectionChanged += OnSelectionChanged;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;

        if (gameObjectEditor != null)
        {
            DestroyImmediate(gameObjectEditor);
        }
    }

    void OnSelectionChanged()
    {
        if (Selection.activeGameObject != null)
        {
            TryLoadUnit(Selection.activeGameObject);
        }
    }

    void TryLoadUnit(GameObject obj)
    {
        BaseUnit unit = obj.GetComponent<BaseUnit>();
        if (unit != null)
        {
            selectedUnit = obj;
            LoadComponents();
            Repaint();
        }
    }

    void LoadComponents()
    {
        if (selectedUnit == null) return;

        baseUnit = selectedUnit.GetComponent<BaseUnit>();
        teamComponent = selectedUnit.GetComponent<TeamComponent>();
        controllable = selectedUnit.GetComponent<Controllable>();
        health = selectedUnit.GetComponent<Health>();
        navAgent = selectedUnit.GetComponent<NavMeshAgent>();
        weaponController = selectedUnit.GetComponent<WeaponController>();

        GeneratePreview();
    }

    void GeneratePreview()
    {
        if (selectedUnit == null) return;

        if (gameObjectEditor != null)
        {
            DestroyImmediate(gameObjectEditor);
        }

        gameObjectEditor = Editor.CreateEditor(selectedUnit);
    }

    void DrawSeparator()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        }

        if (subHeaderStyle == null)
        {
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 12;
        }

        if (tabButtonStyle == null)
        {
            tabButtonStyle = new GUIStyle(EditorStyles.miniButton);
            tabButtonStyle.fontSize = 11;
            tabButtonStyle.fixedHeight = 30;
        }
    }

    void OnGUI()
    {
        InitStyles();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("UNIT EDITOR", headerStyle);
        GUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        selectedUnit = (GameObject)EditorGUILayout.ObjectField("Selected Unit", selectedUnit, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            if (selectedUnit != null)
            {
                TryLoadUnit(selectedUnit);
            }
        }

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Tab Navigation
        DrawTabButtons();

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Draw content based on selected tab
        switch (currentTab)
        {
            case EditorTab.CreateUnit:
                DrawCreateNewUnitSection();
                break;

            case EditorTab.ImportModel:
                DrawModelImportSection();
                break;

            case EditorTab.EditComponents:
                if (selectedUnit == null || baseUnit == null)
                {
                    EditorGUILayout.HelpBox("Select a GameObject with a BaseUnit component to edit.", MessageType.Info);
                }
                else
                {
                    DrawEditComponentsTab();
                }
                break;

            case EditorTab.Audio:
                if (selectedUnit == null || baseUnit == null)
                {
                    EditorGUILayout.HelpBox("Select a GameObject with a BaseUnit component to edit audio settings.", MessageType.Info);
                }
                else
                {
                    DrawAudioSection();
                }
                break;
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }

    void DrawTabButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = currentTab == EditorTab.CreateUnit ? new Color(0.3f, 0.8f, 0.3f) : Color.white;
        if (GUILayout.Button("Create New Unit", tabButtonStyle))
        {
            currentTab = EditorTab.CreateUnit;
        }

        GUI.backgroundColor = currentTab == EditorTab.ImportModel ? new Color(0.3f, 0.6f, 0.8f) : Color.white;
        if (GUILayout.Button("Import 3D Model", tabButtonStyle))
        {
            currentTab = EditorTab.ImportModel;
        }

        GUI.backgroundColor = currentTab == EditorTab.EditComponents ? new Color(0.8f, 0.6f, 0.3f) : Color.white;
        if (GUILayout.Button("Edit Components", tabButtonStyle))
        {
            currentTab = EditorTab.EditComponents;
        }

        GUI.backgroundColor = currentTab == EditorTab.Audio ? new Color(0.6f, 0.3f, 0.8f) : Color.white;
        if (GUILayout.Button("Audio Settings", tabButtonStyle))
        {
            currentTab = EditorTab.Audio;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    void DrawEditComponentsTab()
    {
        // Preview
        if (showPreview)
        {
            DrawPreviewSection();
        }

        EditorGUILayout.Space(5);
        DrawSeparator();

        // Quick Actions
        showQuickActions = EditorGUILayout.Foldout(showQuickActions, "Quick Actions", true);
        if (showQuickActions)
        {
            DrawQuickActions();
        }

        EditorGUILayout.Space(5);
        DrawSeparator();

        // Base Unit
        showBaseUnitSettings = EditorGUILayout.Foldout(showBaseUnitSettings, "Base Unit Settings", true);
        if (showBaseUnitSettings)
        {
            DrawBaseUnitSection();
        }

        EditorGUILayout.Space(5);
        DrawSeparator();

        // Team
        showTeamSettings = EditorGUILayout.Foldout(showTeamSettings, "Team Settings", true);
        if (showTeamSettings)
        {
            DrawTeamSection();
        }

        EditorGUILayout.Space(5);
        DrawSeparator();

        // Health
        showHealthSettings = EditorGUILayout.Foldout(showHealthSettings, "Health Settings", true);
        if (showHealthSettings)
        {
            DrawHealthSection();
        }

        // Movement (only for non-buildings)
        if (!baseUnit.IsBuilding)
        {
            EditorGUILayout.Space(5);
            DrawSeparator();

            showControllableSettings = EditorGUILayout.Foldout(showControllableSettings, "Movement Settings (Controllable)", true);
            if (showControllableSettings)
            {
                DrawControllableSection();
            }

            EditorGUILayout.Space(5);
            DrawSeparator();

            showNavMeshSettings = EditorGUILayout.Foldout(showNavMeshSettings, "NavMesh Settings", true);
            if (showNavMeshSettings)
            {
                DrawNavMeshSection();
            }
        }

        EditorGUILayout.Space(5);
        DrawSeparator();

        // Weapons
        showWeaponSettings = EditorGUILayout.Foldout(showWeaponSettings, "Weapon Settings", true);
        if (showWeaponSettings)
        {
            DrawWeaponSection();
        }
    }

    void DrawModelImportSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Import & Configure 3D Model", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Import a 3D model and assign weapon points, turrets, and selection indicators.", MessageType.Info);
        EditorGUILayout.Space(5);

        // Step 1: Select Model
        EditorGUILayout.LabelField("Step 1: Select 3D Model", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        importedModel = (GameObject)EditorGUILayout.ObjectField("3D Model Prefab", importedModel, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && importedModel != null)
        {
            // Scan model for potential transforms
            ScanModelTransforms();
        }

        if (importedModel == null)
        {
            EditorGUILayout.HelpBox("Select a 3D model prefab to begin configuration.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Step 2: Configure Transform Roles
        EditorGUILayout.LabelField("Step 2: Assign Transform Roles", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox("Select child transforms and assign their roles (Weapon Point, Turret, Selection Indicator).", MessageType.Info);

        modelScrollPosition = EditorGUILayout.BeginScrollView(modelScrollPosition, GUILayout.Height(250));
        DrawTransformHierarchy(importedModel.transform, 0);
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Step 3: Review Assignments
        EditorGUILayout.LabelField("Step 3: Review Assignments", EditorStyles.miniBoldLabel);

        // Weapon Points
        showWeaponPointsList = EditorGUILayout.Foldout(showWeaponPointsList, $"Weapon Points ({weaponPoints.Count})", true);
        if (showWeaponPointsList && weaponPoints.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < weaponPoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {weaponPoints[i].name}", GUILayout.Width(200));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    weaponPoints.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        // Turrets
        showTurretsList = EditorGUILayout.Foldout(showTurretsList, $"Turrets ({turretTransforms.Count})", true);
        if (showTurretsList && turretTransforms.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < turretTransforms.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {turretTransforms[i].name}", GUILayout.Width(200));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    turretTransforms.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        // Selection Indicator - Now with direct assignment option
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selection Indicator:", GUILayout.Width(150));

        EditorGUI.BeginChangeCheck();
        selectionIndicatorTransform = (Transform)EditorGUILayout.ObjectField(
         selectionIndicatorTransform,
          typeof(Transform),
       true,
      GUILayout.Width(200)
      );
        if (EditorGUI.EndChangeCheck())
        {
            // Validate that it's part of the imported model
            if (selectionIndicatorTransform != null && importedModel != null)
            {
                if (!selectionIndicatorTransform.IsChildOf(importedModel.transform) &&
            selectionIndicatorTransform != importedModel.transform)
                {
                    EditorUtility.DisplayDialog("Invalid Selection",
                "Selection Indicator must be part of the imported model hierarchy.",
                     "OK");
                    selectionIndicatorTransform = null;
                }
            }
        }

        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            selectionIndicatorTransform = null;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Step 4: Unit Configuration
        EditorGUILayout.LabelField("Step 4: Unit Configuration", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        addMovementComponents = EditorGUILayout.Toggle("Add Movement Components", addMovementComponents);
        EditorGUI.BeginDisabledGroup(!addMovementComponents);
        EditorGUI.indentLevel++;
        EditorGUILayout.HelpBox("Adds Controllable and NavMeshAgent components for movable units.", MessageType.Info);
        EditorGUI.indentLevel--;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(5);

        addWeaponController = EditorGUILayout.Toggle("Add WeaponController", addWeaponController);
        EditorGUI.BeginDisabledGroup(!addWeaponController);
        EditorGUI.indentLevel++;
        if (weaponPoints.Count > 0)
        {
            EditorGUILayout.HelpBox($"Will create {weaponPoints.Count} weapon(s) automatically.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No weapon points assigned. WeaponController will be added without weapons.", MessageType.Warning);
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        DrawSeparator();

        // Step 5: Apply to Unit
        EditorGUILayout.LabelField("Step 5: Apply to Unit", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();

        // Unit selection for applying
        EditorGUI.BeginChangeCheck();
        GameObject targetUnit = (GameObject)EditorGUILayout.ObjectField("Target Unit", selectedUnit, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            if (targetUnit != null)
            {
                TryLoadUnit(targetUnit);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Apply buttons
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);

        if (GUILayout.Button("Create New Unit with Model", GUILayout.Height(35)))
        {
            CreateUnitWithImportedModel();
        }

        GUI.backgroundColor = new Color(0.3f, 0.6f, 0.8f);

        if (selectedUnit != null && GUILayout.Button("Apply to Selected Unit", GUILayout.Height(35)))
        {
            ApplyModelToExistingUnit();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Quick Actions
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Auto-Detect Weapons", GUILayout.Height(25)))
        {
            AutoDetectWeaponPoints();
        }

        if (GUILayout.Button("Auto-Detect Turrets", GUILayout.Height(25)))
        {
            AutoDetectTurrets();
        }

        if (GUILayout.Button("Clear All", GUILayout.Height(25)))
        {
            ClearAllAssignments();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void CreateUnitWithImportedModel()
    {
        if (importedModel == null)
        {
            EditorUtility.DisplayDialog("Error", "No model selected!", "OK");
            return;
        }

        // Create new unit GameObject
        GameObject newUnit = new GameObject(importedModel.name + "_Unit");
        Undo.RegisterCreatedObjectUndo(newUnit, "Create Unit with Model");

        // Add BaseUnit
        BaseUnit baseUnitComp = newUnit.AddComponent<BaseUnit>();
        SerializedObject serializedBaseUnit = new SerializedObject(baseUnitComp);
        serializedBaseUnit.FindProperty("unitName").stringValue = importedModel.name;
        serializedBaseUnit.FindProperty("isBuilding").boolValue = !addMovementComponents;
        serializedBaseUnit.ApplyModifiedProperties();

        // Add TeamComponent
        newUnit.AddComponent<TeamComponent>();

        // Add Health
        newUnit.AddComponent<Health>();

        // Add movement components if requested
        if (addMovementComponents)
        {
            Controllable controllableComp = newUnit.AddComponent<Controllable>();
            SerializedObject serializedControllable = new SerializedObject(controllableComp);
            serializedControllable.FindProperty("moveSpeed").floatValue = 5f;
            serializedControllable.ApplyModifiedProperties();

            NavMeshAgent navAgentComp = newUnit.AddComponent<NavMeshAgent>();
            navAgentComp.speed = 5f;
            navAgentComp.angularSpeed = 120f;
            navAgentComp.acceleration = 8f;
            navAgentComp.stoppingDistance = 0.5f;
        }

        // Instantiate model as child
        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(importedModel, newUnit.transform);
        modelInstance.name = "Model";

        // Apply weapon configuration if requested
        if (addWeaponController || weaponPoints.Count > 0)
        {
            ApplyWeaponConfiguration(newUnit, modelInstance.transform);
        }

        // Apply selection indicator
        if (selectionIndicatorTransform != null)
        {
            Transform indicatorInInstance = FindTransformInHierarchy(modelInstance.transform, selectionIndicatorTransform.name);
            if (indicatorInInstance != null)
            {
                serializedBaseUnit = new SerializedObject(baseUnitComp);
                serializedBaseUnit.FindProperty("selectionIndicator").objectReferenceValue = indicatorInInstance.gameObject;
                serializedBaseUnit.ApplyModifiedProperties();
            }
        }

        // Add collider (approximate)
        AddApproximateCollider(newUnit);

        // Add Rigidbody if movement components were added
        if (addMovementComponents)
        {
            Rigidbody rb = newUnit.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Setup AudioManager integration
        if (AudioManager.Instance != null)
        {
            AudioSource audioSource = AudioManager.Instance.CreateAudioSource(
             newUnit,
                    AudioManager.AudioCategory.UnitSounds,
                         false
                     );
        }

        // Position in scene
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            newUnit.transform.position = sceneView.pivot;
        }

        // Select and load
        Selection.activeGameObject = newUnit;
        EditorGUIUtility.PingObject(newUnit);
        TryLoadUnit(newUnit);

        string configSummary = "Configuration:\n" +
     $"• Weapon Points: {weaponPoints.Count}\n" +
        $"• Turrets: {turretTransforms.Count}\n" +
           $"• Selection Indicator: {(selectionIndicatorTransform != null ? "Set" : "Not Set")}\n" +
      $"• Movement: {(addMovementComponents ? "Enabled" : "Disabled")}\n" +
      $"• Weapons: {(addWeaponController ? "Enabled" : "Disabled")}";

        EditorUtility.DisplayDialog("Success",
       $"Unit '{importedModel.name}' created successfully!\n\n{configSummary}",
      "OK");
    }

    void ApplyModelToExistingUnit()
    {
        if (selectedUnit == null)
        {
            EditorUtility.DisplayDialog("Error", "No unit selected!", "OK");
            return;
        }

        if (importedModel == null)
        {
            EditorUtility.DisplayDialog("Error", "No model selected!", "OK");
            return;
        }

        Undo.RecordObject(selectedUnit, "Apply Model to Unit");

        // Remove old visuals
        Transform oldVisuals = selectedUnit.transform.Find("Visuals");
        if (oldVisuals != null)
        {
            Undo.DestroyObjectImmediate(oldVisuals.gameObject);
        }

        Transform oldModel = selectedUnit.transform.Find("Model");
        if (oldModel != null)
        {
            Undo.DestroyObjectImmediate(oldModel.gameObject);
        }

        // Add movement components if requested and not building
        if (addMovementComponents && baseUnit != null && !baseUnit.IsBuilding)
        {
            if (controllable == null)
            {
                Controllable controllableComp = selectedUnit.AddComponent<Controllable>();
                SerializedObject serializedControllable = new SerializedObject(controllableComp);
                serializedControllable.FindProperty("moveSpeed").floatValue = 5f;
                serializedControllable.ApplyModifiedProperties();
            }

            if (navAgent == null)
            {
                NavMeshAgent navAgentComp = selectedUnit.AddComponent<NavMeshAgent>();
                navAgentComp.speed = 5f;
                navAgentComp.angularSpeed = 120f;
                navAgentComp.acceleration = 8f;
                navAgentComp.stoppingDistance = 0.5f;
            }

            // Add Rigidbody if not present
            if (selectedUnit.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = selectedUnit.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        // Instantiate new model
        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(importedModel, selectedUnit.transform);
        modelInstance.name = "Model";
        Undo.RegisterCreatedObjectUndo(modelInstance, "Add Model");

        // Apply weapon configuration if requested
        if (addWeaponController || weaponPoints.Count > 0)
        {
            ApplyWeaponConfiguration(selectedUnit, modelInstance.transform);
        }

        // Apply selection indicator
        if (selectionIndicatorTransform != null && baseUnit != null)
        {
            Transform indicatorInInstance = FindTransformInHierarchy(modelInstance.transform, selectionIndicatorTransform.name);
            if (indicatorInInstance != null)
            {
                SerializedObject serializedBaseUnit = new SerializedObject(baseUnit);
                serializedBaseUnit.FindProperty("selectionIndicator").objectReferenceValue = indicatorInInstance.gameObject;
                serializedBaseUnit.ApplyModifiedProperties();
            }
        }

        // Reload components
        LoadComponents();
        EditorUtility.SetDirty(selectedUnit);

        string configSummary = "Configuration Applied:\n" +
 $"• Weapon Points: {weaponPoints.Count}\n" +
  $"• Turrets: {turretTransforms.Count}\n" +
  $"• Movement Added: {(addMovementComponents ? "Yes" : "No")}\n" +
  $"• Weapons: {(addWeaponController ? "Yes" : "No")}";

        EditorUtility.DisplayDialog("Success",
            configSummary,
              "OK");
    }

    // All remaining Draw methods and helper functions continue here...
    void DrawPreviewSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Preview", subHeaderStyle);

        if (gameObjectEditor != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(200, 200), EditorStyles.helpBox);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    void DrawQuickActions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Component Management", EditorStyles.miniBoldLabel);

        GUILayout.BeginHorizontal();

        if (!baseUnit.IsBuilding)
        {
            if (controllable == null)
            {
                if (GUILayout.Button("Add Controllable", GUILayout.Height(25)))
                {
                    AddControllable();
                }
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove Controllable", GUILayout.Height(25)))
                {
                    RemoveControllable();
                }
                GUI.backgroundColor = Color.white;
            }

            if (navAgent == null)
            {
                if (GUILayout.Button("Add NavMeshAgent", GUILayout.Height(25)))
                {
                    AddNavMeshAgent();
                }
            }
            else
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove NavMeshAgent", GUILayout.Height(25)))
                {
                    RemoveNavMeshAgent();
                }
                GUI.backgroundColor = Color.white;
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (teamComponent == null)
        {
            if (GUILayout.Button("Add TeamComponent", GUILayout.Height(25)))
            {
                AddTeamComponent();
            }
        }

        if (health == null)
        {
            if (GUILayout.Button("Add Health", GUILayout.Height(25)))
            {
                AddHealth();
            }
        }
        else
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Health", GUILayout.Height(25)))
            {
                RemoveHealth();
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (weaponController == null)
        {
            if (GUILayout.Button("Add WeaponController", GUILayout.Height(25)))
            {
                AddWeaponController();
            }
        }
        else
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove WeaponController", GUILayout.Height(25)))
            {
                RemoveWeaponController();
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Setup Templates", EditorStyles.miniBoldLabel);

        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("Setup Complete Unit", GUILayout.Height(30)))
        {
            SetupCompleteUnit();
        }

        if (GUILayout.Button("Setup Combat Unit", GUILayout.Height(30)))
        {
            SetupCombatUnit();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void DrawBaseUnitSection()
    {
        if (baseUnit == null) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SerializedObject serializedUnit = new SerializedObject(baseUnit);

        EditorGUILayout.PropertyField(serializedUnit.FindProperty("unitName"));
        EditorGUILayout.PropertyField(serializedUnit.FindProperty("isBuilding"));
        EditorGUILayout.PropertyField(serializedUnit.FindProperty("selectionIndicator"));
        EditorGUILayout.PropertyField(serializedUnit.FindProperty("selectedColor"));
        EditorGUILayout.PropertyField(serializedUnit.FindProperty("normalColor"));

        serializedUnit.ApplyModifiedProperties();

        EditorGUILayout.EndVertical();
    }

    void DrawTeamSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (teamComponent == null)
        {
            EditorGUILayout.HelpBox("No TeamComponent found. Add one using Quick Actions.", MessageType.Warning);
        }
        else
        {
            SerializedObject serializedTeam = new SerializedObject(teamComponent);

            EditorGUILayout.PropertyField(serializedTeam.FindProperty("team"));
            EditorGUILayout.PropertyField(serializedTeam.FindProperty("teamColor"));

            serializedTeam.ApplyModifiedProperties();
        }

        EditorGUILayout.EndVertical();
    }

    void DrawHealthSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (health == null)
        {
            EditorGUILayout.HelpBox("No Health component found. Add one using Quick Actions.", MessageType.Warning);
        }
        else
        {
            SerializedObject serializedHealth = new SerializedObject(health);

            EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("maxHealth"));
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("canRegenerate"));

            if (serializedHealth.FindProperty("canRegenerate").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedHealth.FindProperty("regenerationRate"));
                EditorGUILayout.PropertyField(serializedHealth.FindProperty("regenerationDelay"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Death Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("destroyOnDeath"));

            if (serializedHealth.FindProperty("destroyOnDeath").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedHealth.FindProperty("destroyDelay"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedHealth.FindProperty("explosionPrefab"));
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("explosionScale"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("hitSounds"), true);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("deathSounds"), true);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("audioVolume"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("flashOnHit"));

            if (serializedHealth.FindProperty("flashOnHit").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedHealth.FindProperty("hitFlashColor"));
                EditorGUILayout.PropertyField(serializedHealth.FindProperty("hitFlashDuration"));
                EditorGUI.indentLevel--;
            }

            serializedHealth.ApplyModifiedProperties();
        }

        EditorGUILayout.EndVertical();
    }

    void DrawControllableSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (baseUnit.IsBuilding)
        {
            EditorGUILayout.HelpBox("Buildings don't need Controllable component.", MessageType.Info);
        }
        else if (controllable == null)
        {
            EditorGUILayout.HelpBox("No Controllable component found. Add one using Quick Actions.", MessageType.Warning);
        }
        else
        {
            SerializedObject serializedControllable = new SerializedObject(controllable);

            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("moveSpeed"));
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("rotationSpeed"));
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("stoppingDistance"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("useNavMesh"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("moveTargetIndicator"));
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("indicatorLifetime"));
            EditorGUILayout.PropertyField(serializedControllable.FindProperty("showPath"));

            if (serializedControllable.FindProperty("showPath").boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedControllable.FindProperty("pathColor"));
                EditorGUI.indentLevel--;
            }

            serializedControllable.ApplyModifiedProperties();
        }

        EditorGUILayout.EndVertical();
    }

    void DrawNavMeshSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (baseUnit.IsBuilding)
        {
            EditorGUILayout.HelpBox("Buildings don't need NavMeshAgent.", MessageType.Info);
        }
        else if (navAgent == null)
        {
            EditorGUILayout.HelpBox("No NavMeshAgent found. Add one using Quick Actions.", MessageType.Warning);
        }
        else
        {
            SerializedObject serializedAgent = new SerializedObject(navAgent);

            EditorGUILayout.LabelField("Agent Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_Speed"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_AngularSpeed"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_Acceleration"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_StoppingDistance"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_AutoBraking"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Avoidance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_Radius"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_Height"));
            EditorGUILayout.PropertyField(serializedAgent.FindProperty("m_BaseOffset"));

            serializedAgent.ApplyModifiedProperties();
        }

        EditorGUILayout.EndVertical();
    }

    void DrawWeaponSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (weaponController == null)
        {
            EditorGUILayout.HelpBox("No WeaponController found. Add one using Quick Actions to make this unit combat-capable.", MessageType.Info);
        }
        else
        {
            SerializedObject serializedWeapons = new SerializedObject(weaponController);

            EditorGUILayout.LabelField("Weapons", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("weapons"), true);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Targeting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("autoAcquireTargets"));
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("targetScanInterval"));
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("targetLayerMask"));
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("prioritizeClosestTarget"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Firing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("autoFire"));
            EditorGUILayout.PropertyField(serializedWeapons.FindProperty("fireInterval"));

            serializedWeapons.ApplyModifiedProperties();

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Find Weapons in Children", GUILayout.Height(25)))
            {
                FindWeaponsInChildren();
            }
        }

        EditorGUILayout.EndVertical();
    }

    void DrawAudioSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Audio Manager", EditorStyles.boldLabel);

        if (AudioManager.Instance == null)
        {
            EditorGUILayout.HelpBox("No AudioManager found in scene. Create one to manage mixer groups.", MessageType.Warning);
            if (GUILayout.Button("Create AudioManager", GUILayout.Height(25)))
            {
                CreateAudioManager();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("AudioManager found. MixerGroups will be assigned automatically.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("AudioSources on Unit", EditorStyles.boldLabel);
        AudioSource[] audioSources = selectedUnit.GetComponents<AudioSource>();

        if (audioSources.Length == 0)
        {
            EditorGUILayout.HelpBox("No AudioSources on this unit.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField($"AudioSource {i + 1}", GUILayout.Width(100));

                string groupName = audioSources[i].outputAudioMixerGroup != null
              ? audioSources[i].outputAudioMixerGroup.name
                    : "None";
                EditorGUILayout.LabelField(groupName, GUILayout.Width(150));

                if (AudioManager.Instance != null && GUILayout.Button("Assign UnitSounds", GUILayout.Width(120)))
                {
                    Undo.RecordObject(audioSources[i], "Assign MixerGroup");
                    AudioManager.Instance.SetupAudioSource(audioSources[i], AudioManager.AudioCategory.UnitSounds);
                    EditorUtility.SetDirty(audioSources[i]);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(10);

        if (health != null)
        {
            SerializedObject serializedHealth = new SerializedObject(health);

            EditorGUILayout.LabelField("Unit Sound Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("hitSounds"), true);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("deathSounds"), true);
            EditorGUILayout.PropertyField(serializedHealth.FindProperty("audioVolume"));

            serializedHealth.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.HelpBox("Add Health component to configure unit sounds.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    void DrawCreateNewUnitSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("Create New Unit From Scratch", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Basic Information", EditorStyles.miniBoldLabel);
        newUnitName = EditorGUILayout.TextField("Unit Name", newUnitName);
        newUnitType = (UnitType)EditorGUILayout.EnumPopup("Unit Type", newUnitType);
        newUnitTeam = (Team)EditorGUILayout.EnumPopup("Team", newUnitTeam);
        newUnitIsBuilding = EditorGUILayout.Toggle("Is Building", newUnitIsBuilding);

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Visual Settings", EditorStyles.miniBoldLabel);
        createWithVisuals = EditorGUILayout.Toggle("Create Visual Model", createWithVisuals);

        if (createWithVisuals)
        {
            EditorGUI.indentLevel++;
            newUnitMeshType = (PrimitiveType)EditorGUILayout.EnumPopup("Mesh Type", newUnitMeshType);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Template Info:", EditorStyles.miniBoldLabel);
        string templateInfo = GetTemplateDescription(newUnitType);
        EditorGUILayout.HelpBox(templateInfo, MessageType.Info);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("CREATE UNIT", GUILayout.Height(40)))
        {
            CreateNewUnit();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("This will create a new GameObject with all necessary components based on the selected type.", MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    void DrawTransformHierarchy(Transform transform, int indentLevel)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(indentLevel * 20);

        EditorGUILayout.LabelField(transform.name, GUILayout.Width(150));

        if (GUILayout.Button("Weapon", GUILayout.Width(70)))
        {
            if (!weaponPoints.Contains(transform))
            {
                weaponPoints.Add(transform);
            }
        }

        if (GUILayout.Button("Turret", GUILayout.Width(70)))
        {
            if (!turretTransforms.Contains(transform))
            {
                turretTransforms.Add(transform);
            }
        }

        if (GUILayout.Button("Indicator", GUILayout.Width(70)))
        {
            selectionIndicatorTransform = transform;
        }

        string status = "";
        if (weaponPoints.Contains(transform)) status = "[W]";
        if (turretTransforms.Contains(transform)) status += "[T]";
        if (selectionIndicatorTransform == transform) status += "[I]";

        if (!string.IsNullOrEmpty(status))
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField(status, GUILayout.Width(50));
            GUI.color = Color.white;
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < transform.childCount; i++)
        {
            DrawTransformHierarchy(transform.GetChild(i), indentLevel + 1);
        }
    }

    void ScanModelTransforms()
    {
        if (importedModel == null) return;

        ClearAllAssignments();

        AutoDetectWeaponPoints();
        AutoDetectTurrets();
        AutoDetectSelectionIndicator();
    }

    void AutoDetectWeaponPoints()
    {
        if (importedModel == null) return;

        weaponPoints.Clear();

        Transform[] allTransforms = importedModel.GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            string nameLower = t.name.ToLower();
            if (nameLower.Contains("weapon") ||
           nameLower.Contains("gun") ||
     nameLower.Contains("muzzle") ||
    nameLower.Contains("firepoint") ||
          nameLower.Contains("shotpoint"))
            {
                if (!weaponPoints.Contains(t))
                {
                    weaponPoints.Add(t);
                }
            }
        }

        Debug.Log($"Auto-detected {weaponPoints.Count} weapon points");
    }

    void AutoDetectTurrets()
    {
        if (importedModel == null) return;

        turretTransforms.Clear();

        Transform[] allTransforms = importedModel.GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            string nameLower = t.name.ToLower();
            if (nameLower.Contains("turret") ||
         nameLower.Contains("barrel") ||
              nameLower.Contains("cannon") ||
       nameLower.Contains("rotating"))
            {
                if (!turretTransforms.Contains(t))
                {
                    turretTransforms.Add(t);
                }
            }
        }

        Debug.Log($"Auto-detected {turretTransforms.Count} turrets");
    }

    void AutoDetectSelectionIndicator()
    {
        if (importedModel == null) return;

        Transform[] allTransforms = importedModel.GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            string nameLower = t.name.ToLower();
            if (nameLower.Contains("indicator") ||
                nameLower.Contains("selection") ||
        nameLower.Contains("highlight"))
            {
                selectionIndicatorTransform = t;
                Debug.Log($"Auto-detected selection indicator: {t.name}");
                return;
            }
        }
    }

    void ClearAllAssignments()
    {
        weaponPoints.Clear();
        turretTransforms.Clear();
        selectionIndicatorTransform = null;
    }

    void ApplyWeaponConfiguration(GameObject unit, Transform modelRoot)
    {
        if (weaponPoints.Count == 0 && !addWeaponController)
        {
            return;
        }

        WeaponController wc = unit.GetComponent<WeaponController>();
        if (wc == null)
        {
            wc = unit.AddComponent<WeaponController>();
        }

        if (weaponPoints.Count == 0)
        {
            return;
        }

        List<Weapon> createdWeapons = new List<Weapon>();

        foreach (Transform weaponPointRef in weaponPoints)
        {
            Transform weaponPointInInstance = FindTransformInHierarchy(modelRoot, weaponPointRef.name);

            if (weaponPointInInstance == null)
            {
                Debug.LogWarning($"Could not find weapon point '{weaponPointRef.name}' in instantiated model");
                continue;
            }

            GameObject weaponObj = new GameObject($"Weapon_{weaponPointInInstance.name}");
            weaponObj.transform.SetParent(unit.transform);
            weaponObj.transform.position = weaponPointInInstance.position;
            weaponObj.transform.rotation = weaponPointInInstance.rotation;

            Weapon weapon = weaponObj.AddComponent<Weapon>();

            if (turretTransforms.Count > 0)
            {
                Transform turretRef = turretTransforms[0];
                Transform turretInInstance = FindTransformInHierarchy(modelRoot, turretRef.name);

                if (turretInInstance != null)
                {
                    SerializedObject serializedWeapon = new SerializedObject(weapon);
                    serializedWeapon.FindProperty("turretTransform").objectReferenceValue = turretInInstance;
                    serializedWeapon.ApplyModifiedProperties();
                }
            }

            SerializedObject so = new SerializedObject(weapon);
            SerializedProperty shotPointsArray = so.FindProperty("shotPoints");
            shotPointsArray.arraySize = 1;
            shotPointsArray.GetArrayElementAtIndex(0).objectReferenceValue = weaponPointInInstance;
            so.ApplyModifiedProperties();

            createdWeapons.Add(weapon);
        }

        if (createdWeapons.Count > 0)
        {
            SerializedObject serializedWC = new SerializedObject(wc);
            SerializedProperty weaponsArray = serializedWC.FindProperty("weapons");
            weaponsArray.arraySize = createdWeapons.Count;
            for (int i = 0; i < createdWeapons.Count; i++)
            {
                weaponsArray.GetArrayElementAtIndex(i).objectReferenceValue = createdWeapons[i];
            }
            serializedWC.ApplyModifiedProperties();
        }
    }

    Transform FindTransformInHierarchy(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform result = FindTransformInHierarchy(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    void AddApproximateCollider(GameObject unit)
    {
        Renderer[] renderers = unit.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            BoxCollider collider = unit.AddComponent<BoxCollider>();
            collider.center = bounds.center - unit.transform.position;
            collider.size = bounds.size;
        }
        else
        {
            BoxCollider collider = unit.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 2f, 2f);
        }
    }

    string GetTemplateDescription(UnitType type)
    {
        switch (type)
        {
            case UnitType.Infantry:
                return "Infantry Unit:\n• BaseUnit\n• TeamComponent\n• Health (100 HP)\n• Controllable (Speed: 5)\n• NavMeshAgent\n• Collider & Rigidbody";

            case UnitType.Vehicle:
                return "Vehicle Unit:\n• BaseUnit\n• TeamComponent\n• Health (200 HP)\n• Controllable (Speed: 8)\n• NavMeshAgent\n• Collider & Rigidbody";

            case UnitType.Archer:
                return "Archer Unit:\n• BaseUnit\n• TeamComponent\n• Health (75 HP)\n• Controllable (Speed: 4)\n• NavMeshAgent\n• WeaponController\n• Collider & Rigidbody";

            case UnitType.Building:
                return "Building:\n• BaseUnit (IsBuilding: true)\n• TeamComponent\n• Health (500 HP)\n• Collider\n• No movement components";

            case UnitType.Worker:
                return "Worker Unit:\n• BaseUnit\n• TeamComponent\n• Health (50 HP)\n• Controllable (Speed: 4.5)\n• NavMeshAgent\n• Collider & Rigidbody\n• No weapons";

            case UnitType.Custom:
                return "Custom Unit:\n• BaseUnit\n• TeamComponent\n• Health (100 HP)\n• You can add other components manually";

            default:
                return "Unknown type";
        }
    }

    void CreateNewUnit()
    {
        if (string.IsNullOrEmpty(newUnitName))
        {
            EditorUtility.DisplayDialog("Invalid Name", "Please enter a valid unit name.", "OK");
            return;
        }

        GameObject newUnit = new GameObject(newUnitName);
        Undo.RegisterCreatedObjectUndo(newUnit, "Create New Unit");

        BaseUnit baseUnitComp = newUnit.AddComponent<BaseUnit>();
        SerializedObject serializedBaseUnit = new SerializedObject(baseUnitComp);
        serializedBaseUnit.FindProperty("unitName").stringValue = newUnitName;
        serializedBaseUnit.FindProperty("isBuilding").boolValue = newUnitIsBuilding;
        serializedBaseUnit.ApplyModifiedProperties();

        TeamComponent teamComp = newUnit.AddComponent<TeamComponent>();
        SerializedObject serializedTeam = new SerializedObject(teamComp);
        serializedTeam.FindProperty("team").enumValueIndex = (int)newUnitTeam;
        serializedTeam.ApplyModifiedProperties();

        Health healthComp = newUnit.AddComponent<Health>();
        float maxHealth = GetHealthForType(newUnitType);
        SerializedObject serializedHealth = new SerializedObject(healthComp);
        serializedHealth.FindProperty("maxHealth").floatValue = maxHealth;
        serializedHealth.ApplyModifiedProperties();

        if (!newUnitIsBuilding)
        {
            Controllable controllableComp = newUnit.AddComponent<Controllable>();
            float moveSpeed = GetMoveSpeedForType(newUnitType);
            SerializedObject serializedControllable = new SerializedObject(controllableComp);
            serializedControllable.FindProperty("moveSpeed").floatValue = moveSpeed;
            serializedControllable.ApplyModifiedProperties();

            NavMeshAgent navAgentComp = newUnit.AddComponent<NavMeshAgent>();
            navAgentComp.speed = moveSpeed;
            navAgentComp.angularSpeed = 120f;
            navAgentComp.acceleration = 8f;
            navAgentComp.stoppingDistance = 0.5f;
        }

        if (newUnitType == UnitType.Archer)
        {
            newUnit.AddComponent<WeaponController>();
        }

        if (createWithVisuals)
        {
            CreateUnitVisuals(newUnit, newUnitType);
        }

        if (newUnitIsBuilding)
        {
            BoxCollider collider = newUnit.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 3f, 3f);
        }
        else
        {
            CapsuleCollider collider = newUnit.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = new Vector3(0, 1f, 0);
        }

        if (!newUnitIsBuilding)
        {
            Rigidbody rb = newUnit.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (AudioManager.Instance != null)
        {
            AudioSource audioSource = AudioManager.Instance.CreateAudioSource(
              newUnit,
        AudioManager.AudioCategory.UnitSounds,
                 false
     );
        }

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            newUnit.transform.position = sceneView.pivot;
        }

        Selection.activeGameObject = newUnit;
        EditorGUIUtility.PingObject(newUnit);

        TryLoadUnit(newUnit);

        EditorUtility.DisplayDialog("Unit Created",
            $"'{newUnitName}' has been created successfully!\n\n" +
            $"Type: {newUnitType}\n" +
            $"Team: {newUnitTeam}\n" +
   $"Health: {maxHealth} HP\n\n" +
         (newUnitType == UnitType.Archer ? "Don't forget to add Weapon components!" : ""),
  "OK");
    }

    void CreateUnitVisuals(GameObject parent, UnitType type)
    {
        GameObject visualRoot = new GameObject("Visuals");
        visualRoot.transform.SetParent(parent.transform);
        visualRoot.transform.localPosition = Vector3.zero;

        GameObject mesh;

        switch (type)
        {
            case UnitType.Infantry:
                mesh = GameObject.CreatePrimitive(newUnitMeshType);
                mesh.name = "Body";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 1f, 0);
                mesh.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                DestroyImmediate(mesh.GetComponent<Collider>());

                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(visualRoot.transform);
                head.transform.localPosition = new Vector3(0, 1.75f, 0);
                head.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                DestroyImmediate(head.GetComponent<Collider>());
                break;

            case UnitType.Vehicle:
                mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh.name = "Body";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 0.5f, 0);
                mesh.transform.localScale = new Vector3(1.5f, 0.8f, 2f);
                DestroyImmediate(mesh.GetComponent<Collider>());

                GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                turret.name = "Turret";
                turret.transform.SetParent(visualRoot.transform);
                turret.transform.localPosition = new Vector3(0, 1f, 0);
                turret.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
                DestroyImmediate(turret.GetComponent<Collider>());
                break;

            case UnitType.Archer:
                mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mesh.name = "Body";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 1f, 0);
                mesh.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
                DestroyImmediate(mesh.GetComponent<Collider>());

                GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bow.name = "Bow";
                bow.transform.SetParent(visualRoot.transform);
                bow.transform.localPosition = new Vector3(0.3f, 1.2f, 0);
                bow.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                DestroyImmediate(bow.GetComponent<Collider>());
                break;

            case UnitType.Building:
                mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh.name = "Base";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 1.5f, 0);
                mesh.transform.localScale = new Vector3(3f, 3f, 3f);
                DestroyImmediate(mesh.GetComponent<Collider>());

                GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Roof";
                roof.transform.SetParent(visualRoot.transform);
                roof.transform.localPosition = new Vector3(0, 3.2f, 0);
                roof.transform.localScale = new Vector3(3.2f, 0.3f, 3.2f);
                DestroyImmediate(roof.GetComponent<Collider>());
                break;

            case UnitType.Worker:
                mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mesh.name = "Body";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 1f, 0);
                mesh.transform.localScale = new Vector3(0.45f, 0.9f, 0.45f);
                DestroyImmediate(mesh.GetComponent<Collider>());

                GameObject tool = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tool.name = "Tool";
                tool.transform.SetParent(visualRoot.transform);
                tool.transform.localPosition = new Vector3(0.4f, 1f, 0);
                tool.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
                DestroyImmediate(tool.GetComponent<Collider>());
                break;

            default:
                mesh = GameObject.CreatePrimitive(newUnitMeshType);
                mesh.name = "Body";
                mesh.transform.SetParent(visualRoot.transform);
                mesh.transform.localPosition = new Vector3(0, 1f, 0);
                DestroyImmediate(mesh.GetComponent<Collider>());
                break;
        }

        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "SelectionIndicator";
        indicator.transform.SetParent(visualRoot.transform);
        indicator.transform.localPosition = new Vector3(0, 0.05f, 0);
        indicator.transform.localScale = new Vector3(1f, 0.05f, 1f);
        indicator.transform.localRotation = Quaternion.Euler(90, 0, 0);
        DestroyImmediate(indicator.GetComponent<Collider>());

        Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 1f, 0f, 0.5f);
            indicatorRenderer.material = mat;
        }

        SerializedObject serializedBaseUnit = new SerializedObject(parent.GetComponent<BaseUnit>());
        serializedBaseUnit.FindProperty("selectionIndicator").objectReferenceValue = indicator;
        serializedBaseUnit.ApplyModifiedProperties();

        indicator.SetActive(false);
    }

    float GetHealthForType(UnitType type)
    {
        switch (type)
        {
            case UnitType.Infantry: return 100f;
            case UnitType.Vehicle: return 200f;
            case UnitType.Archer: return 75f;
            case UnitType.Building: return 500f;
            case UnitType.Worker: return 50f;
            case UnitType.Custom: return 100f;
            default: return 100f;
        }
    }

    float GetMoveSpeedForType(UnitType type)
    {
        switch (type)
        {
            case UnitType.Infantry: return 5f;
            case UnitType.Vehicle: return 8f;
            case UnitType.Archer: return 4f;
            case UnitType.Building: return 0f;
            case UnitType.Worker: return 4.5f;
            case UnitType.Custom: return 5f;
            default: return 5f;
        }
    }

    void AddControllable()
    {
        if (selectedUnit != null && controllable == null)
        {
            Undo.RecordObject(selectedUnit, "Add Controllable");
            controllable = selectedUnit.AddComponent<Controllable>();
            EditorUtility.SetDirty(selectedUnit);
            LoadComponents();
        }
    }

    void RemoveControllable()
    {
        if (controllable != null)
        {
            Undo.DestroyObjectImmediate(controllable);
            controllable = null;
            LoadComponents();
        }
    }

    void AddNavMeshAgent()
    {
        if (selectedUnit != null && navAgent == null)
        {
            Undo.RecordObject(selectedUnit, "Add NavMeshAgent");
            navAgent = selectedUnit.AddComponent<NavMeshAgent>();

            navAgent.speed = 5f;
            navAgent.angularSpeed = 120f;
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = 0.5f;

            EditorUtility.SetDirty(selectedUnit);
            LoadComponents();
        }
    }

    void RemoveNavMeshAgent()
    {
        if (navAgent != null)
        {
            Undo.DestroyObjectImmediate(navAgent);
            navAgent = null;
            LoadComponents();
        }
    }

    void AddTeamComponent()
    {
        if (selectedUnit != null && teamComponent == null)
        {
            Undo.RecordObject(selectedUnit, "Add TeamComponent");
            teamComponent = selectedUnit.AddComponent<TeamComponent>();
            EditorUtility.SetDirty(selectedUnit);
            LoadComponents();
        }
    }

    void AddHealth()
    {
        if (selectedUnit != null && health == null)
        {
            Undo.RecordObject(selectedUnit, "Add Health");
            health = selectedUnit.AddComponent<Health>();
            EditorUtility.SetDirty(selectedUnit);
            LoadComponents();
        }
    }

    void RemoveHealth()
    {
        if (health != null)
        {
            Undo.DestroyObjectImmediate(health);
            health = null;
            LoadComponents();
        }
    }

    void AddWeaponController()
    {
        if (selectedUnit != null && weaponController == null)
        {
            Undo.RecordObject(selectedUnit, "Add WeaponController");
            weaponController = selectedUnit.AddComponent<WeaponController>();
            EditorUtility.SetDirty(selectedUnit);
            LoadComponents();
        }
    }

    void RemoveWeaponController()
    {
        if (weaponController != null)
        {
            Undo.DestroyObjectImmediate(weaponController);
            weaponController = null;
            LoadComponents();
        }
    }

    void FindWeaponsInChildren()
    {
        if (weaponController != null)
        {
            Weapon[] weapons = selectedUnit.GetComponentsInChildren<Weapon>();

            SerializedObject serializedWeapons = new SerializedObject(weaponController);
            SerializedProperty weaponsProperty = serializedWeapons.FindProperty("weapons");

            weaponsProperty.arraySize = weapons.Length;
            for (int i = 0; i < weapons.Length; i++)
            {
                weaponsProperty.GetArrayElementAtIndex(i).objectReferenceValue = weapons[i];
            }

            serializedWeapons.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("Weapons Found", $"Found and assigned {weapons.Length} weapon(s).", "OK");
        }
    }

    void SetupCompleteUnit()
    {
        if (selectedUnit == null || baseUnit == null) return;

        Undo.RecordObject(selectedUnit, "Setup Complete Unit");

        if (teamComponent == null) AddTeamComponent();
        if (health == null) AddHealth();

        if (!baseUnit.IsBuilding)
        {
            if (controllable == null) AddControllable();
            if (navAgent == null) AddNavMeshAgent();
        }

        EditorUtility.SetDirty(selectedUnit);
        EditorUtility.DisplayDialog("Setup Complete", "Unit has been configured with all essential components.", "OK");
    }

    void SetupCombatUnit()
    {
        if (selectedUnit == null || baseUnit == null) return;

        Undo.RecordObject(selectedUnit, "Setup Combat Unit");

        SetupCompleteUnit();

        if (weaponController == null) AddWeaponController();

        EditorUtility.SetDirty(selectedUnit);
        EditorUtility.DisplayDialog("Setup Complete", "Combat unit has been configured. Don't forget to add Weapon components!", "OK");
    }

    void CreateAudioManager()
    {
        GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();
        Selection.activeGameObject = audioManagerObj;
        EditorGUIUtility.PingObject(audioManagerObj);

        EditorUtility.DisplayDialog("AudioManager Created",
         "AudioManager has been created. Please assign AudioMixer and MixerGroups in the Inspector.",
       "OK");
    }
}
