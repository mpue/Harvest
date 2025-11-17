using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Harvester unit that collects resources from collectables
/// </summary>
[RequireComponent(typeof(BaseUnit))]
[RequireComponent(typeof(Controllable))]
public class HarvesterUnit : MonoBehaviour
{
    [Header("Harvest Settings")]
    [SerializeField] private int carryCapacity = 50;
    [SerializeField] private int currentCarried = 0;
    [SerializeField] private ResourceType carriedResourceType = ResourceType.Gold;
    [SerializeField] private float harvestRange = 2f;

    [Header("References")]
    [SerializeField] private TeamComponent teamComponent;
    [SerializeField] private ResourceManager resourceManager;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject carryVisual;
    [SerializeField] private Transform carryPoint;

    [Header("State")]
    [SerializeField] private HarvesterState currentState = HarvesterState.Idle;
    [SerializeField] private Collectable targetCollectable;
    [SerializeField] private ResourceCollector targetCollector;

    private BaseUnit baseUnit;
    private Controllable controllable;
    private float harvestTimer = 0f;

    // Properties
    public int CurrentCarried => currentCarried;
    public int CarryCapacity => carryCapacity;
    public bool IsFull => currentCarried >= carryCapacity;
    public bool IsEmpty => currentCarried == 0;
    public HarvesterState CurrentState => currentState;
    public Collectable TargetCollectable => targetCollectable;

    void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();
        controllable = GetComponent<Controllable>();
        if (teamComponent == null)
        {
            teamComponent = GetComponent<TeamComponent>();
        }
    }

    void Start()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }

        UpdateCarryVisual();
    }

    void Update()
    {
        switch (currentState)
        {
            case HarvesterState.MovingToResource:
                UpdateMovingToResource();
                break;

            case HarvesterState.Harvesting:
                UpdateHarvesting();
                break;

            case HarvesterState.MovingToCollector:
                UpdateMovingToCollector();
                break;

            case HarvesterState.Unloading:
                UpdateUnloading();
                break;
        }
    }

    /// <summary>
    /// Command harvester to gather from a collectable
    /// </summary>
    public void GatherFrom(Collectable collectable)
    {
        if (collectable == null || collectable.IsDepleted)
        {
            Debug.LogWarning("Cannot gather from null or depleted collectable!");
            return;
        }

        targetCollectable = collectable;
        currentState = HarvesterState.MovingToResource;

        // Move to collectable using Controllable
        if (controllable != null)
        {
            controllable.MoveTo(collectable.transform.position);
        }

        Debug.Log($"{gameObject.name}: Moving to harvest {collectable.ResourceType}");
    }

    /// <summary>
    /// Update state: Moving to resource
    /// </summary>
    private void UpdateMovingToResource()
    {
        if (targetCollectable == null || targetCollectable.IsDepleted)
        {
            // Resource gone, find new one or return
            StopHarvesting();
            return;
        }

        // Check if reached collectable
        float distance = Vector3.Distance(transform.position, targetCollectable.transform.position);
        if (distance <= harvestRange)
        {
            // Start harvesting
            currentState = HarvesterState.Harvesting;
            harvestTimer = 0f;

            if (controllable != null)
            {
                controllable.Stop();
            }

            Debug.Log($"{gameObject.name}: Started harvesting");
        }
    }

    /// <summary>
    /// Update state: Harvesting
    /// </summary>
    private void UpdateHarvesting()
    {
        if (targetCollectable == null || targetCollectable.IsDepleted)
        {
            // Resource depleted
            if (currentCarried > 0)
            {
                // Return to collector
                ReturnToCollector();
            }
            else
            {
                // Find new resource
                StopHarvesting();
            }
            return;
        }

        // Check if full
        if (IsFull)
        {
            ReturnToCollector();
            return;
        }

        // Harvest over time
        harvestTimer += Time.deltaTime;

        if (harvestTimer >= targetCollectable.HarvestTime)
        {
            // Harvest
            int amountToHarvest = Mathf.Min(
                targetCollectable.AmountPerHarvest,
                carryCapacity - currentCarried
            );

            int harvested = targetCollectable.Harvest(amountToHarvest);
            currentCarried += harvested;
            carriedResourceType = targetCollectable.ResourceType;

            UpdateCarryVisual();

            harvestTimer = 0f;

            Debug.Log($"{gameObject.name}: Harvested {harvested}. Carrying: {currentCarried}/{carryCapacity}");

            // Check if full
            if (IsFull)
            {
                ReturnToCollector();
            }
        }
    }

    /// <summary>
    /// Return to nearest collector
    /// </summary>
    private void ReturnToCollector()
    {
        // Find nearest collector
        targetCollector = FindNearestCollector();

        if (targetCollector == null)
        {
            Debug.LogWarning($"{gameObject.name}: No collector found!");
            currentState = HarvesterState.Idle;
            return;
        }

        currentState = HarvesterState.MovingToCollector;

        // Move to collector
        if (controllable != null)
        {
            controllable.MoveTo(targetCollector.transform.position);
        }

        Debug.Log($"{gameObject.name}: Returning to collector with {currentCarried} {carriedResourceType}");
    }

    /// <summary>
    /// Update state: Moving to collector
    /// </summary>
    private void UpdateMovingToCollector()
    {
        if (targetCollector == null)
        {
            StopHarvesting();
            return;
        }

        // Check if reached collector
        float distance = Vector3.Distance(transform.position, targetCollector.transform.position);
        if (distance <= targetCollector.UnloadRange)
        {
            // Start unloading
            currentState = HarvesterState.Unloading;
            harvestTimer = 0f;

            if (controllable != null)
            {
                controllable.Stop();
            }

            Debug.Log($"{gameObject.name}: Started unloading");
        }
    }

    /// <summary>
    /// Update state: Unloading
    /// </summary>
    private void UpdateUnloading()
    {
        if (targetCollector == null)
        {
            StopHarvesting();
            return;
        }

        // Unload over time
        harvestTimer += Time.deltaTime;

        if (harvestTimer >= targetCollector.UnloadTime)
        {
            // Unload resources
            targetCollector.DepositResources(carriedResourceType, currentCarried, resourceManager);

            currentCarried = 0;
            UpdateCarryVisual();

            harvestTimer = 0f;

            Debug.Log($"{gameObject.name}: Unloaded resources");

            // Return to harvest if we still have a target
            if (targetCollectable != null && !targetCollectable.IsDepleted)
            {
                currentState = HarvesterState.MovingToResource;
                if (controllable != null)
                {
                    controllable.MoveTo(targetCollectable.transform.position);
                }
            }
            else
            {
                // Find new resource or idle
                Collectable nearest = FindNearestCollectable();
                if (nearest != null)
                {
                    GatherFrom(nearest);
                }
                else
                {
                    currentState = HarvesterState.Idle;
                }
            }
        }
    }

    /// <summary>
    /// Stop harvesting and go idle
    /// </summary>
    public void StopHarvesting()
    {
        currentState = HarvesterState.Idle;
        targetCollectable = null;
        targetCollector = null;

        if (controllable != null)
        {
            controllable.Stop();
        }
    }

    /// <summary>
    /// Find nearest collectable
    /// </summary>
    private Collectable FindNearestCollectable()
    {
        Collectable[] collectables = FindObjectsOfType<Collectable>();
        Collectable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var collectable in collectables)
        {
            if (collectable.IsDepleted)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collectable.transform.position);
            if (distance < nearestDistance)
            {
                nearest = collectable;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find nearest collector of same team
    /// </summary>
    private ResourceCollector FindNearestCollector()
    {
        ResourceCollector[] collectors = FindObjectsOfType<ResourceCollector>();
        ResourceCollector nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var collector in collectors)
        {
            // Check if same team
            TeamComponent collectorTeam = collector.GetComponent<TeamComponent>();
            if (collectorTeam == null || teamComponent == null)
            {
                continue;
            }

            if (collectorTeam.CurrentTeam != teamComponent.CurrentTeam)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, collector.transform.position);
            if (distance < nearestDistance)
            {
                nearest = collector;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Update carry visual
    /// </summary>
    private void UpdateCarryVisual()
    {
        if (carryVisual != null)
        {
            carryVisual.SetActive(currentCarried > 0);

            // Update position
            if (carryPoint != null && carryVisual.activeSelf)
            {
                carryVisual.transform.position = carryPoint.position;
            }
        }
    }

    /// <summary>
    /// Auto-gather: Find and harvest nearest resource
    /// </summary>
    public void AutoGather()
    {
        Collectable nearest = FindNearestCollectable();
        if (nearest != null)
        {
            GatherFrom(nearest);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No collectables found!");
        }
    }

    void OnDrawGizmos()
    {
        // Draw state
        Gizmos.color = currentState == HarvesterState.Idle ? Color.white :
       currentState == HarvesterState.Harvesting ? Color.yellow :
          currentState == HarvesterState.Unloading ? Color.green :
           Color.cyan;

     Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.5f);

   // Draw harvest range
    if (currentState == HarvesterState.Harvesting && targetCollectable != null)
   {
            Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, harvestRange);
            Gizmos.DrawLine(transform.position, targetCollectable.transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
  // Draw info
        #if UNITY_EDITOR
  UnityEditor.Handles.Label(transform.position + Vector3.up * 3f,
            $"State: {currentState}\nCarrying: {currentCarried}/{carryCapacity}");
        #endif
    }
}

/// <summary>
/// Harvester states
/// </summary>
public enum HarvesterState
{
    Idle,
    MovingToResource,
    Harvesting,
    MovingToCollector,
    Unloading
}
