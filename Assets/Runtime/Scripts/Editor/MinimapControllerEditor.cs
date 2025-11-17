using UnityEngine;
using UnityEditor;
using Harvest.Minimap;

namespace HarvestEditor.Minimap
{
    /// <summary>
    /// Custom editor for MinimapController with setup wizard
    /// </summary>
    [CustomEditor(typeof(MinimapController))]
    public class MinimapControllerEditor : Editor
    {
        private MinimapController controller;

        private SerializedProperty minimapCamera;
        private SerializedProperty cameraHeight;
        private SerializedProperty cameraSize;
        private SerializedProperty minimapRenderTexture;
        private SerializedProperty minimapImage;
        private SerializedProperty minimapContainer;
        private SerializedProperty iconContainer;
        private SerializedProperty worldCenter;
        private SerializedProperty worldSize;
        private SerializedProperty iconPrefab;
        private SerializedProperty autoCreateIcons;
        private SerializedProperty updateInterval;
        private SerializedProperty enableClickNavigation;
        private SerializedProperty mainCamera;
        private SerializedProperty cameraHeightOffset;
        private SerializedProperty showCameraViewIndicator;
        private SerializedProperty cameraViewIndicator;
        private SerializedProperty viewIndicatorColor;

        private bool showSetupWizard = false;
        private bool showReferences = true;
        private bool showWorldBounds = true;
        private bool showIconSettings = true;
        private bool showInteraction = true;
        private bool showCameraView = true;

        private void OnEnable()
        {
            controller = (MinimapController)target;

            // Get serialized properties
            minimapCamera = serializedObject.FindProperty("minimapCamera");
            cameraHeight = serializedObject.FindProperty("cameraHeight");
            cameraSize = serializedObject.FindProperty("cameraSize");
            minimapRenderTexture = serializedObject.FindProperty("minimapRenderTexture");
            minimapImage = serializedObject.FindProperty("minimapImage");
            minimapContainer = serializedObject.FindProperty("minimapContainer");
            iconContainer = serializedObject.FindProperty("iconContainer");
            worldCenter = serializedObject.FindProperty("worldCenter");
            worldSize = serializedObject.FindProperty("worldSize");
            iconPrefab = serializedObject.FindProperty("iconPrefab");
            autoCreateIcons = serializedObject.FindProperty("autoCreateIcons");
            updateInterval = serializedObject.FindProperty("updateInterval");
            enableClickNavigation = serializedObject.FindProperty("enableClickNavigation");
            mainCamera = serializedObject.FindProperty("mainCamera");
            cameraHeightOffset = serializedObject.FindProperty("cameraHeightOffset");
            showCameraViewIndicator = serializedObject.FindProperty("showCameraViewIndicator");
            cameraViewIndicator = serializedObject.FindProperty("cameraViewIndicator");
            viewIndicatorColor = serializedObject.FindProperty("viewIndicatorColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(10);

            if (showSetupWizard)
            {
                DrawSetupWizard();
                EditorGUILayout.Space(10);
            }

            DrawQuickActions();

            EditorGUILayout.Space(10);

            // Foldout sections
            showReferences = EditorGUILayout.BeginFoldoutHeaderGroup(showReferences, "References");
            if (showReferences)
            {
                DrawReferencesSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showWorldBounds = EditorGUILayout.BeginFoldoutHeaderGroup(showWorldBounds, "World Bounds");
            if (showWorldBounds)
            {
                DrawWorldBoundsSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showIconSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showIconSettings, "Icon Management");
            if (showIconSettings)
            {
                DrawIconSettingsSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showInteraction = EditorGUILayout.BeginFoldoutHeaderGroup(showInteraction, "Interaction");
            if (showInteraction)
            {
                DrawInteractionSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            showCameraView = EditorGUILayout.BeginFoldoutHeaderGroup(showCameraView, "Camera View Indicator");
            if (showCameraView)
            {
                DrawCameraViewSection();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Minimap Controller", EditorStyles.boldLabel);
            GUILayout.Label("Manages minimap camera, icons, and interaction", EditorStyles.miniLabel);

            if (GUILayout.Button(showSetupWizard ? "Hide Setup Wizard" : "Show Setup Wizard"))
            {
                showSetupWizard = !showSetupWizard;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSetupWizard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("?? Setup Wizard", EditorStyles.boldLabel);

            bool allReferencesSet = minimapCamera.objectReferenceValue != null &&
                   minimapImage.objectReferenceValue != null &&
                  minimapContainer.objectReferenceValue != null &&
                   iconContainer.objectReferenceValue != null;

            if (!allReferencesSet)
            {
                EditorGUILayout.HelpBox("Some references are missing. Use the setup buttons below to auto-configure.", MessageType.Warning);

                if (GUILayout.Button("Auto Setup All References"))
                {
                    AutoSetupReferences();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("? All references configured!", MessageType.Info);
            }

            EditorGUILayout.Space(5);

            GUILayout.Label("Quick Setup Steps:", EditorStyles.miniLabel);
            GUILayout.Label("1. Click 'Auto Setup All References'", EditorStyles.miniLabel);
            GUILayout.Label("2. Adjust World Bounds to match your terrain", EditorStyles.miniLabel);
            GUILayout.Label("3. Press Play - icons auto-create for TeamComponents", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Icons for All Units"))
            {
                if (Application.isPlaying)
                {
                    controller.AutoCreateIconsForUnits();
                    Debug.Log("Created minimap icons for all units with TeamComponent");
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required",
                    "This action requires Play Mode to be active.", "OK");
                }
            }

            if (GUILayout.Button("Clear All Icons"))
            {
                if (Application.isPlaying)
                {
                    controller.ClearAllIcons();
                    Debug.Log("Cleared all minimap icons");
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required",
                         "This action requires Play Mode to be active.", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Refresh All Icon Colors"))
            {
                if (Application.isPlaying)
                {
                    controller.RefreshAllIcons();
                    Debug.Log("Refreshed all minimap icon colors");
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required",
                       "This action requires Play Mode to be active.", "OK");
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(minimapCamera);
            EditorGUILayout.PropertyField(cameraHeight);
            EditorGUILayout.PropertyField(cameraSize);

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(minimapRenderTexture);
            EditorGUILayout.PropertyField(minimapImage);
            EditorGUILayout.PropertyField(minimapContainer);
            EditorGUILayout.PropertyField(iconContainer);

            EditorGUILayout.EndVertical();
        }

        private void DrawWorldBoundsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(worldCenter);
            EditorGUILayout.PropertyField(worldSize);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Auto-Detect from Terrain"))
            {
                AutoDetectWorldBounds();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawIconSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(iconPrefab);
            EditorGUILayout.PropertyField(autoCreateIcons);
            EditorGUILayout.PropertyField(updateInterval);

            EditorGUILayout.HelpBox(
                "Update Interval:\n" +
     "0 = Every frame (smooth but more CPU)\n" +
           "1-5 = Good balance\n" +
       "10+ = Better performance for many units",
           MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawInteractionSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(enableClickNavigation);
            EditorGUILayout.PropertyField(mainCamera);
            EditorGUILayout.PropertyField(cameraHeightOffset);

            EditorGUILayout.HelpBox(
   "Enable click navigation to move the main camera when clicking on the minimap.",
   MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawCameraViewSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(showCameraViewIndicator);
            EditorGUILayout.PropertyField(cameraViewIndicator);
            EditorGUILayout.PropertyField(viewIndicatorColor);

            EditorGUILayout.HelpBox(
               "Shows the main camera's visible area on the minimap as a semi-transparent box.",
                    MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void AutoSetupReferences()
        {
            // This would need to be implemented based on your scene structure
            EditorUtility.DisplayDialog("Auto Setup",
      "Auto setup is not yet fully implemented. Please use RTSSetupHelper.CreateMinimapSystem() instead.",
      "OK");
        }

        private void AutoDetectWorldBounds()
        {
            Terrain terrain = FindObjectOfType<Terrain>();
            if (terrain != null)
            {
                TerrainData data = terrain.terrainData;
                worldCenter.vector3Value = terrain.transform.position + new Vector3(data.size.x / 2f, 0, data.size.z / 2f);
                worldSize.vector2Value = new Vector2(data.size.x, data.size.z);

                serializedObject.ApplyModifiedProperties();

                Debug.Log($"Auto-detected world bounds from terrain: Center={worldCenter.vector3Value}, Size={worldSize.vector2Value}");
            }
            else
            {
                EditorUtility.DisplayDialog("No Terrain Found",
                       "Could not find a Terrain in the scene. Please set world bounds manually.",
                          "OK");
            }
        }
    }
}
