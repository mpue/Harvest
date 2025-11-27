using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls AI attack waves - periodically sends groups of military units to attack the player
/// </summary>
public class AIAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 60f; // Attack every 1 minute (REDUCED from 2min)
    [SerializeField] private float firstAttackDelay = 90f; // First attack after 1.5 minutes (REDUCED from 3min)
    [SerializeField] private int minAttackForce = 2; // Minimum units to send (REDUCED from 3)
    [SerializeField] private int maxAttackForce = 6; // Maximum units to send (REDUCED from 8)

    [Header("Adaptive Attack")]
    [SerializeField] private bool useAdaptiveAttacks = true; // Attack when enough units available
    [SerializeField] private int adaptiveAttackThreshold = 4; // Attack immediately if this many military units available
    [SerializeField] private float adaptiveCheckInterval = 10f; // Check every 10 seconds

    [Header("Unit Selection")]
    [SerializeField] private bool includeTanks = true;
    [SerializeField] private bool includeSoldiers = true;
    [SerializeField] private float gatherRadius = 20f; // Rally units within this radius before attacking

    [Header("Target Selection")]
    [SerializeField] private bool targetBuildings = true;
    [SerializeField] private bool targetUnits = true;
    [SerializeField] private bool preferHeadquarters = true;

    [Header("References")]
    [SerializeField] private Team aiTeam = Team.Enemy;
    [SerializeField] private Team targetTeam = Team.Player;

    private float attackTimer = 0f;
    private float adaptiveCheckTimer = 0f;
    private bool firstAttackSent = false;
    private List<BaseUnit> attackForce = new List<BaseUnit>();
    private Vector3 attackTarget = Vector3.zero;

    void Start()
    {
        attackTimer = firstAttackDelay;
        Debug.Log($"AIAttackController (AGGRESSIVE): First attack in {firstAttackDelay}s, then every {attackInterval}s");
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        adaptiveCheckTimer -= Time.deltaTime;

        // Regular timed attacks
        if (attackTimer <= 0f)
        {
            LaunchAttack();
            attackTimer = attackInterval;
        }

        // Adaptive attacks - attack early if enough units available
        if (useAdaptiveAttacks && adaptiveCheckTimer <= 0f)
        {
            adaptiveCheckTimer = adaptiveCheckInterval;

            // Check if we have enough units for an opportunistic attack
            List<BaseUnit> availableUnits = FindAvailableMilitaryUnits();

            if (availableUnits.Count >= adaptiveAttackThreshold)
            {
                Debug.Log($"?? AIAttackController: ADAPTIVE ATTACK triggered! ({availableUnits.Count} units >= {adaptiveAttackThreshold} threshold)");
                LaunchAttack();
                attackTimer = attackInterval; // Reset normal timer
            }
        }
    }

    /// <summary>
    /// Launch an attack wave
    /// </summary>
    private void LaunchAttack()
    {
        // Find available military units
        List<BaseUnit> availableUnits = FindAvailableMilitaryUnits();

        if (availableUnits.Count < minAttackForce)
        {
            Debug.LogWarning($"AIAttackController: Not enough units for attack (have {availableUnits.Count}, need {minAttackForce})");
            return;
        }

        // Select attack force
        int attackSize = Mathf.Min(Random.Range(minAttackForce, maxAttackForce + 1), availableUnits.Count);
        attackForce.Clear();

        for (int i = 0; i < attackSize; i++)
        {
            attackForce.Add(availableUnits[i]);
        }

        // Find attack target
        Vector3 target = FindAttackTarget();
        if (target == Vector3.zero)
        {
            Debug.LogWarning("AIAttackController: No valid attack target found!");
            return;
        }

        attackTarget = target;

        // Rally units to gather point first, then attack
        Vector3 rallyPoint = FindRallyPoint();
        RallyAndAttack(rallyPoint);

        Debug.Log($"?? AIAttackController: Launching attack with {attackForce.Count} units to {attackTarget}!");
        firstAttackSent = true;
    }

    /// <summary>
    /// Check for adaptive attack opportunities based on available units
    /// </summary>
    private void CheckAdaptiveAttack()
    {
        List<BaseUnit> availableUnits = FindAvailableMilitaryUnits();

        if (availableUnits.Count >= adaptiveAttackThreshold)
        {
            Debug.Log("AIAttackController: Adaptive attack triggered!");
            LaunchAttack();
        }
    }

    /// <summary>
    /// Find available military units (tanks and soldiers)
    /// </summary>
    private List<BaseUnit> FindAvailableMilitaryUnits()
    {
        List<BaseUnit> militaryUnits = new List<BaseUnit>();
        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();

        foreach (var unit in allUnits)
        {
            // Check team
            TeamComponent teamComp = unit.GetComponent<TeamComponent>();
            if (teamComp == null || teamComp.CurrentTeam != aiTeam)
                continue;

            // Check if it's a military unit (has weapon, not a harvester)
            WeaponController weapon = unit.GetComponent<WeaponController>();
            HarvesterUnit harvester = unit.GetComponent<HarvesterUnit>();

            if (weapon != null && harvester == null)
            {
                // Check unit type
                string unitName = unit.UnitName.ToLower();
                bool isTank = unitName.Contains("mk3") || unitName.Contains("tank");
                bool isSoldier = unitName.Contains("soldier") || unitName.Contains("infantry");

                if ((includeTanks && isTank) || (includeSoldiers && isSoldier))
                {
                    militaryUnits.Add(unit);
                }
            }
        }

        return militaryUnits;
    }

    /// <summary>
    /// Find attack target (player building or unit)
    /// </summary>
    private Vector3 FindAttackTarget()
    {
        // Try to find player headquarters first if preferred
        if (preferHeadquarters && targetBuildings)
        {
            BuildingComponent[] buildings = FindObjectsOfType<BuildingComponent>();
            foreach (var building in buildings)
            {
                TeamComponent teamComp = building.GetComponent<TeamComponent>();
                if (teamComp != null && teamComp.CurrentTeam == targetTeam && building.IsHeadquarter)
                {
                    return building.transform.position;
                }
            }
        }

        // Find any player building
        if (targetBuildings)
        {
            BuildingComponent[] buildings = FindObjectsOfType<BuildingComponent>();
            List<BuildingComponent> playerBuildings = new List<BuildingComponent>();

            foreach (var building in buildings)
            {
                TeamComponent teamComp = building.GetComponent<TeamComponent>();
                if (teamComp != null && teamComp.CurrentTeam == targetTeam)
                {
                    playerBuildings.Add(building);
                }
            }

            if (playerBuildings.Count > 0)
            {
                return playerBuildings[Random.Range(0, playerBuildings.Count)].transform.position;
            }
        }

        // Find player units
        if (targetUnits)
        {
            BaseUnit[] units = FindObjectsOfType<BaseUnit>();
            List<BaseUnit> playerUnits = new List<BaseUnit>();

            foreach (var unit in units)
            {
                TeamComponent teamComp = unit.GetComponent<TeamComponent>();
                if (teamComp != null && teamComp.CurrentTeam == targetTeam)
                {
                    playerUnits.Add(unit);
                }
            }

            if (playerUnits.Count > 0)
            {
                return playerUnits[Random.Range(0, playerUnits.Count)].transform.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Find rally point for gathering units before attack
    /// </summary>
    private Vector3 FindRallyPoint()
    {
        if (attackForce.Count == 0)
            return Vector3.zero;

        // Use center of attack force as rally point
        Vector3 center = Vector3.zero;
        foreach (var unit in attackForce)
        {
            center += unit.transform.position;
        }
        center /= attackForce.Count;

        // Move rally point slightly towards target
        Vector3 toTarget = (attackTarget - center).normalized;
        return center + toTarget * 10f;
    }

    /// <summary>
    /// Rally units then send them to attack
    /// </summary>
    private void RallyAndAttack(Vector3 rallyPoint)
    {
        foreach (var unit in attackForce)
        {
            if (unit == null) continue;

            Controllable controllable = unit.GetComponent<Controllable>();
            WeaponController weaponController = unit.GetComponent<WeaponController>();
            
            if (controllable != null)
            {
                // Move to attack target directly
                controllable.MoveTo(attackTarget);
            }
         
            // Make sure weapons are active and auto-firing
            if (weaponController != null)
 {
                weaponController.SetAutoAcquireTargets(true);
                weaponController.SetAutoFire(true);
      Debug.Log($"? {unit.name} weapon auto-targeting enabled for attack");
    }
        }
    }

    /// <summary>
    /// Force an attack immediately (for testing or triggered events)
    /// </summary>
    public void ForceAttack()
    {
        attackTimer = 0f;
        Debug.Log("AIAttackController: Forcing immediate attack!");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw attack target
        if (attackTarget != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTarget, 5f);
            Gizmos.DrawLine(attackTarget, attackTarget + Vector3.up * 10f);
        }

        // Draw attack force
        if (attackForce != null && attackForce.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var unit in attackForce)
            {
                if (unit != null)
                {
                    Gizmos.DrawLine(unit.transform.position, attackTarget);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            string info = $"Next Attack: {attackTimer:F1}s\n";
            info += $"Attack Force: {(attackForce != null ? attackForce.Count : 0)} units\n";
            info += $"First Attack: {(firstAttackSent ? "Sent" : "Pending")}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, info);
        }
#endif
    }
}
