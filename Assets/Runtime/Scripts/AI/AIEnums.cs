/// <summary>
/// AI game state phases
/// </summary>
public enum AIState
{
    Initializing,
EarlyGame,
    MidGame,
    LateGame
}

/// <summary>
/// AI strategy types
/// </summary>
public enum AIStrategy
{
    Economic,   // Focus on resource gathering
    Military,   // Focus on army production
  Balanced,   // Mix of economy and military
    Defensive   // Focus on defense structures
}

/// <summary>
/// AI difficulty levels
/// </summary>
public enum AIDifficulty
{
    Easy,
    Medium,
    Hard
}
