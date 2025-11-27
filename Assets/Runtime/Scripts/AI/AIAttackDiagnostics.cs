using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Diagnostic tool to debug AI attack problems
/// Add this to any GameObject in the scene to debug attack issues
/// </summary>
public class AIAttackDiagnostics : MonoBehaviour
{
    [Header("Diagnostics Settings")]
    [SerializeField] private bool runDiagnosticsOnStart = true;
    [SerializeField] private float diagnosticInterval = 10f; // Run diagnostics every 10 seconds
    [SerializeField] private Team aiTeam = Team.Enemy;
    [SerializeField] private Team playerTeam = Team.Player;

    private float diagnosticTimer = 0f;

    void Start()
    {
        if (runDiagnosticsOnStart)
        {
 RunFullDiagnostics();
        }
    }

    void Update()
    {
        diagnosticTimer += Time.deltaTime;
        
        if (diagnosticTimer >= diagnosticInterval)
 {
         RunFullDiagnostics();
  diagnosticTimer = 0f;
        }
    }

    [ContextMenu("Run Full Diagnostics")]
  public void RunFullDiagnostics()
    {
        Debug.Log("=== AI ATTACK DIAGNOSTICS ===");
        
        CheckAIAttackController();
        CheckAIMilitaryUnits();
        CheckPlayerTargets();
        CheckWeaponControllers();
      CheckTeamConfiguration();
        
        Debug.Log("=== DIAGNOSTICS COMPLETE ===");
    }

    private void CheckAIAttackController()
    {
        Debug.Log("--- Checking AIAttackController ---");
        
 AIAttackController[] controllers = FindObjectsOfType<AIAttackController>();
        
     if (controllers.Length == 0)
   {
          Debug.LogError("? NO AIAttackController found in scene!");
  Debug.LogError("   FIX: Add AIAttackController component to a GameObject");
    return;
    }
        
        foreach (var controller in controllers)
     {
       Debug.Log($"? Found AIAttackController on: {controller.gameObject.name}");
    Debug.Log($"   - Enabled: {controller.enabled}");
            Debug.Log($"   - GameObject Active: {controller.gameObject.activeInHierarchy}");
        }
    }

    private void CheckAIMilitaryUnits()
    {
        Debug.Log("--- Checking AI Military Units ---");
        
        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();
        List<BaseUnit> aiMilitaryUnits = new List<BaseUnit>();
        
        foreach (var unit in allUnits)
        {
            TeamComponent teamComp = unit.GetComponent<TeamComponent>();
 if (teamComp == null || teamComp.CurrentTeam != aiTeam)
  continue;
          
   WeaponController weapon = unit.GetComponent<WeaponController>();
    HarvesterUnit harvester = unit.GetComponent<HarvesterUnit>();
     
            if (weapon != null && harvester == null)
            {
   aiMilitaryUnits.Add(unit);
  }
   }
        
        if (aiMilitaryUnits.Count == 0)
        {
         Debug.LogWarning("?? NO AI military units found!");
  Debug.LogWarning("   AI needs to produce Soldiers or MK3 Tanks to attack");
   return;
  }
        
        Debug.Log($"? Found {aiMilitaryUnits.Count} AI military units:");
        foreach (var unit in aiMilitaryUnits)
        {
    WeaponController wc = unit.GetComponent<WeaponController>();
         Debug.Log($"   - {unit.name}:");
 Debug.Log($"     • Has WeaponController: {wc != null}");
   if (wc != null)
            {
          Debug.Log($"     • Auto Acquire: {wc.GetType().GetField("autoAcquireTargets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(wc)}");
       Debug.Log($"     • Auto Fire: {wc.GetType().GetField("autoFire", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(wc)}");
         Debug.Log($"     • Current Target: {wc.CurrentTarget?.name ?? "None"}");
                Debug.Log($"     • Weapons Count: {wc.Weapons?.Length ?? 0}");
    }
        }
    }

    private void CheckPlayerTargets()
    {
        Debug.Log("--- Checking Player Targets ---");
        
   // Check player buildings
      BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        List<BuildingComponent> playerBuildings = new List<BuildingComponent>();
        
        foreach (var building in allBuildings)
    {
         TeamComponent teamComp = building.GetComponent<TeamComponent>();
     if (teamComp != null && teamComp.CurrentTeam == playerTeam)
{
                playerBuildings.Add(building);
            }
        }
        
        if (playerBuildings.Count == 0)
{
            Debug.LogWarning("?? NO player buildings found!");
        }
        else
        {
            Debug.Log($"? Found {playerBuildings.Count} player buildings");
            foreach (var building in playerBuildings)
          {
Debug.Log($"   - {building.name} (Layer: {LayerMask.LayerToName(building.gameObject.layer)})");
          }
 }
        
      // Check player units
      BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();
        List<BaseUnit> playerUnits = new List<BaseUnit>();
     
   foreach (var unit in allUnits)
  {
            TeamComponent teamComp = unit.GetComponent<TeamComponent>();
     if (teamComp != null && teamComp.CurrentTeam == playerTeam)
            {
    playerUnits.Add(unit);
 }
      }
    
   Debug.Log($"? Found {playerUnits.Count} player units");
    }

    private void CheckWeaponControllers()
    {
        Debug.Log("--- Checking WeaponController Configuration ---");
    
        WeaponController[] allWeapons = FindObjectsOfType<WeaponController>();
      
 if (allWeapons.Length == 0)
     {
            Debug.LogError("? NO WeaponControllers found in scene!");
          return;
        }
   
  Debug.Log($"Found {allWeapons.Length} WeaponControllers:");
        
        foreach (var wc in allWeapons)
        {
        TeamComponent team = wc.GetComponent<TeamComponent>();
            if (team == null || team.CurrentTeam != aiTeam)
 continue;
            
 Debug.Log($"   AI WeaponController on: {wc.gameObject.name}");
            
   // Use reflection to check private fields
   var autoAcquire = wc.GetType().GetField("autoAcquireTargets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
 var autoFire = wc.GetType().GetField("autoFire", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var layerMask = wc.GetType().GetField("targetLayerMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
   Debug.Log($"     • Auto Acquire Targets: {autoAcquire?.GetValue(wc)}");
            Debug.Log($"     • Auto Fire: {autoFire?.GetValue(wc)}");
            Debug.Log($"     • Target Layer Mask: {layerMask?.GetValue(wc)}");
          Debug.Log($"     • Weapons Assigned: {wc.Weapons?.Length ?? 0}");
 Debug.Log($"     • Current Target: {wc.CurrentTarget?.name ?? "None"}");
  
            if (wc.Weapons != null && wc.Weapons.Length > 0)
     {
        foreach (var weapon in wc.Weapons)
      {
         if (weapon != null)
  {
             Debug.Log($"       - Weapon: Range={weapon.Range}, Damage={weapon.Damage}, FireRate={weapon.FireRate}");
     }
    }
      }
            else
            {
    Debug.LogError($"     ? NO WEAPONS assigned to {wc.gameObject.name}!");
            }
        }
    }

    private void CheckTeamConfiguration()
    {
        Debug.Log("--- Checking Team Configuration ---");
   
        TeamComponent[] allTeams = FindObjectsOfType<TeamComponent>();
    
        int playerCount = 0;
   int enemyCount = 0;
     int neutralCount = 0;
        
        foreach (var team in allTeams)
        {
        switch (team.CurrentTeam)
            {
     case Team.Player: playerCount++; break;
           case Team.Enemy: enemyCount++; break;
         case Team.Neutral: neutralCount++; break;
         }
      }
        
        Debug.Log($"Team Distribution:");
        Debug.Log($"   - Player: {playerCount}");
     Debug.Log($"   - Enemy: {enemyCount}");
        Debug.Log($"- Neutral: {neutralCount}");
        
 if (enemyCount == 0)
   {
    Debug.LogError("? NO Enemy team entities found!");
          Debug.LogError("   AI units must have TeamComponent set to 'Enemy'");
        }
      
        if (playerCount == 0)
        {
    Debug.LogError("? NO Player team entities found!");
   Debug.LogError("   Player units/buildings must have TeamComponent set to 'Player'");
        }
    }

    [ContextMenu("Force AI Attack Now")]
    public void ForceAIAttackNow()
    {
        AIAttackController controller = FindObjectOfType<AIAttackController>();
        if (controller != null)
        {
      controller.ForceAttack();
   Debug.Log("? Forced AI attack!");
 }
  else
        {
  Debug.LogError("? No AIAttackController found to force attack!");
  }
    }

    [ContextMenu("List All Units With Weapons")]
    public void ListAllUnitsWithWeapons()
    {
        Debug.Log("=== All Units With Weapons ===");
        
        WeaponController[] weapons = FindObjectsOfType<WeaponController>();
        
        foreach (var wc in weapons)
        {
        TeamComponent team = wc.GetComponent<TeamComponent>();
   BaseUnit unit = wc.GetComponent<BaseUnit>();
         
  Debug.Log($"{wc.gameObject.name}:");
Debug.Log($"   Team: {team?.CurrentTeam.ToString() ?? "No Team"}");
          Debug.Log($"   Unit Name: {unit?.UnitName ?? "No BaseUnit"}");
  Debug.Log($"   Has Target: {wc.HasTarget}");
    Debug.Log($"   Target: {wc.CurrentTarget?.name ?? "None"}");
        }
    }
}
