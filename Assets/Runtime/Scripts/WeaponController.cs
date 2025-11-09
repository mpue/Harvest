using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls one or more weapons on a unit
/// Handles target acquisition and firing
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private Weapon[] weapons;

    [Header("Targeting")]
    [SerializeField] private bool autoAcquireTargets = true;
    [SerializeField] private float targetScanInterval = 0.5f; // Seconds between scans
    [SerializeField] private LayerMask targetLayerMask = -1;
    [SerializeField] private bool prioritizeClosestTarget = true;

    [Header("Firing")]
    [SerializeField] private bool autoFire = true;
    [SerializeField] private float fireInterval = 0.1f; // Time between weapon fire attempts

    private Transform currentTarget;
    private TeamComponent ownerTeam;
    private float lastScanTime = 0f;
    private float lastFireTime = 0f;
    private List<Transform> potentialTargets = new List<Transform>();

    public Transform CurrentTarget => currentTarget;
    public Weapon[] Weapons => weapons;
    public bool HasTarget => currentTarget != null;

    void Awake()
    {
        ownerTeam = GetComponent<TeamComponent>();

        // Find weapons if not assigned
        if (weapons == null || weapons.Length == 0)
        {
            weapons = GetComponentsInChildren<Weapon>();
        }
    }

    void Start()
    {
        if (weapons.Length == 0)
        {
            Debug.LogWarning($"WeaponController on {gameObject.name} has no weapons assigned!");
        }
    }

    void Update()
    {
        if (autoAcquireTargets)
        {
            UpdateTargetAcquisition();
        }

        if (autoFire && currentTarget != null)
        {
            TryFireWeapons();
        }
    }

    /// <summary>
    /// Scan for and acquire targets
    /// </summary>
    private void UpdateTargetAcquisition()
    {
        // Throttle scanning
        if (Time.time - lastScanTime < targetScanInterval)
        {
            return;
        }

        lastScanTime = Time.time;

        // Check if current target is still valid
        if (currentTarget != null)
        {
            if (!IsValidTarget(currentTarget))
            {
                ClearTarget();
            }
        }

        // Find new target if needed
        if (currentTarget == null)
        {
            AcquireNewTarget();
        }
    }

    /// <summary>
    /// Find and acquire a new target
    /// </summary>
    private void AcquireNewTarget()
    {
        potentialTargets.Clear();

        // Get max weapon range
        float maxRange = GetMaxWeaponRange();

        // Find all potential targets in range
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, maxRange, targetLayerMask);

        foreach (Collider col in collidersInRange)
        {
            Transform target = col.transform;

            // Skip self
            if (target == transform || target.IsChildOf(transform))
            {
                continue;
            }

            // Check if valid target
            if (IsValidTarget(target))
            {
                potentialTargets.Add(target);
            }
        }

        // Select best target
        if (potentialTargets.Count > 0)
        {
            Transform bestTarget = SelectBestTarget(potentialTargets);
            SetTarget(bestTarget);
        }
    }

    /// <summary>
    /// Select the best target from a list
    /// </summary>
    private Transform SelectBestTarget(List<Transform> targets)
    {
        if (targets.Count == 0) return null;
        if (targets.Count == 1) return targets[0];

        if (prioritizeClosestTarget)
        {
            // Find closest target
            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (Transform target in targets)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }
        else
        {
            // Return first valid target
            return targets[0];
        }
    }

    /// <summary>
    /// Check if a target is valid (enemy, alive, etc.)
    /// </summary>
    private bool IsValidTarget(Transform target)
    {
        if (target == null) return false;

        // Check if target has team component
        TeamComponent targetTeam = target.GetComponent<TeamComponent>();
        if (targetTeam == null)
        {
            // No team = can't determine if enemy
            return false;
        }

        // Check if enemy
        if (ownerTeam != null && !ownerTeam.IsEnemy(targetTeam))
        {
            return false; // Not an enemy
        }

        // Optional: Check if target has BaseUnit (is a valid unit)
        BaseUnit targetUnit = target.GetComponent<BaseUnit>();
        if (targetUnit == null)
        {
            return false;
        }

        // Target is valid
        return true;
    }

    /// <summary>
    /// Set target for all weapons
    /// </summary>
    public void SetTarget(Transform target)
    {
        currentTarget = target;

        // Set target for all weapons
        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.SetTarget(target);
            }
        }

        OnTargetAcquired(target);
    }

    /// <summary>
    /// Clear target from all weapons
    /// </summary>
    public void ClearTarget()
    {
        if (currentTarget != null)
        {
            OnTargetLost(currentTarget);
        }

        currentTarget = null;

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.ClearTarget();
            }
        }
    }

    /// <summary>
    /// Try to fire all weapons
    /// </summary>
    private void TryFireWeapons()
    {
        if (Time.time - lastFireTime < fireInterval)
        {
            return;
        }

        bool anyWeaponFired = false;

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                if (weapon.TryFire())
                {
                    anyWeaponFired = true;
                }
            }
        }

        if (anyWeaponFired)
        {
            lastFireTime = Time.time;
        }
    }

    /// <summary>
    /// Manually command to fire at target
    /// </summary>
    public void FireAtTarget(Transform target)
    {
        SetTarget(target);
        TryFireWeapons();
    }

    /// <summary>
    /// Stop firing and clear target
    /// </summary>
    public void CeaseFire()
    {
        ClearTarget();
    }

    /// <summary>
    /// Get maximum range of all weapons
    /// </summary>
    private float GetMaxWeaponRange()
    {
        float maxRange = 0f;

        if (weapons == null || weapons.Length == 0)
        {
            return 20f; // Default range if no weapons
        }   

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null && weapon.Range > maxRange)
            {
                maxRange = weapon.Range;
            }
        }

        return maxRange > 0 ? maxRange : 20f; // Default 20 if no weapons
    }

    /// <summary>
    /// Enable/disable auto targeting
    /// </summary>
    public void SetAutoAcquireTargets(bool enable)
    {
        autoAcquireTargets = enable;
    }

    /// <summary>
    /// Enable/disable auto firing
    /// </summary>
    public void SetAutoFire(bool enable)
    {
        autoFire = enable;
    }

    /// <summary>
    /// Called when a target is acquired
    /// </summary>
    protected virtual void OnTargetAcquired(Transform target)
    {
        Debug.Log($"{gameObject.name} acquired target: {target.name}");
    }

    /// <summary>
    /// Called when a target is lost
    /// </summary>
    protected virtual void OnTargetLost(Transform target)
    {
        Debug.Log($"{gameObject.name} lost target: {target.name}");
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw max weapon range
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        float maxRange = GetMaxWeaponRange();
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}
