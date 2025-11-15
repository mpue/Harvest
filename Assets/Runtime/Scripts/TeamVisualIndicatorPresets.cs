using UnityEngine;

/// <summary>
/// Example setup configurations for TeamVisualIndicator
/// Demonstrates recommended settings for different game types
/// </summary>
public class TeamVisualIndicatorPresets : MonoBehaviour
{
    /// <summary>
    /// Apply RTS-style team indicator (recommended for strategy games)
    /// </summary>
    public static void ApplyRTSPreset(TeamVisualIndicator indicator)
    {
        if (indicator == null) return;

      indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.Combined);
        // Settings would be configured via reflection or made public
        Debug.Log("RTS Preset applied: Color Ring + Material Tint with rotation and pulse");
    }

    /// <summary>
    /// Apply MOBA-style team indicator (clear, static rings)
    /// </summary>
    public static void ApplyMOBAPreset(TeamVisualIndicator indicator)
    {
 if (indicator == null) return;

     indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.ColorRing);
     Debug.Log("MOBA Preset applied: Large static Color Ring");
    }

    /// <summary>
    /// Apply Action-style team indicator (outline + icon)
    /// </summary>
    public static void ApplyActionPreset(TeamVisualIndicator indicator)
    {
 if (indicator == null) return;

        indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.ShieldIcon);
   Debug.Log("Action Preset applied: Billboard Shield Icon");
    }

    /// <summary>
    /// Apply minimal team indicator (subtle tint only)
    /// </summary>
    public static void ApplyMinimalPreset(TeamVisualIndicator indicator)
    {
        if (indicator == null) return;

        indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.MaterialTint);
        Debug.Log("Minimal Preset applied: Material Tint only");
    }

    /// <summary>
    /// Setup standard team colors
    /// </summary>
    public static class StandardColors
    {
        public static readonly Color Player = new Color(0.2f, 0.5f, 1f);      // Bright Blue
        public static readonly Color Enemy = new Color(1f, 0.2f, 0.2f);       // Bright Red
        public static readonly Color Ally = new Color(0.2f, 1f, 0.2f);        // Bright Green
        public static readonly Color Neutral = new Color(0.7f, 0.7f, 0.7f);   // Gray
        
        // Alternative color schemes
        public static readonly Color PlayerAlt = new Color(0f, 0.8f, 1f);     // Cyan
        public static readonly Color EnemyAlt = new Color(1f, 0.4f, 0f);    // Orange
        public static readonly Color AllyAlt = new Color(1f, 1f, 0.2f);       // Yellow
    }

    /// <summary>
    /// Setup a unit with team and visual indicator
    /// </summary>
    public static void SetupUnitWithTeam(GameObject unit, Team team, Color teamColor, TeamVisualIndicator.IndicatorType indicatorType)
    {
  if (unit == null) return;

// Add or get TeamComponent
        TeamComponent teamComp = unit.GetComponent<TeamComponent>();
  if (teamComp == null)
        {
   teamComp = unit.AddComponent<TeamComponent>();
        }
        teamComp.SetTeam(team);
        teamComp.SetTeamColor(teamColor);

 // Add or get TeamVisualIndicator
     TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
        if (indicator == null)
        {
            indicator = unit.AddComponent<TeamVisualIndicator>();
        }
        indicator.SetIndicatorType(indicatorType);
        indicator.UpdateTeamColor();

 Debug.Log($"Setup {unit.name} with Team: {team}, Color: {teamColor}, Indicator: {indicatorType}");
    }

    /// <summary>
/// Quick setup for player units (Blue Color Ring)
    /// </summary>
    public static void SetupAsPlayerUnit(GameObject unit)
    {
        SetupUnitWithTeam(unit, Team.Player, StandardColors.Player, TeamVisualIndicator.IndicatorType.ColorRing);
    }

    /// <summary>
    /// Quick setup for enemy units (Red Color Ring)
    /// </summary>
    public static void SetupAsEnemyUnit(GameObject unit)
    {
        SetupUnitWithTeam(unit, Team.Enemy, StandardColors.Enemy, TeamVisualIndicator.IndicatorType.ColorRing);
    }

    /// <summary>
    /// Quick setup for ally units (Green Color Ring)
    /// </summary>
public static void SetupAsAllyUnit(GameObject unit)
    {
        SetupUnitWithTeam(unit, Team.Ally, StandardColors.Ally, TeamVisualIndicator.IndicatorType.ColorRing);
    }

    /// <summary>
    /// Quick setup for neutral units (Gray Material Tint)
    /// </summary>
    public static void SetupAsNeutralUnit(GameObject unit)
    {
        SetupUnitWithTeam(unit, Team.Neutral, StandardColors.Neutral, TeamVisualIndicator.IndicatorType.MaterialTint);
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu items for quick setup
/// </summary>
public static class TeamVisualIndicatorMenuItems
{
    [UnityEditor.MenuItem("GameObject/RTS/Setup As Player Unit", false, 10)]
    private static void SetupAsPlayerUnit()
    {
  foreach (GameObject obj in UnityEditor.Selection.gameObjects)
        {
            TeamVisualIndicatorPresets.SetupAsPlayerUnit(obj);
      }
    }

    [UnityEditor.MenuItem("GameObject/RTS/Setup As Enemy Unit", false, 11)]
    private static void SetupAsEnemyUnit()
    {
        foreach (GameObject obj in UnityEditor.Selection.gameObjects)
        {
            TeamVisualIndicatorPresets.SetupAsEnemyUnit(obj);
   }
    }

    [UnityEditor.MenuItem("GameObject/RTS/Setup As Ally Unit", false, 12)]
    private static void SetupAsAllyUnit()
    {
        foreach (GameObject obj in UnityEditor.Selection.gameObjects)
      {
   TeamVisualIndicatorPresets.SetupAsAllyUnit(obj);
        }
}

    [UnityEditor.MenuItem("GameObject/RTS/Setup As Neutral Unit", false, 13)]
    private static void SetupAsNeutralUnit()
    {
    foreach (GameObject obj in UnityEditor.Selection.gameObjects)
   {
   TeamVisualIndicatorPresets.SetupAsNeutralUnit(obj);
        }
    }

  [UnityEditor.MenuItem("GameObject/RTS/Apply RTS Preset", false, 20)]
    private static void ApplyRTSPreset()
    {
        foreach (GameObject obj in UnityEditor.Selection.gameObjects)
        {
      TeamVisualIndicator indicator = obj.GetComponent<TeamVisualIndicator>();
            if (indicator != null)
       {
            TeamVisualIndicatorPresets.ApplyRTSPreset(indicator);
     }
        }
    }

    [UnityEditor.MenuItem("GameObject/RTS/Apply MOBA Preset", false, 21)]
 private static void ApplyMOBAPreset()
    {
        foreach (GameObject obj in UnityEditor.Selection.gameObjects)
    {
         TeamVisualIndicator indicator = obj.GetComponent<TeamVisualIndicator>();
    if (indicator != null)
            {
       TeamVisualIndicatorPresets.ApplyMOBAPreset(indicator);
   }
  }
    }

    // Validation
    [UnityEditor.MenuItem("GameObject/RTS/Setup As Player Unit", true)]
    [UnityEditor.MenuItem("GameObject/RTS/Setup As Enemy Unit", true)]
    [UnityEditor.MenuItem("GameObject/RTS/Setup As Ally Unit", true)]
    [UnityEditor.MenuItem("GameObject/RTS/Setup As Neutral Unit", true)]
    [UnityEditor.MenuItem("GameObject/RTS/Apply RTS Preset", true)]
    [UnityEditor.MenuItem("GameObject/RTS/Apply MOBA Preset", true)]
  private static bool ValidateSelection()
    {
        return UnityEditor.Selection.gameObjects.Length > 0;
    }
}
#endif
