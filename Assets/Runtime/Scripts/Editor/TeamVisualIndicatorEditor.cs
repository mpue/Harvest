using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for TeamVisualIndicator to make configuration easier
/// </summary>
[CustomEditor(typeof(TeamVisualIndicator))]
public class TeamVisualIndicatorEditor : Editor
{
    private SerializedProperty indicatorType;
    private SerializedProperty showIndicator;
    private SerializedProperty indicatorHeightOffset;
    private SerializedProperty indicatorScale;

    private SerializedProperty colorRingPrefab;
    private SerializedProperty ringRotationSpeed;
    private SerializedProperty pulseEffect;
    private SerializedProperty pulseSpeed;
    private SerializedProperty pulseAmount;

    private SerializedProperty shieldIconPrefab;
    private SerializedProperty billboardIcon;

    private SerializedProperty tintMaterials;
    private SerializedProperty tintStrength;

    private SerializedProperty useOutline;
    private SerializedProperty outlineWidth;

    void OnEnable()
    {
        indicatorType = serializedObject.FindProperty("indicatorType");
        showIndicator = serializedObject.FindProperty("showIndicator");
        indicatorHeightOffset = serializedObject.FindProperty("indicatorHeightOffset");
        indicatorScale = serializedObject.FindProperty("indicatorScale");

        colorRingPrefab = serializedObject.FindProperty("colorRingPrefab");
        ringRotationSpeed = serializedObject.FindProperty("ringRotationSpeed");
        pulseEffect = serializedObject.FindProperty("pulseEffect");
        pulseSpeed = serializedObject.FindProperty("pulseSpeed");
        pulseAmount = serializedObject.FindProperty("pulseAmount");

        shieldIconPrefab = serializedObject.FindProperty("shieldIconPrefab");
        billboardIcon = serializedObject.FindProperty("billboardIcon");

        tintMaterials = serializedObject.FindProperty("tintMaterials");
        tintStrength = serializedObject.FindProperty("tintStrength");

        useOutline = serializedObject.FindProperty("useOutline");
        outlineWidth = serializedObject.FindProperty("outlineWidth");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TeamVisualIndicator indicator = (TeamVisualIndicator)target;
        TeamComponent teamComponent = indicator.GetComponent<TeamComponent>();

        // Warning if no team component
        if (teamComponent == null)
        {
            EditorGUILayout.HelpBox("TeamVisualIndicator requires a TeamComponent to function!", MessageType.Error);
            if (GUILayout.Button("Add TeamComponent"))
            {
                indicator.gameObject.AddComponent<TeamComponent>();
            }
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Team Visual Indicator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Zeigt die Team-Zugehörigkeit durch visuelle Marker an", MessageType.Info);

        EditorGUILayout.Space();

        // Current team info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Aktuelles Team", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Team:", teamComponent.CurrentTeam.ToString());
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ColorField("Team Farbe:", teamComponent.TeamColor);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // General settings
        EditorGUILayout.LabelField("Allgemeine Einstellungen", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showIndicator, new GUIContent("Indikator Anzeigen"));
        EditorGUILayout.PropertyField(indicatorType, new GUIContent("Indikator Typ"));
        EditorGUILayout.PropertyField(indicatorHeightOffset, new GUIContent("Höhen-Offset"));
        EditorGUILayout.PropertyField(indicatorScale, new GUIContent("Skalierung"));

        EditorGUILayout.Space();

        var typeValue = (TeamVisualIndicator.IndicatorType)indicatorType.enumValueIndex;

        // Type-specific settings
        if (typeValue == TeamVisualIndicator.IndicatorType.ColorRing ||
        typeValue == TeamVisualIndicator.IndicatorType.Combined)
        {
            EditorGUILayout.LabelField("Farb-Ring Einstellungen", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(colorRingPrefab, new GUIContent("Ring Prefab (Optional)"));
            EditorGUILayout.PropertyField(ringRotationSpeed, new GUIContent("Rotations-Geschwindigkeit"));
            EditorGUILayout.PropertyField(pulseEffect, new GUIContent("Puls-Effekt"));
            if (pulseEffect.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(pulseSpeed, new GUIContent("Puls-Geschwindigkeit"));
                EditorGUILayout.PropertyField(pulseAmount, new GUIContent("Puls-Stärke"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }

        if (typeValue == TeamVisualIndicator.IndicatorType.ShieldIcon)
        {
            EditorGUILayout.LabelField("Schild-Icon Einstellungen", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(shieldIconPrefab, new GUIContent("Schild Prefab (Optional)"));
            EditorGUILayout.PropertyField(billboardIcon, new GUIContent("Billboard-Effekt"));
            EditorGUILayout.Space();
        }

        if (typeValue == TeamVisualIndicator.IndicatorType.MaterialTint ||
            typeValue == TeamVisualIndicator.IndicatorType.Combined)
        {
            EditorGUILayout.LabelField("Material-Färbung Einstellungen", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tintMaterials, new GUIContent("Materialien Färben"));
            if (tintMaterials.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(tintStrength, new GUIContent("Färbungs-Stärke"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }

        if (typeValue == TeamVisualIndicator.IndicatorType.Outline)
        {
            EditorGUILayout.LabelField("Outline Einstellungen", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useOutline, new GUIContent("Outline Verwenden"));
            if (useOutline.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(outlineWidth, new GUIContent("Outline-Breite"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
        }

        // Preview buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Vorschau & Test", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Indikator Aktualisieren"))
            {
                indicator.UpdateTeamColor();
            }
            if (GUILayout.Button("Indikator " + (showIndicator.boolValue ? "Verstecken" : "Anzeigen")))
            {
                showIndicator.boolValue = !showIndicator.boolValue;
                indicator.SetIndicatorVisible(showIndicator.boolValue);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Vorschau-Funktionen sind nur im Play-Modus verfügbar", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
