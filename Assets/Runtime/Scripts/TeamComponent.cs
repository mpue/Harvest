using UnityEngine;

/// <summary>
/// Defines team/faction affiliation for units
/// Used to determine friend or foe for combat
/// </summary>
public class TeamComponent : MonoBehaviour
{
    [Header("Team Settings")]
    [SerializeField] private Team team = Team.Player;
    [SerializeField] private Color teamColor = Color.blue;

    public Team CurrentTeam => team;
    public Color TeamColor => teamColor;

    void Awake()
    {
        // Set default team colors if not customized
        if (teamColor == Color.blue || teamColor == new Color(0, 0, 1, 1))
        {
            teamColor = GetDefaultTeamColor(team);
        }
    }

    /// <summary>
    /// Get default color for a team
    /// </summary>
    public static Color GetDefaultTeamColor(Team team)
    {
        switch (team)
        {
            case Team.Player:
                return new Color(0.2f, 0.5f, 1f); // Bright Blue
            case Team.Enemy:
                return new Color(1f, 0.2f, 0.2f); // Bright Red
            case Team.Ally:
                return new Color(0.2f, 1f, 0.2f); // Bright Green
            case Team.Neutral:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Check if another unit is an enemy
    /// </summary>
    public bool IsEnemy(TeamComponent other)
    {
        if (other == null) return false;
        return other.CurrentTeam != this.CurrentTeam && other.CurrentTeam != Team.Neutral;
    }

    /// <summary>
    /// Check if another unit is an ally
    /// </summary>
    public bool IsAlly(TeamComponent other)
    {
        if (other == null) return false;
        return other.CurrentTeam == this.CurrentTeam;
    }

    /// <summary>
    /// Set team at runtime
    /// </summary>
    public void SetTeam(Team newTeam)
    {
        team = newTeam;
    }

    /// <summary>
    /// Set team color
    /// </summary>
    public void SetTeamColor(Color color)
    {
        teamColor = color;
        ApplyTeamColorToRenderers();
    }

    void Start()
    {
        ApplyTeamColorToRenderers();
    }

    /// <summary>
    /// Apply team color to unit renderers (optional visual feedback)
    /// </summary>
    private void ApplyTeamColorToRenderers()
    {
        // Optional: Apply team color to materials
        // This can be customized based on your visual style
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.name.Contains("TeamColorMarker"))
            {
                renderer.material.color = teamColor;
            }
        }
    }
}

/// <summary>
/// Team/Faction enum
/// </summary>
public enum Team
{
    Player = 0,
    Enemy = 1,
    Ally = 2,
    Neutral = 3
}
