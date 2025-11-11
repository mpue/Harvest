using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper to quickly setup combat system on units
/// Can be used at runtime or in editor
/// </summary>
public static class CombatSystemHelper
{
    /// <summary>
    /// Add all combat components to a unit
    /// </summary>
    public static void SetupCombatUnit(GameObject unit, float maxHealth = 100f, bool addHealthBar = true)
    {
        if (unit == null)
        {
            Debug.LogError("Cannot setup combat on null unit!");
            return;
        }

        // Add Health if not present
        Health health = unit.GetComponent<Health>();
        if (health == null)
        {
            health = unit.AddComponent<Health>();
            Debug.Log($"Added Health component to {unit.name}");
        }

        // Add HealthBarSetup if requested
        if (addHealthBar)
        {
            HealthBarSetup healthBarSetup = unit.GetComponent<HealthBarSetup>();
            if (healthBarSetup == null)
            {
                healthBarSetup = unit.AddComponent<HealthBarSetup>();
                Debug.Log($"Added HealthBarSetup to {unit.name}");
            }
        }

        Debug.Log($"? Combat System setup complete on {unit.name}");
    }

    /// <summary>
    /// Quick damage test
    /// </summary>
    public static void TestDamage(GameObject unit, float damage = 25f)
    {
        if (unit == null) return;

        Health health = unit.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"Dealt {damage} damage to {unit.name}. Health: {health.CurrentHealth}/{health.MaxHealth}");
        }
        else
        {
            Debug.LogWarning($"{unit.name} has no Health component!");
        }
    }

    /// <summary>
    /// Quick heal test
    /// </summary>
    public static void TestHeal(GameObject unit, float amount = 25f)
    {
        if (unit == null) return;

        Health health = unit.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(amount);
            Debug.Log($"Healed {unit.name} by {amount}. Health: {health.CurrentHealth}/{health.MaxHealth}");
        }
        else
        {
            Debug.LogWarning($"{unit.name} has no Health component!");
        }
    }

    /// <summary>
    /// Batch setup for multiple units
    /// </summary>
    public static void SetupMultipleUnits(GameObject[] units, float maxHealth = 100f, bool addHealthBar = true)
    {
        if (units == null || units.Length == 0)
        {
            Debug.LogWarning("No units to setup!");
            return;
        }

        foreach (GameObject unit in units)
        {
            if (unit != null)
            {
                SetupCombatUnit(unit, maxHealth, addHealthBar);
            }
        }

        Debug.Log($"? Setup combat system on {units.Length} units");
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor window for easy combat system setup
/// </summary>
public class CombatSystemSetupWindow : EditorWindow
{
    private GameObject targetUnit;
    private float maxHealth = 100f;
    private bool addHealthBar = true;

    [MenuItem("Tools/Combat System Setup")]
    public static void ShowWindow()
    {
        GetWindow<CombatSystemSetupWindow>("Combat Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Combat System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetUnit = (GameObject)EditorGUILayout.ObjectField("Target Unit", targetUnit, typeof(GameObject), true);
        maxHealth = EditorGUILayout.FloatField("Max Health", maxHealth);
        addHealthBar = EditorGUILayout.Toggle("Add Health Bar", addHealthBar);

        EditorGUILayout.Space();

        GUI.enabled = targetUnit != null;
        if (GUILayout.Button("Setup Combat System", GUILayout.Height(30)))
        {
            CombatSystemHelper.SetupCombatUnit(targetUnit, maxHealth, addHealthBar);
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        GUILayout.Label("Batch Setup", EditorStyles.boldLabel);
        if (GUILayout.Button("Setup All Selected GameObjects"))
        {
            GameObject[] selected = Selection.gameObjects;
            CombatSystemHelper.SetupMultipleUnits(selected, maxHealth, addHealthBar);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
       "Select one or more GameObjects in the Hierarchy, then click 'Setup All Selected GameObjects' to add combat components to all of them at once.",
         MessageType.Info
             );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        GUILayout.Label("Quick Tests (Play Mode Only)", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying && targetUnit != null;
        if (GUILayout.Button("Test Damage (25)"))
        {
            CombatSystemHelper.TestDamage(targetUnit, 25f);
        }
        if (GUILayout.Button("Test Heal (25)"))
        {
            CombatSystemHelper.TestHeal(targetUnit, 25f);
        }
        if (GUILayout.Button("Test Kill"))
        {
            Health health = targetUnit.GetComponent<Health>();
            if (health != null) health.Kill();
        }
        GUI.enabled = true;
    }
}

/// <summary>
/// Context menu additions for quick setup
/// </summary>
public static class CombatSystemContextMenu
{
    [MenuItem("GameObject/Combat System/Add Combat Components", false, 10)]
    static void AddCombatComponents()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            CombatSystemHelper.SetupCombatUnit(obj);
        }
    }

    [MenuItem("GameObject/Combat System/Add Combat Components", true)]
    static bool ValidateAddCombatComponents()
    {
        return Selection.gameObjects.Length > 0;
    }
}
#endif
