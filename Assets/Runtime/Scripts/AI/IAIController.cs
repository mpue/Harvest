using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base interface for AI controllers to work with task system
/// </summary>
public interface IAIController
{
    ResourceManager ResourceManager { get; }
    Team AITeam { get; }
    
    // Building methods
    bool CanBuildNow();
    int GetBuildingCount(string buildingName);
    bool HasBuilding(string buildingName);
    bool IsBuildingInProgress(string buildingName);
    bool BuildStructure(string buildingName);
    
    // Unit methods
    int GetUnitCount(string unitName);
    bool IsProductionQueueFull(string buildingName);
    bool ProduceUnit(string buildingName, string unitName);
}
