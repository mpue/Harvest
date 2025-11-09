using UnityEngine;

/// <summary>
/// Helper script to quickly setup weapon systems on units
/// </summary>
public class WeaponSetupHelper : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Team unitTeam = Team.Player;

    [Header("Turret Setup")]
    [SerializeField] private bool createTurret = true;
    [SerializeField] private Vector3 turretPosition = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 barrelPosition = new Vector3(0, 0.5f, 1);
    [SerializeField] private Vector3[] shotPointOffsets = new Vector3[] { new Vector3(0, 0, 0.5f) };

    [Header("Weapon Stats")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 30f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float turretRotationSpeed = 90f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private bool createDefaultProjectile = true;

    [ContextMenu("Setup Weapon on Selected Unit")]
    public void SetupWeaponOnSelectedUnit()
    {
#if UNITY_EDITOR
        GameObject selectedUnit = UnityEditor.Selection.activeGameObject;
        if (selectedUnit == null)
        {
            Debug.LogError("No unit selected! Please select a unit in hierarchy.");
            return;
        }

        SetupWeaponSystem(selectedUnit);
#endif
    }

    [ContextMenu("Create Test Combat Unit")]
    public void CreateTestCombatUnit()
    {
        // Create basic unit
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        unit.name = "Combat Unit";
        unit.transform.position = transform.position;

        // Add RTS components
        BaseUnit baseUnit = unit.AddComponent<BaseUnit>();
        TeamComponent team = unit.AddComponent<TeamComponent>();
        team.SetTeam(unitTeam);
        Controllable controllable = unit.AddComponent<Controllable>();

        // Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = unitTeam == Team.Player ? Color.blue : Color.red;
        unit.GetComponent<Renderer>().material = mat;

        // Setup weapon system
        SetupWeaponSystem(unit);

        Debug.Log($"? Created combat unit: {unit.name}");
    }

    [ContextMenu("Create Projectile Prefab")]
    public void CreateProjectilePrefab()
    {
        // Create projectile
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Setup physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SphereCollider collider = projectile.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        // Add Projectile script
        Projectile proj = projectile.AddComponent<Projectile>();

        // Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow);
        projectile.GetComponent<Renderer>().material = mat;

        // Add trail
        TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = new Color(1, 1, 0, 0);

        Debug.Log("? Created projectile! Now save as prefab and assign to weapon.");

        // Select it
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = projectile;
#endif
    }

    private void SetupWeaponSystem(GameObject unit)
    {
        // Add WeaponController if not present
        WeaponController weaponController = unit.GetComponent<WeaponController>();
        if (weaponController == null)
        {
            weaponController = unit.AddComponent<WeaponController>();
        }

        // Create turret hierarchy
        Transform turretTransform = null;
        Transform barrelTransform = null;
        Transform[] shotPoints = null;

        if (createTurret)
        {
            // Create Turret
            GameObject turretObj = new GameObject("Turret");
            turretObj.transform.SetParent(unit.transform);
            turretObj.transform.localPosition = turretPosition;
            turretTransform = turretObj.transform;

            // Create Barrel
            GameObject barrelObj = new GameObject("Barrel");
            barrelObj.transform.SetParent(turretTransform);
            barrelObj.transform.localPosition = barrelPosition;
            barrelTransform = barrelObj.transform;

            // Create Shot Points
            shotPoints = new Transform[shotPointOffsets.Length];
            for (int i = 0; i < shotPointOffsets.Length; i++)
            {
                GameObject shotPointObj = new GameObject($"ShotPoint_{i}");
                shotPointObj.transform.SetParent(barrelTransform);
                shotPointObj.transform.localPosition = shotPointOffsets[i];
                shotPoints[i] = shotPointObj.transform;

                // Visual marker
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Marker";
                marker.transform.SetParent(shotPointObj.transform);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localScale = Vector3.one * 0.1f;
                DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().material.color = Color.yellow;
            }

            // Visual turret base
            GameObject turretVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretVisual.name = "TurretVisual";
            turretVisual.transform.SetParent(turretTransform);
            turretVisual.transform.localPosition = Vector3.zero;
            turretVisual.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);
            DestroyImmediate(turretVisual.GetComponent<Collider>());

            // Visual barrel
            GameObject barrelVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            barrelVisual.name = "BarrelVisual";
            barrelVisual.transform.SetParent(barrelTransform);
            barrelVisual.transform.localPosition = new Vector3(0, 0, 0.5f);
            barrelVisual.transform.localRotation = Quaternion.Euler(90, 0, 0);
            barrelVisual.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            DestroyImmediate(barrelVisual.GetComponent<Collider>());
        }

        // Create Weapon GameObject
        GameObject weaponObj = new GameObject("Weapon");
        weaponObj.transform.SetParent(unit.transform);
        weaponObj.transform.localPosition = Vector3.zero;

        Weapon weapon = weaponObj.AddComponent<Weapon>();

        // Configure weapon settings (via reflection since fields are private)
#if UNITY_EDITOR
        UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(weapon);
        so.FindProperty("weaponName").stringValue = "Main Weapon";
        so.FindProperty("damage").floatValue = damage;
        so.FindProperty("fireRate").floatValue = fireRate;
        so.FindProperty("range").floatValue = range;
        so.FindProperty("projectileSpeed").floatValue = projectileSpeed;
        so.FindProperty("turretTransform").objectReferenceValue = turretTransform;
        so.FindProperty("barrelTransform").objectReferenceValue = barrelTransform;
        so.FindProperty("turretRotationSpeed").floatValue = turretRotationSpeed;

        if (shotPoints != null)
        {
            UnityEditor.SerializedProperty shotPointsProperty = so.FindProperty("shotPoints");
            shotPointsProperty.arraySize = shotPoints.Length;
            for (int i = 0; i < shotPoints.Length; i++)
            {
                shotPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = shotPoints[i];
            }
        }

        if (projectilePrefab != null)
        {
            so.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        }
        else if (createDefaultProjectile)
        {
            Debug.LogWarning("No projectile prefab assigned. Create one with 'Create Projectile Prefab' button.");
        }

        so.ApplyModifiedProperties();
#endif

        Debug.Log($"? Weapon system setup complete on {unit.name}!");
        Debug.Log("?? Next steps:");
        Debug.Log("   1. Create projectile prefab if not done yet");
        Debug.Log("   2. Assign projectile prefab to weapon");
        Debug.Log("   3. Adjust turret/barrel positions");
        Debug.Log("   4. Set target layer mask in WeaponController");
        Debug.Log("   5. Test by placing enemy units nearby!");
    }

    [ContextMenu("Create Enemy Test Unit")]
    public void CreateEnemyTestUnit()
    {
        Team originalTeam = unitTeam;
        unitTeam = Team.Enemy;
        CreateTestCombatUnit();
        unitTeam = originalTeam;
    }

    void OnDrawGizmosSelected()
    {
        // Draw turret position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + turretPosition, 0.3f);

        // Draw barrel position
        Gizmos.color = Color.blue;
        Vector3 barrelWorldPos = transform.position + turretPosition + barrelPosition;
        Gizmos.DrawWireSphere(barrelWorldPos, 0.2f);

        // Draw shot points
        Gizmos.color = Color.yellow;
        foreach (Vector3 offset in shotPointOffsets)
        {
            Vector3 shotPointPos = barrelWorldPos + offset;
            Gizmos.DrawWireSphere(shotPointPos, 0.1f);
            Gizmos.DrawRay(shotPointPos, Vector3.forward * 2f);
        }

        // Draw weapon range
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
