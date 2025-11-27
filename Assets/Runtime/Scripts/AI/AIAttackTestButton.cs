using UnityEngine;

/// <summary>
/// Simple button to force AI attacks for testing
/// Attach to a UI Button or call manually
/// </summary>
public class AIAttackTestButton : MonoBehaviour
{
    [ContextMenu("Force AI Attack")]
    public void ForceAIAttack()
    {
        AIAttackController controller = FindObjectOfType<AIAttackController>();
        
  if (controller != null)
        {
   controller.ForceAttack();
      Debug.Log("? AI Attack forced!");
        }
    else
    {
     Debug.LogError("? No AIAttackController found in scene!");
        }
    }
    
    [ContextMenu("Enable All Weapons")]
    public void EnableAllWeapons()
    {
        WeaponController[] weapons = FindObjectsOfType<WeaponController>();
        
  int count = 0;
        foreach (var weapon in weapons)
        {
     weapon.SetAutoAcquireTargets(true);
      weapon.SetAutoFire(true);
          count++;
        }
        
        Debug.Log($"? Enabled auto-targeting and auto-fire for {count} weapons");
    }
    
    [ContextMenu("List All Weapons Status")]
    public void ListAllWeaponsStatus()
    {
        Debug.Log("=== WEAPON STATUS ===");
    
        WeaponController[] weapons = FindObjectsOfType<WeaponController>();
        
        foreach (var wc in weapons)
  {
    TeamComponent team = wc.GetComponent<TeamComponent>();
  Debug.Log($"{wc.gameObject.name}:");
            Debug.Log($"  Team: {team?.CurrentTeam ?? Team.Neutral}");
       Debug.Log($"  Has Target: {wc.HasTarget}");
Debug.Log($"  Target: {wc.CurrentTarget?.name ?? "None"}");
    Debug.Log($"  Weapons: {wc.Weapons?.Length ?? 0}");
 }
     
        Debug.Log("=== END ===");
    }
}
