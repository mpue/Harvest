using UnityEngine;

/// <summary>
/// Example script demonstrating team visual indicator setup
/// Attach to a GameObject in scene to test different configurations
/// </summary>
public class TeamVisualIndicatorDemo : MonoBehaviour
{
    [Header("Demo Units")]
    [SerializeField] private GameObject[] playerUnits;
[SerializeField] private GameObject[] enemyUnits;
    [SerializeField] private GameObject[] allyUnits;
    [SerializeField] private GameObject[] neutralUnits;

    [Header("Demo Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private TeamVisualIndicator.IndicatorType demoIndicatorType = TeamVisualIndicator.IndicatorType.ColorRing;

    [Header("Runtime Controls")]
    [SerializeField] private KeyCode toggleIndicatorsKey = KeyCode.I;
    [SerializeField] private KeyCode switchTeamsKey = KeyCode.T;
    [SerializeField] private KeyCode cycleIndicatorTypeKey = KeyCode.Y;

  private bool indicatorsVisible = true;

    void Start()
    {
        if (autoSetupOnStart)
        {
         SetupDemoUnits();
        }
    }

    void Update()
    {
        // Toggle indicators visibility
        if (Input.GetKeyDown(toggleIndicatorsKey))
     {
            ToggleAllIndicators();
        }

        // Switch team colors
        if (Input.GetKeyDown(switchTeamsKey))
        {
          RandomizeTeamColors();
    }

        // Cycle indicator types
  if (Input.GetKeyDown(cycleIndicatorTypeKey))
      {
       CycleIndicatorTypes();
        }
    }

    /// <summary>
    /// Setup all demo units with team indicators
    /// </summary>
  private void SetupDemoUnits()
    {
        Debug.Log("=== Setting up Team Visual Indicator Demo ===");

        // Setup player units
        foreach (GameObject unit in playerUnits)
        {
      if (unit != null)
     {
      TeamVisualIndicatorPresets.SetupUnitWithTeam(
     unit, 
        Team.Player, 
     TeamVisualIndicatorPresets.StandardColors.Player,
      demoIndicatorType
    );
        }
  }
        Debug.Log($"Setup {playerUnits.Length} Player units");

        // Setup enemy units
        foreach (GameObject unit in enemyUnits)
    {
          if (unit != null)
        {
       TeamVisualIndicatorPresets.SetupUnitWithTeam(
    unit, 
 Team.Enemy, 
            TeamVisualIndicatorPresets.StandardColors.Enemy,
   demoIndicatorType
      );
      }
        }
        Debug.Log($"Setup {enemyUnits.Length} Enemy units");

        // Setup ally units
        foreach (GameObject unit in allyUnits)
 {
        if (unit != null)
            {
      TeamVisualIndicatorPresets.SetupUnitWithTeam(
                    unit, 
        Team.Ally, 
         TeamVisualIndicatorPresets.StandardColors.Ally,
            demoIndicatorType
           );
   }
        }
        Debug.Log($"Setup {allyUnits.Length} Ally units");

        // Setup neutral units
        foreach (GameObject unit in neutralUnits)
        {
            if (unit != null)
            {
      TeamVisualIndicatorPresets.SetupUnitWithTeam(
                unit, 
 Team.Neutral, 
      TeamVisualIndicatorPresets.StandardColors.Neutral,
TeamVisualIndicator.IndicatorType.MaterialTint // Neutral units use subtle tint
                );
          }
        }
        Debug.Log($"Setup {neutralUnits.Length} Neutral units");

        Debug.Log("=== Demo setup complete! ===");
      Debug.Log($"Press [{toggleIndicatorsKey}] to toggle indicators");
        Debug.Log($"Press [{switchTeamsKey}] to randomize team colors");
   Debug.Log($"Press [{cycleIndicatorTypeKey}] to cycle indicator types");
    }

    /// <summary>
    /// Toggle visibility of all indicators
    /// </summary>
    private void ToggleAllIndicators()
    {
        indicatorsVisible = !indicatorsVisible;

      GameObject[] allUnits = GetAllDemoUnits();
        foreach (GameObject unit in allUnits)
      {
      if (unit == null) continue;

            TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
    if (indicator != null)
      {
        indicator.SetIndicatorVisible(indicatorsVisible);
   }
        }

        Debug.Log($"Indicators {(indicatorsVisible ? "shown" : "hidden")}");
    }

    /// <summary>
    /// Randomize team colors for demo
    /// </summary>
    private void RandomizeTeamColors()
    {
        Color[] randomColors = new Color[]
        {
    Color.red, Color.blue, Color.green, Color.yellow,
            Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), // Orange
  new Color(0.5f, 0f, 1f) // Purple
      };

        GameObject[] allUnits = GetAllDemoUnits();
        foreach (GameObject unit in allUnits)
        {
  if (unit == null) continue;

            TeamComponent team = unit.GetComponent<TeamComponent>();
      TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();

            if (team != null && indicator != null)
         {
       Color randomColor = randomColors[Random.Range(0, randomColors.Length)];
   team.SetTeamColor(randomColor);
  indicator.UpdateTeamColor();
            }
        }

 Debug.Log("Randomized team colors");
    }

    /// <summary>
    /// Cycle through indicator types
    /// </summary>
    private void CycleIndicatorTypes()
    {
        // Get current type
        int currentType = (int)demoIndicatorType;
        currentType = (currentType + 1) % System.Enum.GetValues(typeof(TeamVisualIndicator.IndicatorType)).Length;
        demoIndicatorType = (TeamVisualIndicator.IndicatorType)currentType;

        GameObject[] allUnits = GetAllDemoUnits();
        foreach (GameObject unit in allUnits)
        {
    if (unit == null) continue;

        TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
if (indicator != null)
            {
       indicator.SetIndicatorType(demoIndicatorType);
          }
     }

   Debug.Log($"Changed indicator type to: {demoIndicatorType}");
    }

    /// <summary>
    /// Get all demo units
    /// </summary>
    private GameObject[] GetAllDemoUnits()
    {
System.Collections.Generic.List<GameObject> allUnits = new System.Collections.Generic.List<GameObject>();
        allUnits.AddRange(playerUnits);
        allUnits.AddRange(enemyUnits);
    allUnits.AddRange(allyUnits);
        allUnits.AddRange(neutralUnits);
    return allUnits.ToArray();
    }

    /// <summary>
    /// Public method to setup all units (can be called from UI button)
    /// </summary>
    public void SetupAllUnits()
    {
        SetupDemoUnits();
    }

    /// <summary>
    /// Public method to reset all units
    /// </summary>
 public void ResetAllUnits()
    {
        GameObject[] allUnits = GetAllDemoUnits();
        foreach (GameObject unit in allUnits)
        {
       if (unit == null) continue;

        // Remove components
            TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
            TeamComponent team = unit.GetComponent<TeamComponent>();

      if (indicator != null) DestroyImmediate(indicator);
            if (team != null) DestroyImmediate(team);
     }

     Debug.Log("Reset all units - removed team components");
    }

    void OnGUI()
    {
// Simple on-screen instructions
   GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
 style.normal.textColor = Color.white;
style.padding = new RectOffset(10, 10, 10, 10);

  GUILayout.BeginArea(new Rect(10, 10, 300, 150));
  GUILayout.BeginVertical("box");

 GUILayout.Label("Team Visual Indicator Demo", style);
        GUILayout.Space(5);
        GUILayout.Label($"[{toggleIndicatorsKey}] Toggle Indicators", style);
  GUILayout.Label($"[{switchTeamsKey}] Randomize Colors", style);
        GUILayout.Label($"[{cycleIndicatorTypeKey}] Cycle Type", style);
        GUILayout.Space(5);
        GUILayout.Label($"Current Type: {demoIndicatorType}", style);
    GUILayout.Label($"Indicators: {(indicatorsVisible ? "Visible" : "Hidden")}", style);

  GUILayout.EndVertical();
   GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
   // Draw colored gizmos for demo units
   DrawTeamGizmos(playerUnits, Color.blue, "P");
    DrawTeamGizmos(enemyUnits, Color.red, "E");
        DrawTeamGizmos(allyUnits, Color.green, "A");
     DrawTeamGizmos(neutralUnits, Color.gray, "N");
    }

    private void DrawTeamGizmos(GameObject[] units, Color color, string label)
    {
        if (units == null) return;

        Gizmos.color = color;
  foreach (GameObject unit in units)
        {
            if (unit != null)
    {
           Gizmos.DrawWireSphere(unit.transform.position, 0.5f);
                
#if UNITY_EDITOR
       UnityEditor.Handles.Label(
            unit.transform.position + Vector3.up * 1.5f, 
label,
           new GUIStyle() { normal = new GUIStyleState() { textColor = color } }
 );
#endif
      }
        }
    }
}
