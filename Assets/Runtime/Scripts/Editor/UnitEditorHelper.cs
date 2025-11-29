using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Helper class for Unit Editor Window
/// Provides validation and auto-setup functionality
/// </summary>
public static class UnitEditorHelper
{
    /// <summary>
    /// Validates if a unit has all required components for gameplay
    /// </summary>
    public static UnitValidationResult ValidateUnit(GameObject unit)
    {
        UnitValidationResult result = new UnitValidationResult();

        if (unit == null)
        {
            result.isValid = false;
            result.errors.Add("No unit selected");
            return result;
        }

        // Check required components
        BaseUnit baseUnit = unit.GetComponent<BaseUnit>();
        if (baseUnit == null)
        {
            result.errors.Add("Missing BaseUnit component");
        }

        TeamComponent teamComponent = unit.GetComponent<TeamComponent>();
        if (teamComponent == null)
        {
            result.errors.Add("Missing TeamComponent");
        }

        TeamVisualIndicator teamIndicator = unit.GetComponent<TeamVisualIndicator>();
        if (teamIndicator == null)
        {
            result.warnings.Add("Missing TeamVisualIndicator (recommended)");
        }

        Health health = unit.GetComponent<Health>();
        if (health == null)
        {
            result.errors.Add("Missing Health component");
        }

        bool isBuilding = baseUnit != null && baseUnit.IsBuilding;

        if (!isBuilding)
        {
            Controllable controllable = unit.GetComponent<Controllable>();
            if (controllable == null)
            {
                result.errors.Add("Missing Controllable component (required for non-buildings)");
            }

            NavMeshAgent navAgent = unit.GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                result.warnings.Add("Missing NavMeshAgent (recommended for movement)");
            }
        }

        WeaponController weaponController = unit.GetComponent<WeaponController>();
        if (weaponController == null)
        {
            result.warnings.Add("Missing WeaponController (unit cannot attack)");
        }
        else
        {
            // Check for weapons
            Weapon[] weapons = unit.GetComponentsInChildren<Weapon>();
            if (weapons.Length == 0)
            {
                result.warnings.Add("WeaponController has no Weapon children");
            }
        }

        // Check for collider
        Collider collider = unit.GetComponent<Collider>();
        if (collider == null)
        {
            result.errors.Add("Missing Collider component");
        }

        // Check for rigidbody (if not building)
        if (!isBuilding)
        {
            Rigidbody rb = unit.GetComponent<Rigidbody>();
            if (rb == null)
            {
                result.warnings.Add("Missing Rigidbody (recommended for physics)");
            }
        }

        // Check children
        Transform healthBarTransform = unit.transform.Find("HealthBar");
        if (healthBarTransform == null)
        {
            result.warnings.Add("No HealthBar child found");
        }
        else
        {
            HealthBar healthBar = healthBarTransform.GetComponent<HealthBar>();
            if (healthBar == null)
            {
                result.warnings.Add("HealthBar child has no HealthBar component");
            }
        }

        // Check selection indicator
        if (baseUnit != null)
        {
            // Check using reflection since selectionIndicator is private
            var field = typeof(BaseUnit).GetField("selectionIndicator",
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                GameObject indicator = field.GetValue(baseUnit) as GameObject;
                if (indicator == null)
                {
                    result.warnings.Add("BaseUnit.selectionIndicator not assigned");
                }
            }
        }

        // Layer check
        if (unit.layer == 0)
        {
            result.warnings.Add("Unit is on Default layer (should use Unit/Player/Enemy layer)");
        }

        result.isValid = result.errors.Count == 0;
        return result;
    }

    /// <summary>
    /// Auto-setup a complete playable unit
    /// </summary>
    public static void SetupCompletePlayableUnit(GameObject unit, Team team, bool isBuilding)
    {
        if (unit == null) return;

        Undo.RegisterCompleteObjectUndo(unit, "Setup Complete Unit");

        // 1. Add BaseUnit
        BaseUnit baseUnit = unit.GetComponent<BaseUnit>();
        if (baseUnit == null)
        {
            baseUnit = unit.AddComponent<BaseUnit>();
        }
        SerializedObject soBase = new SerializedObject(baseUnit);
        soBase.FindProperty("unitName").stringValue = unit.name;
        soBase.FindProperty("isBuilding").boolValue = isBuilding;
        soBase.ApplyModifiedProperties();

        // 2. Add TeamComponent
        TeamComponent teamComponent = unit.GetComponent<TeamComponent>();
        if (teamComponent == null)
        {
            teamComponent = unit.AddComponent<TeamComponent>();
        }
        teamComponent.SetTeam(team);
        teamComponent.SetTeamColor(GetTeamColor(team));

        // 3. Add TeamVisualIndicator
        TeamVisualIndicator teamIndicator = unit.GetComponent<TeamVisualIndicator>();
        if (teamIndicator == null)
        {
            teamIndicator = unit.AddComponent<TeamVisualIndicator>();
        }

        // 4. Add Health
        Health health = unit.GetComponent<Health>();
        if (health == null)
        {
            health = unit.AddComponent<Health>();
        }
        SerializedObject soHealth = new SerializedObject(health);
        soHealth.FindProperty("maxHealth").floatValue = isBuilding ? 500f : 100f;
        soHealth.ApplyModifiedProperties();

        // 5. Add movement components (if not building)
        if (!isBuilding)
        {
            Controllable controllable = unit.GetComponent<Controllable>();
            if (controllable == null)
            {
                controllable = unit.AddComponent<Controllable>();
            }
            SerializedObject soControl = new SerializedObject(controllable);
            soControl.FindProperty("moveSpeed").floatValue = 5f;
            soControl.ApplyModifiedProperties();

            NavMeshAgent navAgent = unit.GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = unit.AddComponent<NavMeshAgent>();
                navAgent.speed = 5f;
                navAgent.angularSpeed = 120f;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 0.5f;
            }

            // Add Rigidbody
            Rigidbody rb = unit.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = unit.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        // 6. Add WeaponController
        WeaponController weaponController = unit.GetComponent<WeaponController>();
        if (weaponController == null)
        {
            weaponController = unit.AddComponent<WeaponController>();
        }

        // 7. Add Collider
        if (unit.GetComponent<Collider>() == null)
        {
            if (isBuilding)
            {
                BoxCollider collider = unit.AddComponent<BoxCollider>();
                collider.size = new Vector3(3f, 3f, 3f);
            }
            else
            {
                CapsuleCollider collider = unit.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }
        }

        // 8. Create SelectionIndicator child
        CreateSelectionIndicator(unit, baseUnit);

        // 9. Create HealthBar child
        CreateHealthBar(unit, health);

        // 10. Set Layer
        SetUnitLayer(unit, team);

        EditorUtility.SetDirty(unit);
    }

    /// <summary>
    /// Creates a Weapon child GameObject
    /// </summary>
    public static GameObject CreateWeaponChild(GameObject unit)
    {
        if (unit == null) return null;

        GameObject weaponObj = new GameObject("Weapon");
        weaponObj.transform.SetParent(unit.transform);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        Weapon weapon = weaponObj.AddComponent<Weapon>();

        // Configure weapon
        SerializedObject soWeapon = new SerializedObject(weapon);
        soWeapon.FindProperty("weaponName").stringValue = "Main Gun";
        soWeapon.FindProperty("damage").floatValue = 10f;
        soWeapon.FindProperty("fireRate").floatValue = 1f;
        soWeapon.FindProperty("range").floatValue = 20f;
        soWeapon.FindProperty("projectileSpeed").floatValue = 30f;
        soWeapon.ApplyModifiedProperties();

        // Add to WeaponController
        WeaponController weaponController = unit.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            SerializedObject soController = new SerializedObject(weaponController);
            SerializedProperty weaponsArray = soController.FindProperty("weapons");
            weaponsArray.arraySize++;
            weaponsArray.GetArrayElementAtIndex(weaponsArray.arraySize - 1).objectReferenceValue = weapon;
            soController.ApplyModifiedProperties();
        }

        Undo.RegisterCreatedObjectUndo(weaponObj, "Create Weapon");
        EditorUtility.SetDirty(unit);

        return weaponObj;
    }

    /// <summary>
    /// Creates a SelectionIndicator child
    /// </summary>
    private static void CreateSelectionIndicator(GameObject unit, BaseUnit baseUnit)
    {
        Transform existing = unit.transform.Find("SelectionIndicator");
        if (existing != null) return;

        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "SelectionIndicator";
        indicator.transform.SetParent(unit.transform);
        indicator.transform.localPosition = new Vector3(0, 0.05f, 0);
        indicator.transform.localRotation = Quaternion.Euler(90, 0, 0);
        indicator.transform.localScale = new Vector3(1f, 0.05f, 1f);

        // Remove collider
        Object.DestroyImmediate(indicator.GetComponent<Collider>());

        // Setup material
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 1f, 0f, 0.5f);
            renderer.material = mat;
        }

        // Assign to BaseUnit
        if (baseUnit != null)
        {
            SerializedObject so = new SerializedObject(baseUnit);
            so.FindProperty("selectionIndicator").objectReferenceValue = indicator;
            so.ApplyModifiedProperties();
        }

        indicator.SetActive(false);
        Undo.RegisterCreatedObjectUndo(indicator, "Create SelectionIndicator");
    }

    /// <summary>
    /// Creates a HealthBar child
    /// </summary>
    private static void CreateHealthBar(GameObject unit, Health health)
    {
        Transform existing = unit.transform.Find("HealthBar");
        if (existing != null) return;

        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(unit.transform);
        healthBarObj.transform.localPosition = new Vector3(0, 2f, 0);

        HealthBar healthBar = healthBarObj.AddComponent<HealthBar>();

        // Configure HealthBar
        if (health != null)
        {
            SerializedObject so = new SerializedObject(healthBar);
            so.FindProperty("healthComponent").objectReferenceValue = health;
            so.FindProperty("offset").vector3Value = new Vector3(0, 2f, 0);
            so.ApplyModifiedProperties();
        }

        Undo.RegisterCreatedObjectUndo(healthBarObj, "Create HealthBar");
    }

    /// <summary>
    /// Sets the unit layer based on team
    /// </summary>
    public static void SetUnitLayer(GameObject unit, Team team)
    {
        int layer = 0;

        switch (team)
        {
            case Team.Player:
                layer = LayerMask.NameToLayer("Player");
                if (layer == -1) layer = 10; // Fallback
                break;
            case Team.Enemy:
                layer = LayerMask.NameToLayer("Enemy");
                if (layer == -1) layer = 9; // Fallback
                break;
            case Team.Neutral:
                layer = LayerMask.NameToLayer("Default");
                if (layer == -1) layer = 0;
                break;
        }

        unit.layer = layer;
    }

    /// <summary>
    /// Gets default team color
    /// </summary>
    private static Color GetTeamColor(Team team)
    {
        switch (team)
        {
            case Team.Player:
                return Color.blue;
            case Team.Enemy:
                return Color.red;
            case Team.Neutral:
                return Color.gray;
            case Team.Ally:
                return Color.green;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Add missing components
    /// </summary>
    public static void AddMissingComponents(GameObject unit)
    {
        if (unit == null) return;

        Undo.RegisterCompleteObjectUndo(unit, "Add Missing Components");

        // Add all essential components if missing
        if (unit.GetComponent<BaseUnit>() == null)
            unit.AddComponent<BaseUnit>();

        if (unit.GetComponent<TeamComponent>() == null)
            unit.AddComponent<TeamComponent>();

        if (unit.GetComponent<TeamVisualIndicator>() == null)
            unit.AddComponent<TeamVisualIndicator>();

        if (unit.GetComponent<Health>() == null)
            unit.AddComponent<Health>();

        BaseUnit baseUnit = unit.GetComponent<BaseUnit>();
        if (baseUnit != null && !baseUnit.IsBuilding)
        {
            if (unit.GetComponent<Controllable>() == null)
                unit.AddComponent<Controllable>();

            if (unit.GetComponent<NavMeshAgent>() == null)
                unit.AddComponent<NavMeshAgent>();

            if (unit.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = unit.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        if (unit.GetComponent<WeaponController>() == null)
            unit.AddComponent<WeaponController>();

        if (unit.GetComponent<Collider>() == null)
        {
            if (baseUnit != null && baseUnit.IsBuilding)
            {
                unit.AddComponent<BoxCollider>();
            }
            else
            {
                unit.AddComponent<CapsuleCollider>();
            }
        }

        EditorUtility.SetDirty(unit);
    }
}

/// <summary>
/// Validation result for a unit
/// </summary>
public class UnitValidationResult
{
    public bool isValid = true;
    public List<string> errors = new List<string>();
    public List<string> warnings = new List<string>();

    public string GetSummary()
    {
        string summary = "";

        if (isValid)
        {
            summary = "✓ Unit is valid for gameplay!\n\n";
        }
        else
        {
            summary = "✗ Unit has critical errors:\n\n";
        }

        if (errors.Count > 0)
        {
            summary += "ERRORS:\n";
            foreach (string error in errors)
            {
                summary += $"  ✗ {error}\n";
            }
            summary += "\n";
        }

        if (warnings.Count > 0)
        {
            summary += "WARNINGS:\n";
            foreach (string warning in warnings)
            {
                summary += $"  ⚠ {warning}\n";
            }
        }

        return summary;
    }
}
