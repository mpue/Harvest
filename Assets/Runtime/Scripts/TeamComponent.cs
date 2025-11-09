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
