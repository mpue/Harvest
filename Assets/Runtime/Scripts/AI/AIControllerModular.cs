using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Modular AI Controller using task-based system
/// MUCH CLEANER AND MAINTAINABLE!
/// </summary>
public class AIControllerModular : MonoBehaviour, IAIController
{
    [Header("AI Settings")]
    [SerializeField] private Team aiTeam = Team.Enemy;
    [SerializeField] private AIStrategy initialStrategy = AIStrategy.Balanced;
    [SerializeField] private float updateInterval = 1f;

    [Header("Strategy Settings")]
    [SerializeField] private bool autoSwitchStrategy = true;
    [SerializeField] private float strategyChangeInterval = 120f;

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameObject headquarters;

    // Task system
    private AITaskManager taskManager;
    private AIStrategyBuilder strategyBuilder;

    // State tracking
    private List<BaseUnit> myUnits = new List<BaseUnit>();
    private List<BuildingComponent> myBuildings = new List<BuildingComponent>();
    private ProductionComponent hqProduction;
    private float updateTimer = 0f;
    private float strategyTimer = 0f;
    private float lastBuildTime = 0f;
    private float buildCooldown = 2f;
    private HashSet<string> buildingsInProgress = new HashSet<string>();
    private AIState currentState = AIState.EarlyGame;
    private AIStrategy currentStrategy;

    // Properties (for tasks to access)
    public ResourceManager ResourceManager => resourceManager;
    public Team AITeam => aiTeam;
    public ProductionComponent HQProduction => hqProduction;

    void Start()
    {
        // Initialize
        if (!Initialize())
        {
            enabled = false;
            return;
        }

        // Create task system
        taskManager = new AITaskManager(this);
        strategyBuilder = new AIStrategyBuilder(this, taskManager);

        // Load initial strategy
        currentStrategy = initialStrategy;
        LoadStrategy(currentStrategy);

        Debug.Log($"? AI ({aiTeam}): Modular AI initialized with {initialStrategy} strategy");
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        strategyTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateAI();
            updateTimer = 0f;
        }

        // Auto-switch strategy
        if (autoSwitchStrategy && strategyTimer >= strategyChangeInterval)
        {
            SwitchStrategy();
            strategyTimer = 0f;
        }
    }

    /// <summary>
    /// Initialize AI
    /// </summary>
    private bool Initialize()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError($"AI ({aiTeam}): No ResourceManager found!");
                return false;
            }
        }

        if (!FindHeadquarters())
        {
            Debug.LogError($"AI ({aiTeam}): No headquarters found!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Find headquarters
    /// </summary>
    private bool FindHeadquarters()
    {
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (var building in allBuildings)
        {
            TeamComponent team = building.GetComponent<TeamComponent>();
            if (team != null && team.CurrentTeam == aiTeam && building.IsHeadquarter)
            {
                headquarters = building.gameObject;
                hqProduction = building.GetComponent<ProductionComponent>();

                if (hqProduction == null)
                {
                    Debug.LogError($"AI ({aiTeam}): Headquarters has no ProductionComponent!");
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Main AI update loop
    /// </summary>
    private void UpdateAI()
    {
        UpdateUnitLists();
        UpdateGameState();
        CleanupBuildingsInProgress();

        // Execute tasks
        taskManager.Update();

        // Debug every 10 seconds
        if (Time.frameCount % 600 == 0)
        {
            Debug.Log($"AI ({aiTeam}) Status: State={currentState}, Strategy={currentStrategy}, Gold={resourceManager.Gold}, Tasks={taskManager.ActiveTaskCount}");
            Debug.Log(taskManager.GetDebugInfo());
        }
    }

    /// <summary>
    /// Update unit and building lists
    /// </summary>
    private void UpdateUnitLists()
    {
        myUnits.Clear();
        myBuildings.Clear();

        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();
        foreach (var unit in allUnits)
        {
            TeamComponent team = unit.GetComponent<TeamComponent>();
            if (team != null && team.CurrentTeam == aiTeam)
            {
                myUnits.Add(unit);
            }
        }

        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (var building in allBuildings)
        {
            TeamComponent team = building.GetComponent<TeamComponent>();
            if (team != null && team.CurrentTeam == aiTeam)
            {
                myBuildings.Add(building);
            }
        }
    }

    /// <summary>
    /// Update game state
    /// </summary>
    private void UpdateGameState()
    {
        if (myBuildings.Count <= 3)
            currentState = AIState.EarlyGame;
        else if (myBuildings.Count <= 8)
            currentState = AIState.MidGame;
        else
            currentState = AIState.LateGame;
    }

    /// <summary>
    /// Cleanup buildings in progress HashSet
    /// </summary>
    private void CleanupBuildingsInProgress()
    {
        if (hqProduction == null) return;

        var queuedProducts = hqProduction.GetQueuedProducts();
        HashSet<string> buildingsInQueue = new HashSet<string>();

        foreach (var product in queuedProducts)
        {
            if (product != null && product.IsBuilding)
            {
                string name = product.ProductName;
                if (name.Contains("Factory")) buildingsInQueue.Add("Factory");
                if (name.Contains("Barracks")) buildingsInQueue.Add("Barracks");
                if (name.Contains("ResourceCollector")) buildingsInQueue.Add("ResourceCollector");
                if (name.Contains("DefenceTower")) buildingsInQueue.Add("DefenceTower");
                if (name.Contains("EnergyBlock")) buildingsInQueue.Add("EnergyBlock");
            }
        }

        buildingsInProgress.RemoveWhere(name => !buildingsInQueue.Contains(name));
    }

    /// <summary>
    /// Load a strategy
    /// </summary>
    private void LoadStrategy(AIStrategy strategy)
    {
        currentStrategy = strategy;

        switch (strategy)
        {
            case AIStrategy.Economic:
                strategyBuilder.BuildEconomicStrategy();
                break;
            case AIStrategy.Military:
                strategyBuilder.BuildMilitaryStrategy();
                break;
            case AIStrategy.Balanced:
                // Use game state to decide
                if (currentState == AIState.EarlyGame)
                    strategyBuilder.BuildEarlyGameStrategy();
                else if (currentState == AIState.MidGame)
                    strategyBuilder.BuildMidGameStrategy();
                else
                    strategyBuilder.BuildLateGameStrategy();
                break;
        }
    }

    /// <summary>
    /// Switch to a new strategy
    /// </summary>
    private void SwitchStrategy()
    {
        // Cycle through strategies
        AIStrategy newStrategy = currentStrategy;

        if (currentState == AIState.EarlyGame)
        {
            newStrategy = AIStrategy.Economic; // Focus economy early
        }
        else if (currentState == AIState.MidGame)
        {
            newStrategy = AIStrategy.Balanced;
        }
        else
        {
            newStrategy = AIStrategy.Military; // Attack late game
        }

        if (newStrategy != currentStrategy)
        {
            Debug.Log($"AI ({aiTeam}): Switching strategy from {currentStrategy} to {newStrategy}");
            LoadStrategy(newStrategy);
        }
    }

    // ===== HELPER METHODS FOR TASKS =====

    public bool CanBuildNow()
    {
        return Time.time - lastBuildTime >= buildCooldown;
    }

    public int GetBuildingCount(string buildingName)
    {
        return myBuildings.Count(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains(buildingName));
    }

    public bool HasBuilding(string buildingName)
    {
        return GetBuildingCount(buildingName) > 0;
    }

    public bool IsBuildingInProgress(string buildingName)
    {
        return buildingsInProgress.Contains(buildingName);
    }

    public int GetUnitCount(string unitName)
    {
        if (unitName == "Harvester")
            return myUnits.Count(u => u.GetComponent<HarvesterUnit>() != null);
        else if (unitName == "Soldier" || unitName == "MK3")
            return myUnits.Count(u => u.GetComponent<WeaponController>() != null && u.UnitName.Contains(unitName));
        else
            return myUnits.Count(u => u.UnitName.Contains(unitName));
    }

    public bool IsProductionQueueFull(string buildingName)
    {
        var building = myBuildings.FirstOrDefault(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains(buildingName));
        if (building == null) return true;

        var production = building.GetComponent<ProductionComponent>();
        if (production == null) return true;

        return production.QueueCount >= production.MaxQueueSize;
    }

    public bool BuildStructure(string buildingName)
    {
        if (buildingsInProgress.Contains(buildingName))
            return false;

        if (!CanBuildNow())
            return false;

        if (hqProduction == null)
            return false;

        if (hqProduction.QueueCount >= hqProduction.MaxQueueSize)
            return false;

        Product product = hqProduction.AvailableProducts.Find(p => p.IsBuilding && p.ProductName.Contains(buildingName));
        if (product == null)
            return false;

        buildingsInProgress.Add(buildingName);
        bool success = hqProduction.AddToQueue(product);

        if (success)
        {
            lastBuildTime = Time.time;
            Debug.Log($"? AI ({aiTeam}): Building {buildingName}");
        }
        else
        {
            buildingsInProgress.Remove(buildingName);
        }

        return success;
    }

    public bool ProduceUnit(string buildingName, string unitName)
    {
        var building = myBuildings.FirstOrDefault(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains(buildingName));
        if (building == null)
            return false;

        var production = building.GetComponent<ProductionComponent>();
        if (production == null)
            return false;

        if (production.QueueCount >= production.MaxQueueSize)
            return false;

        Product product = production.AvailableProducts.Find(p => !p.IsBuilding && p.ProductName.Contains(unitName));
        if (product == null)
            return false;

        bool success = production.AddToQueue(product);
        if (success)
        {
            Debug.Log($"? AI ({aiTeam}): Producing {unitName} from {buildingName}");
        }

        return success;
    }
}
