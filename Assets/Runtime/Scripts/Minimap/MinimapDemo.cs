using UnityEngine;
using Harvest.Minimap;

/// <summary>
/// Demo script for Minimap System
/// Shows various minimap features and integrations
/// </summary>
public class MinimapDemo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinimapController minimapController;

    [Header("Demo Settings")]
    [SerializeField] private bool showDemoUI = true;
    [SerializeField] private KeyCode toggleMinimapKey = KeyCode.M;
    [SerializeField] private KeyCode spawnUnitKey = KeyCode.U;
    [SerializeField] private KeyCode cycleTeamKey = KeyCode.T;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private float spawnRadius = 20f;

    [Header("Runtime Info")]
    [SerializeField] private bool minimapVisible = true;
    [SerializeField] private int totalUnitsSpawned = 0;

    private Team currentSpawnTeam = Team.Player;

    private void Start()
    {
        if (minimapController == null)
        {
            minimapController = FindFirstObjectByType<MinimapController>();
        }

        if (minimapController == null)
        {
            Debug.LogWarning("MinimapDemo: No MinimapController found! Create one with RTSSetupHelper.");
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Toggle minimap visibility
        if (Input.GetKeyDown(toggleMinimapKey))
        {
            ToggleMinimapVisibility();
        }

        // Spawn test unit
        if (Input.GetKeyDown(spawnUnitKey))
        {
            SpawnTestUnit();
        }

        // Cycle through teams
        if (Input.GetKeyDown(cycleTeamKey))
        {
            CycleSpawnTeam();
        }
    }

    private void OnGUI()
    {
        if (!showDemoUI)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("box");

        GUILayout.Label("=== MINIMAP DEMO ===", GUI.skin.GetStyle("box"));
        GUILayout.Space(10);

        // Status
        GUILayout.Label($"Minimap Visible: {minimapVisible}");
        GUILayout.Label($"Units Spawned: {totalUnitsSpawned}");
        GUILayout.Label($"Current Team: {currentSpawnTeam}");
        GUILayout.Space(10);

        // Controls
        GUILayout.Label("CONTROLS:", GUI.skin.GetStyle("box"));
        GUILayout.Label($"[{toggleMinimapKey}] Toggle Minimap");
        GUILayout.Label($"[{spawnUnitKey}] Spawn Unit");
        GUILayout.Label($"[{cycleTeamKey}] Cycle Team");
        GUILayout.Label("[Click Minimap] Navigate Camera");
        GUILayout.Space(10);

        // Buttons
        if (GUILayout.Button("Spawn Unit"))
        {
            SpawnTestUnit();
        }

        if (GUILayout.Button("Spawn 5 Units"))
        {
            for (int i = 0; i < 5; i++)
                SpawnTestUnit();
        }

        if (GUILayout.Button("Cycle Team"))
        {
            CycleSpawnTeam();
        }

        if (GUILayout.Button("Toggle Minimap"))
        {
            ToggleMinimapVisibility();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Refresh All Icons"))
        {
            if (minimapController != null)
            {
                minimapController.RefreshAllIcons();
                Debug.Log("Refreshed all minimap icons");
            }
        }

        if (GUILayout.Button("Clear All Icons"))
        {
            if (minimapController != null)
            {
                minimapController.ClearAllIcons();
                totalUnitsSpawned = 0;
                Debug.Log("Cleared all minimap icons");
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    #region Demo Actions

    /// <summary>
    /// Spawn a test unit with team component and minimap integration
    /// </summary>
    public void SpawnTestUnit()
    {
        if (minimapController == null)
        {
            Debug.LogWarning("Cannot spawn unit: No MinimapController found!");
            return;
        }

        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject unit;
        if (unitPrefab != null)
        {
            unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Create simple test unit
            unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unit.transform.position = spawnPos;
            unit.name = $"Unit_{totalUnitsSpawned + 1}";
        }

        // Add TeamComponent
        TeamComponent team = unit.GetComponent<TeamComponent>();
        if (team == null)
        {
            team = unit.AddComponent<TeamComponent>();
        }
        team.SetTeam(currentSpawnTeam);

        // Add MinimapUnit for automatic icon creation
        MinimapUnit minimapUnit = unit.GetComponent<MinimapUnit>();
        if (minimapUnit == null)
        {
            minimapUnit = unit.AddComponent<MinimapUnit>();
        }

        // Set icon shape based on team for demo variety
        MinimapIcon.IconShape shape = GetIconShapeForTeam(currentSpawnTeam);
        minimapUnit.SetIconShape(shape);
        minimapUnit.CreateIcon();

        totalUnitsSpawned++;

        Debug.Log($"Spawned {unit.name} as {currentSpawnTeam} with {shape} icon");
    }

    /// <summary>
    /// Toggle minimap visibility
    /// </summary>
    public void ToggleMinimapVisibility()
    {
        if (minimapController == null)
            return;

        minimapVisible = !minimapVisible;

        // Toggle minimap container
        GameObject minimapContainer = GameObject.Find("MinimapContainer");
        if (minimapContainer != null)
        {
            minimapContainer.SetActive(minimapVisible);
        }

        Debug.Log($"Minimap {(minimapVisible ? "shown" : "hidden")}");
    }

    /// <summary>
    /// Cycle through spawn teams
    /// </summary>
    public void CycleSpawnTeam()
    {
        switch (currentSpawnTeam)
        {
            case Team.Player:
                currentSpawnTeam = Team.Enemy;
                break;
            case Team.Enemy:
                currentSpawnTeam = Team.Ally;
                break;
            case Team.Ally:
                currentSpawnTeam = Team.Neutral;
                break;
            case Team.Neutral:
                currentSpawnTeam = Team.Player;
                break;
        }

        Debug.Log($"Spawn team changed to: {currentSpawnTeam}");
    }

    /// <summary>
    /// Change all units to random teams (demo purpose)
    /// </summary>
    [ContextMenu("Randomize All Teams")]
    public void RandomizeAllTeams()
    {
        TeamComponent[] allTeams = FindObjectsOfType<TeamComponent>();

        foreach (TeamComponent team in allTeams)
        {
            Team randomTeam = (Team)Random.Range(0, 4);
            team.SetTeam(randomTeam);
        }

        if (minimapController != null)
        {
            minimapController.RefreshAllIcons();
        }

        Debug.Log($"Randomized {allTeams.Length} team assignments");
    }

    /// <summary>
    /// Demo: Show all icon shapes
    /// </summary>
    [ContextMenu("Demo All Icon Shapes")]
    public void DemoAllIconShapes()
    {
        if (minimapController == null)
        {
            Debug.LogWarning("No MinimapController found!");
            return;
        }

        Vector3 center = spawnArea != null ? spawnArea.position : Vector3.zero;
        float radius = 15f;
        int shapeCount = System.Enum.GetValues(typeof(MinimapIcon.IconShape)).Length;

        for (int i = 0; i < shapeCount; i++)
        {
            float angle = (360f / shapeCount) * i;
            Vector3 pos = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

            GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unit.transform.position = pos;
            unit.name = $"Shape_Demo_{i}";

            TeamComponent team = unit.AddComponent<TeamComponent>();
            team.SetTeam((Team)(i % 4));

            MinimapUnit minimapUnit = unit.AddComponent<MinimapUnit>();
            minimapUnit.SetIconShape((MinimapIcon.IconShape)i);
            minimapUnit.SetIconSize(12f);
            minimapUnit.CreateIcon();
        }

        Debug.Log($"Created demo units showing all {shapeCount} icon shapes");
    }

    #endregion

    #region Helper Methods

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = spawnArea != null ? spawnArea.position : Vector3.zero;
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return center + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private MinimapIcon.IconShape GetIconShapeForTeam(Team team)
    {
        switch (team)
        {
            case Team.Player:
                return MinimapIcon.IconShape.Circle;
            case Team.Enemy:
                return MinimapIcon.IconShape.Triangle;
            case Team.Ally:
                return MinimapIcon.IconShape.Square;
            case Team.Neutral:
                return MinimapIcon.IconShape.Diamond;
            default:
                return MinimapIcon.IconShape.Circle;
        }
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnArea != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnArea.position, spawnRadius);
        }
    }
#endif

    #endregion
}
