using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AI Controller for computer opponents
/// </summary>
public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private Team aiTeam = Team.Enemy;
    [SerializeField] private AIDifficulty difficulty = AIDifficulty.Medium;
    [SerializeField] private float updateInterval = 1f;

    [Header("Strategy Settings")]
    [SerializeField] private AIStrategy currentStrategy = AIStrategy.Balanced;
    [SerializeField] private bool autoSwitchStrategy = true;
    [SerializeField] private float strategyChangeInterval = 60f;
    [SerializeField] private float buildCooldown = 2f; // NEW: Cooldown between build commands

    [Header("Economic Settings")]
    [SerializeField] private int minGoldReserve = 100;
    [SerializeField] private int idealHarvesterCount = 5;
    [SerializeField] private int maxHarvesterCount = 10;

    [Header("Military Settings")]
    [SerializeField] private int minArmySize = 5;
    [SerializeField] private int idealArmySize = 15;
    [SerializeField] private float attackThreshold = 0.7f; // Army strength ratio to attack

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameObject headquarters;

    private float updateTimer = 0f;
    private float strategyTimer = 0f;
    private float lastBuildTime = 0f; // Track last build command
    private string lastBuiltStructure = ""; // Track what was just built
    private HashSet<string> buildingsInProgress = new HashSet<string>(); // NEW: Track buildings being built RIGHT NOW
    private List<BaseUnit> myUnits = new List<BaseUnit>();
    private List<BuildingComponent> myBuildings = new List<BuildingComponent>();
    private ProductionComponent hqProduction;

    // State tracking
    private AIState currentState = AIState.Initializing;
    private int currentHarvesterCount = 0;
    private int currentArmySize = 0;
    private bool isUnderAttack = false;

    void Start()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError($"AI ({aiTeam}): No ResourceManager found! AI cannot function.");
                enabled = false;
                return;
            }
        }

        FindHeadquarters();

        if (headquarters == null)
        {
            Debug.LogError($"AI ({aiTeam}): No headquarters found! AI cannot function. Make sure you have a building with TeamComponent (Team={aiTeam}) and BuildingComponent (IsHeadquarter=true).");
            enabled = false;
            return;
        }

        if (hqProduction == null)
        {
            Debug.LogError($"AI ({aiTeam}): Headquarters has no ProductionComponent! AI cannot produce units.");
            enabled = false;
            return;
        }

        if (hqProduction.AvailableProducts.Count == 0)
        {
            Debug.LogWarning($"AI ({aiTeam}): No products available in ProductionComponent! Add products to enable production.");
        }

        InitializeAI();

        Debug.Log($"? AI ({aiTeam}): Successfully initialized!");
        Debug.Log($"  - Headquarters: {headquarters.name}");
        Debug.Log($"  - Available Products: {hqProduction.AvailableProducts.Count}");
        Debug.Log($"  - Resource Manager: {resourceManager.gameObject.name}");
        Debug.Log($"  - Initial Gold: {resourceManager.Gold}");
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

        if (autoSwitchStrategy && strategyTimer >= strategyChangeInterval)
        {
            EvaluateStrategy();
            strategyTimer = 0f;
        }
    }

    /// <summary>
    /// Initialize AI systems
    /// </summary>
    private void InitializeAI()
    {
        Debug.Log($"AI ({aiTeam}): Initializing with difficulty {difficulty}");
        currentState = AIState.EarlyGame;
        UpdateUnitLists();

        Debug.Log($"AI ({aiTeam}): Found {myUnits.Count} units and {myBuildings.Count} buildings");
    }

    /// <summary>
    /// Main AI update loop
    /// </summary>
    private void UpdateAI()
    {
        UpdateUnitLists();
        AnalyzeGameState();

        // Debug every 10 updates
        if (Time.frameCount % 600 == 0) // Every ~10 seconds at 60fps
        {
            Debug.Log($"AI ({aiTeam}) Status: State={currentState}, Strategy={currentStrategy}, Gold={resourceManager.Gold}, Harvesters={currentHarvesterCount}/{idealHarvesterCount}, Army={currentArmySize}/{idealArmySize}, Buildings={myBuildings.Count}");
        }

        switch (currentState)
        {
            case AIState.EarlyGame:
                ExecuteEarlyGameStrategy();
                break;
            case AIState.MidGame:
                ExecuteMidGameStrategy();
                break;
            case AIState.LateGame:
                ExecuteLateGameStrategy();
                break;
        }

        // Always check for defensive needs
        if (isUnderAttack)
        {
            HandleDefense();
        }
    }

    /// <summary>
    /// Update lists of controlled units and buildings
    /// </summary>
    private void UpdateUnitLists()
    {
        myUnits.Clear();
        myBuildings.Clear();
        currentHarvesterCount = 0;
        currentArmySize = 0;

        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();
        foreach (var unit in allUnits)
        {
            TeamComponent team = unit.GetComponent<TeamComponent>();
            if (team != null && team.CurrentTeam == aiTeam)
            {
                myUnits.Add(unit);

                // Count harvesters
                if (unit.GetComponent<HarvesterUnit>() != null)
                {
                    currentHarvesterCount++;
                }
                // Count military units
                else if (unit.GetComponent<WeaponController>() != null)
                {
                    currentArmySize++;
                }
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

   // NEW: Clean up buildingsInProgress HashSet
     // Remove buildings that are NO LONGER in the production queue
      if (hqProduction != null)
  {
     var queuedProducts = hqProduction.GetQueuedProducts();
   HashSet<string> buildingsInQueue = new HashSet<string>();
     
            // Get all building names currently in queue
            foreach (var product in queuedProducts)
            {
  if (product != null && product.IsBuilding)
              {
               // Extract building name (e.g., "Factory" from "FactoryBuilding")
        string productName = product.ProductName;
   
   // Add to set (handles "Factory", "Barracks", etc.)
                  if (productName.Contains("Factory")) buildingsInQueue.Add("Factory");
         if (productName.Contains("Barracks")) buildingsInQueue.Add("Barracks");
        if (productName.Contains("ResourceCollector")) buildingsInQueue.Add("ResourceCollector");
        if (productName.Contains("DefenceTower")) buildingsInQueue.Add("DefenceTower");
        if (productName.Contains("EnergyBlock")) buildingsInQueue.Add("EnergyBlock");
  }
          }

     // Remove from HashSet if NOT in queue anymore
        HashSet<string> toRemove = new HashSet<string>();
      foreach (string buildingName in buildingsInProgress)
 {
        if (!buildingsInQueue.Contains(buildingName))
                {
      toRemove.Add(buildingName);
     Debug.Log($"AI ({aiTeam}): Removing '{buildingName}' from buildingsInProgress (no longer in queue)");
       }
    }
        
     foreach (string name in toRemove)
            {
       buildingsInProgress.Remove(name);
 }
        }
    }

    /// <summary>
    /// Analyze current game state
    /// </summary>
    private void AnalyzeGameState()
    {
        // Determine game phase
        if (myBuildings.Count <= 3)
        {
            currentState = AIState.EarlyGame;
        }
        else if (myBuildings.Count <= 8)
        {
            currentState = AIState.MidGame;
        }
        else
        {
            currentState = AIState.LateGame;
        }

        // Check if under attack
        isUnderAttack = CheckIfUnderAttack();
    }

    /// <summary>
    /// Execute early game strategy
    /// </summary>
    private void ExecuteEarlyGameStrategy()
    {
   // REVISED STRATEGY: Parallel production for efficiency!

     // Count existing energy blocks (BUILT, not in production)
        int energyBlockCount = myBuildings.Count(b =>
              b.BuildingProduct != null &&
   b.BuildingProduct.EnergyProduction > 0 &&
      b.BuildingProduct.ProductName.Contains("EnergyBlock"));

        // Count energy blocks IN PRODUCTION
      int energyBlocksInProduction = 0;
  if (hqProduction != null)
        {
        var queuedProducts = hqProduction.GetQueuedProducts();
        energyBlocksInProduction = queuedProducts.Count(p =>
         p != null &&
           p.IsBuilding &&
     p.ProductName.Contains("EnergyBlock"));
        }

        int totalEnergyBlocks = energyBlockCount + energyBlocksInProduction;

    Debug.Log($"AI ({aiTeam}) Early Game: Energy={energyBlockCount}+{energyBlocksInProduction}={totalEnergyBlocks}/3, Gold={resourceManager.Gold}, Harvesters={currentHarvesterCount}/{idealHarvesterCount}, Buildings={myBuildings.Count}");

        // === PRIORITY 1: Build 3 Energy Blocks (FOUNDATION!) ===
 if (totalEnergyBlocks < 3 && resourceManager.Gold >= 100)
        {
            Debug.Log($"AI ({aiTeam}): PRIORITY 1 - Building EnergyBlock #{totalEnergyBlocks + 1}/3");
            BuildStructure("EnergyBlock");
       return; // STOP! Wait for energy
  }

      // === PRIORITY 2: Build EXACTLY ONE Factory ===
     bool hasFactory = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));
     
    bool factoryInProgress = buildingsInProgress.Contains("Factory"); // NEW: Immediate check!
      
        bool factoryInProduction = false;
     if (hqProduction != null)
   {
      var queuedProducts = hqProduction.GetQueuedProducts();
       factoryInProduction = queuedProducts.Any(p =>
     p != null &&
      p.IsBuilding &&
    p.ProductName.Contains("Factory"));
     
    // DEBUG: Log what's in the queue
   if (queuedProducts.Count > 0)
         {
 Debug.Log($"AI ({aiTeam}): HQ Queue contains {queuedProducts.Count} items: {string.Join(", ", queuedProducts.Select(p => p != null ? p.ProductName : "null"))}");
       }
        }

     // NEW: Check if we JUST built a Factory (within last 5 seconds)
        bool factoryRecentlyBuilt = (Time.time - lastBuildTime < 5f && lastBuiltStructure == "Factory");

        // ENHANCED DEBUG
  Debug.Log($"AI ({aiTeam}): Factory check - hasFactory={hasFactory}, factoryInProgress={factoryInProgress}, factoryInProduction={factoryInProduction}, factoryRecentlyBuilt={factoryRecentlyBuilt}, Gold={resourceManager.Gold}, Energy={resourceManager.AvailableEnergy}");

        // Build Factory ONLY if NONE of these are true!
  if (!hasFactory && !factoryInProgress && !factoryInProduction && !factoryRecentlyBuilt && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 10)
   {
   Debug.Log($"AI ({aiTeam}): PRIORITY 2 - Building Factory (ONLY ONE!)");
BuildStructure("Factory");
    return; // STOP! Wait for Factory
 }
      else if (hasFactory)
   {
    Debug.Log($"AI ({aiTeam}): Factory already exists, skipping to next priority");
    }
        else if (factoryInProgress)
 {
       Debug.Log($"AI ({aiTeam}): Factory IN PROGRESS (HashSet), waiting...");
            return; // WAIT!
        }
else if (factoryInProduction)
   {
Debug.Log($"AI ({aiTeam}): Factory in production queue, waiting...");
   return; // WAIT for it!
        }
      else if (factoryRecentlyBuilt)
  {
  Debug.Log($"AI ({aiTeam}): Factory recently built ({Time.time - lastBuildTime:F1}s ago), waiting for queue update...");
   return; // WAIT!
        }
        else
        {
      Debug.Log($"AI ({aiTeam}): Cannot build Factory - Gold={resourceManager.Gold}/250, Energy={resourceManager.AvailableEnergy}/10");
     }

        // Check Factory production status
        ProductionComponent factoryProduction = null;
        BuildingComponent factoryBuilding = myBuildings.FirstOrDefault(b =>
            b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));
        if (factoryBuilding != null)
    {
            factoryProduction = factoryBuilding.GetComponent<ProductionComponent>();
        }

    // === PRIORITY 3: Build ResourceCollector (FROM HQ, parallel with Harvesters!) ===
        int collectorCount = myBuildings.Count(b => b.GetComponent<ResourceCollector>() != null);
        bool collectorInProduction = false;
    if (hqProduction != null)
        {
        var queuedProducts = hqProduction.GetQueuedProducts();
   collectorInProduction = queuedProducts.Any(p =>
    p != null &&
                p.IsBuilding &&
      p.ProductName.Contains("ResourceCollector"));
        }

        // Build collector if we have at least 1 Harvester OR one is being produced
        int harvestersInProduction = factoryProduction != null ? 
   factoryProduction.GetQueuedProducts().Count(p => p != null && p.ProductName.Contains("Harvester")) : 0;
        
        bool canBuildCollector = (currentHarvesterCount >= 1 || harvestersInProduction >= 1) && 
              !collectorInProduction && 
           collectorCount < 1 && 
           resourceManager.Gold >= 200 && 
         resourceManager.AvailableEnergy >= 10;

        if (canBuildCollector)
        {
            Debug.Log($"AI ({aiTeam}): PRIORITY 3 - Building ResourceCollector (parallel to Harvesters)");
       BuildStructure("ResourceCollector");
            return; // Build from HQ
        }

        // === PRIORITY 4: Produce Harvesters from Factory (CONTINUOUSLY until we have 3!) ===
      if (hasFactory && currentHarvesterCount < 3 && resourceManager.Gold >= 100 && resourceManager.AvailableEnergy >= 5)
        {
            // Check if Factory queue has space
     if (factoryProduction != null && factoryProduction.QueueCount < factoryProduction.MaxQueueSize)
 {
        Debug.Log($"AI ({aiTeam}): PRIORITY 4 - Producing Harvester {currentHarvesterCount + 1}/3 (early economy)");
           ProduceUnitFromFactory("Harvester");
          return; // Produce from Factory
    }
 }

     // === PRIORITY 5: Build EXACTLY ONE Barracks (after 3 Harvesters!) ===
        bool hasBarracks = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));
        
    bool barracksInProgress = buildingsInProgress.Contains("Barracks"); // NEW: HashSet check!
        
    bool barracksInProduction = false;
   if (hqProduction != null)
        {
 var queuedProducts = hqProduction.GetQueuedProducts();
    barracksInProduction = queuedProducts.Any(p =>
 p != null &&
     p.IsBuilding &&
         p.ProductName.Contains("Barracks"));
        }

     if (currentHarvesterCount >= 3 && !hasBarracks && !barracksInProgress && !barracksInProduction && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 10)
        {
      Debug.Log($"AI ({aiTeam}): PRIORITY 5 - Building Barracks (ONLY ONE!)");
  BuildStructure("Barracks");
            return; // Build from HQ
        }
     else if (hasBarracks)
        {
 Debug.Log($"AI ({aiTeam}): Barracks already exists, moving to next priority");
      }
   else if (barracksInProgress)
     {
    Debug.Log($"AI ({aiTeam}): Barracks IN PROGRESS (HashSet), waiting...");
      return; // WAIT!
     }

        // === PRIORITY 6: Continue producing Harvesters (up to 5 total) ===
        if (hasFactory && currentHarvesterCount < idealHarvesterCount && resourceManager.Gold >= 100 && resourceManager.AvailableEnergy >= 5)
        {
      if (factoryProduction != null && factoryProduction.QueueCount < factoryProduction.MaxQueueSize)
      {
       Debug.Log($"AI ({aiTeam}): PRIORITY 6 - Producing Harvester {currentHarvesterCount + 1}/{idealHarvesterCount}");
  ProduceUnitFromFactory("Harvester");
  return;
 }
   }

        // === PRIORITY 6B: Produce MK3 Tanks (from Factory, after 3 Harvesters!) ===
 // Count MK3 tanks
        int mk3Count = myUnits.Count(u => 
       u.GetComponent<WeaponController>() != null && 
      u.UnitName.Contains("MK3"));

        int mk3Limit = 3; // Build 3 MK3 tanks in early game

        if (hasFactory && 
     currentHarvesterCount >= 3 && 
      mk3Count < mk3Limit && 
     resourceManager.Gold >= 200 && // MK3 is expensive!
   resourceManager.AvailableEnergy >= 10)
        {
  if (factoryProduction != null && factoryProduction.QueueCount < factoryProduction.MaxQueueSize)
            {
     Debug.Log($"AI ({aiTeam}): PRIORITY 6B - Producing MK3 Tank {mk3Count + 1}/{mk3Limit} (heavy firepower!)");
        ProduceUnitFromFactory("MK3");
        return;
  }
        }

      // === PRIORITY 7: Build Defense (ONLY after economy is running!) ===
        int towerCount = myBuildings.Count(b => b.BuildingProduct?.BuildingType == BuildingType.DefenseTower);
      if (collectorCount >= 1 && 
         currentHarvesterCount >= 3 &&
    towerCount < 2 &&
       resourceManager.Gold >= 200 &&
     resourceManager.AvailableEnergy >= 8)
        {
     Debug.Log($"AI ({aiTeam}): PRIORITY 7 - Building DefenceTower {towerCount + 1}/2");
       BuildStructure("DefenceTower");
   return;
  }

        // === PRIORITY 8: Produce Soldiers (if we have Barracks) ===
  if (hasBarracks && currentArmySize < minArmySize && resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 5)
        {
  Debug.Log($"AI ({aiTeam}): PRIORITY 8 - Producing Soldier {currentArmySize + 1}/{minArmySize}");
   ProduceUnitFromBarracks("Soldier");
   return;
    }

     // === PRIORITY 9: Assign harvesters to gather ===
 if (currentHarvesterCount > 0)
   {
       AssignHarvestersToResources();
        }

  Debug.Log($"AI ({aiTeam}): Early Game - Waiting (Gold={resourceManager.Gold}, Energy={resourceManager.AvailableEnergy})");
    }

    /// <summary>
    /// Execute mid game strategy
    /// </summary>
    private void ExecuteMidGameStrategy()
    {
        switch (currentStrategy)
        {
            case AIStrategy.Economic:
                ExecuteEconomicStrategy();
                break;
            case AIStrategy.Military:
                ExecuteMilitaryStrategy();
                break;
            case AIStrategy.Balanced:
                ExecuteBalancedStrategy();
                break;
        }
    }

    /// <summary>
    /// Execute late game strategy
    /// </summary>
    private void ExecuteLateGameStrategy()
    {
        // Focus on military dominance

        // Count existing energy blocks
        int energyBlockCount = myBuildings.Count(b =>
             b.BuildingProduct != null &&
          b.BuildingProduct.EnergyProduction > 0 &&
         b.BuildingProduct.ProductName.Contains("EnergyBlock"));

        // Ensure enough energy - late game needs LOTS of power, but cap at 5 blocks
        if (energyBlockCount < 5 && resourceManager.AvailableEnergy < 30 && resourceManager.Gold >= 100)
        {
            BuildStructure("EnergyBlock");
            return; // Max 5 energy blocks in late game
        }

        // Ensure we have both production buildings (only with good energy)
        bool hasBarracks = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));
        bool hasFactory = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));

        if (!hasBarracks && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 12)
        {
            BuildStructure("Barracks");
            return;
        }

        if (!hasFactory && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 12)
        {
            BuildStructure("Factory");
            return;
        }

        // Build army from Barracks (with energy buffer)
        if (hasBarracks && currentArmySize < idealArmySize && resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 8)
        {
            ProduceUnitFromBarracks("Soldier");
            return;
        }

        // Build vehicles from Factory (if we need more units and have energy)
        if (hasFactory && currentArmySize < idealArmySize && resourceManager.Gold >= 200 && resourceManager.AvailableEnergy >= 10)
        {
            // Try to produce vehicles (if available in Factory)
            ProduceUnitFromFactory("Tank"); // or whatever vehicle name you have
            return;
        }

        // Additional defense (needs energy reserve)
        if (myBuildings.Count(b => b.BuildingProduct?.BuildingType == BuildingType.DefenseTower) < 5 &&
  resourceManager.Gold >= 200 &&
 resourceManager.AvailableEnergy >= 8)
        {
            BuildStructure("DefenceTower");
            return;
        }

        // Build walls for protection (low energy cost)
        if (resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 5)
        {
            BuildStructure("WallBig");
            return;
        }

        // Attack if strong enough (and we have energy surplus)
        if (resourceManager.AvailableEnergy >= 15 && currentArmySize >= minArmySize && ShouldAttack())
        {
            LaunchAttack();
        }
    }

    /// <summary>
    /// Economic focused strategy
    /// </summary>
    private void ExecuteEconomicStrategy()
    {
        // Count existing energy blocks
        int energyBlockCount = myBuildings.Count(b =>
 b.BuildingProduct != null &&
   b.BuildingProduct.EnergyProduction > 0 &&
   b.BuildingProduct.ProductName.Contains("EnergyBlock"));

        // Expand energy FIRST - economic strategy needs LOTS of energy, but not infinite!
        if (energyBlockCount < 6 && resourceManager.AvailableEnergy < 40 && resourceManager.Gold >= 100)
        {
            BuildStructure("EnergyBlock");
            return; // Max 6 energy blocks for economic strategy
        }

        // Ensure we have Factory for Harvesters (only with good energy)
        bool hasFactory = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));
        if (!hasFactory && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 15)
        {
            BuildStructure("Factory");
            return;
        }

        // Max out harvesters (from Factory) - needs energy buffer
        if (hasFactory && currentHarvesterCount < maxHarvesterCount && resourceManager.Gold >= 100 && resourceManager.AvailableEnergy >= 10)
        {
            ProduceUnitFromFactory("Harvester");
            return;
        }

        // Build collectors (if lots of energy available)
        if (resourceManager.Gold >= 200 && resourceManager.AvailableEnergy >= 15)
        {
            BuildStructure("ResourceCollector");
            return;
        }

        // Minimal defense (Soldiers from Barracks) - low priority
        bool hasBarracks = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));
        if (!hasBarracks && currentArmySize < minArmySize && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 15)
        {
            BuildStructure("Barracks");
            return;
        }

        if (hasBarracks && currentArmySize < minArmySize && resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 8)
        {
            ProduceUnitFromBarracks("Soldier");
        }
    }

    /// <summary>
    /// Military focused strategy
    /// </summary>
    private void ExecuteMilitaryStrategy()
    {
        // Count existing energy blocks
        int energyBlockCount = myBuildings.Count(b =>
            b.BuildingProduct != null &&
            b.BuildingProduct.EnergyProduction > 0 &&
         b.BuildingProduct.ProductName.Contains("EnergyBlock"));

        // Ensure energy first - even military needs power, but not too many blocks
        if (energyBlockCount < 4 && resourceManager.AvailableEnergy < 25 && resourceManager.Gold >= 100)
        {
            BuildStructure("EnergyBlock");
            return; // Max 4 energy blocks for military (focus on units, not power)
        }

        // Build Barracks for Soldiers (needs stable energy)
        bool hasBarracks = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));
        if (!hasBarracks && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 10)
        {
            BuildStructure("Barracks");
            return;
        }

        // Build aggressive army from Barracks (only with energy reserve)
        if (hasBarracks && currentArmySize < idealArmySize * 1.5f && resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 8)
        {
            ProduceUnitFromBarracks("Soldier");
            return;
        }

        // Build Factory for vehicles (support units) - needs good energy
        bool hasFactory = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));
        if (!hasFactory && resourceManager.Gold >= 300 && resourceManager.AvailableEnergy >= 12)
        {
            BuildStructure("Factory");
            return;
        }

        // Build defensive structures (energy reserve needed)
        if (resourceManager.Gold >= 200 && resourceManager.AvailableEnergy >= 8)
        {
            BuildStructure("DefenceTower");
            return;
        }

        // Maintain minimum harvesters (from Factory) - for income
        if (hasFactory && currentHarvesterCount < idealHarvesterCount / 2 && resourceManager.Gold >= 100 && resourceManager.AvailableEnergy >= 5)
        {
            ProduceUnitFromFactory("Harvester");
            return;
        }

        // Attack when ready (only with energy surplus)
        if (resourceManager.AvailableEnergy >= 10 && currentArmySize >= minArmySize && ShouldAttack())
        {
            LaunchAttack();
        }
    }

    /// <summary>
    /// Balanced strategy
    /// </summary>
    private void ExecuteBalancedStrategy()
    {
        // Count existing energy blocks
        int energyBlockCount = myBuildings.Count(b =>
     b.BuildingProduct != null &&
    b.BuildingProduct.EnergyProduction > 0 &&
            b.BuildingProduct.ProductName.Contains("EnergyBlock"));

        // Priority 1: Energy (CRITICAL!) - but only if we don't have enough blocks yet
        if (energyBlockCount < 4 && resourceManager.AvailableEnergy < 25 && resourceManager.Gold >= 100)
        {
            BuildStructure("EnergyBlock");
            return; // Max 4 energy blocks in balanced strategy
        }

        // Priority 2: Factory (for Harvesters) - only if good energy
        bool hasFactory = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));
        if (!hasFactory && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 10)
        {
            BuildStructure("Factory");
            return;
        }

        // Priority 3: Harvesters from Factory - only if enough energy
        if (hasFactory && currentHarvesterCount < idealHarvesterCount && resourceManager.Gold >= 100 && resourceManager.AvailableEnergy >= 5)
        {
            ProduceUnitFromFactory("Harvester");
            return;
        }

        // Priority 4: Barracks (for Soldiers) - needs stable energy
        bool hasBarracks = myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));
        if (!hasBarracks && resourceManager.Gold >= 250 && resourceManager.AvailableEnergy >= 10)
        {
            BuildStructure("Barracks");
            return;
        }

        // Priority 5: Army from Barracks - only with good energy
        if (hasBarracks && currentArmySize < idealArmySize && resourceManager.Gold >= 150 && resourceManager.AvailableEnergy >= 5)
        {
            ProduceUnitFromBarracks("Soldier");
            return;
        }

        // Priority 6: Defense - requires energy reserve
        if (myBuildings.Count(b => b.BuildingProduct?.BuildingType == BuildingType.DefenseTower) < 3)
        {
            if (resourceManager.Gold >= 200 && resourceManager.AvailableEnergy >= 8)
            {
                BuildStructure("DefenceTower");
                return;
            }
        }

        // Priority 7: Resource collection - needs lots of energy
        if (myBuildings.Count(b => b.GetComponent<ResourceCollector>() != null) < 2)
        {
            if (resourceManager.Gold >= 200 && resourceManager.AvailableEnergy >= 10)
            {
                BuildStructure("ResourceCollector");
                return;
            }
        }

        // Priority 8: Opportunistic attacks (only if we have energy surplus)
        if (resourceManager.AvailableEnergy >= 15 && currentArmySize >= idealArmySize * 0.8f && ShouldAttack())
        {
            LaunchAttack();
        }
    }

    /// <summary>
    /// Produce a unit
    /// </summary>
    private void ProduceUnit(string unitName)
    {
        if (hqProduction == null)
        {
            Debug.LogWarning($"AI ({aiTeam}): Cannot produce {unitName} - No ProductionComponent!");
            return;
        }

        if (hqProduction.QueueCount >= hqProduction.MaxQueueSize)
        {
            Debug.Log($"AI ({aiTeam}): Cannot produce {unitName} - Queue full ({hqProduction.QueueCount}/{hqProduction.MaxQueueSize})");
            return;
        }

        // Find product by name
        Product product = hqProduction.AvailableProducts.Find(p => p.ProductName.Contains(unitName));

        if (product == null)
        {
            Debug.LogWarning($"AI ({aiTeam}): Cannot find product containing '{unitName}'. Available products: {string.Join(", ", hqProduction.AvailableProducts.Select(p => p.ProductName))}");
            return;
        }

        // Check if can afford
        if (resourceManager.Gold < product.GoldCost)
        {
            Debug.Log($"AI ({aiTeam}): Cannot afford {unitName} - Need {product.GoldCost} gold, have {resourceManager.Gold}");
            return;
        }

        bool success = hqProduction.AddToQueue(product);
        if (success)
        {
            Debug.Log($"✓ AI ({aiTeam}): Producing {unitName} (Cost: {product.GoldCost} gold)");
        }
        else
        {
            Debug.LogWarning($"AI ({aiTeam}): Failed to add {unitName} to queue (AddToQueue returned false)");
        }
    }

    /// <summary>
    /// Produce unit from Factory (Harvesters, Vehicles)
    /// </summary>
    private void ProduceUnitFromFactory(string unitName)
    {
        // Find Factory
   BuildingComponent factory = myBuildings.FirstOrDefault(b =>
     b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"));

   if (factory == null)
 {
Debug.LogWarning($"AI ({aiTeam}): Cannot produce {unitName} - No Factory found! Buildings: {myBuildings.Count}, Factory in list: {myBuildings.Any(b => b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Factory"))}");
     return;
   }

   ProductionComponent production = factory.GetComponent<ProductionComponent>();
   if (production == null)
   {
     Debug.LogError($"AI ({aiTeam}): Factory '{factory.gameObject.name}' has no ProductionComponent!");
return;
   }

        // DEBUG: Log Factory status
        Debug.Log($"AI ({aiTeam}): Factory found - Name: {factory.gameObject.name}, Production Queue: {production.QueueCount}/{production.MaxQueueSize}, Available Products: {production.AvailableProducts.Count}");

if (production.QueueCount >= production.MaxQueueSize)
        {
   Debug.Log($"AI ({aiTeam}): Cannot produce {unitName} from Factory - Queue full ({production.QueueCount}/{production.MaxQueueSize})");
   return;
  }

     // Find product
        Product product = production.AvailableProducts.Find(p => !p.IsBuilding && p.ProductName.Contains(unitName));
    if (product == null)
  {
       Debug.LogWarning($"AI ({aiTeam}): Factory cannot produce '{unitName}'. Available units: {string.Join(", ", production.AvailableProducts.Where(p => !p.IsBuilding).Select(p => p.ProductName))}");
  return;
   }

        // Check affordability
    if (resourceManager.Gold < product.GoldCost)
 {
Debug.Log($"AI ({aiTeam}): Cannot afford {unitName} from Factory - Need {product.GoldCost}, have {resourceManager.Gold}");
  return;
        }

 bool success = production.AddToQueue(product);
     if (success)
   {
 Debug.Log($"✓ AI ({aiTeam}): Factory producing {unitName} (Cost: {product.GoldCost} gold, Queue: {production.QueueCount}/{production.MaxQueueSize})");
  }
   else
        {
Debug.LogError($"AI ({aiTeam}): Factory AddToQueue({unitName}) FAILED!");
     }
    }

    /// <summary>
    /// Produce unit from Barracks (Soldiers)
    /// </summary>
    private void ProduceUnitFromBarracks(string unitName)
    {
  // Find Barracks
   BuildingComponent barracks = myBuildings.FirstOrDefault(b =>
            b.BuildingProduct != null && b.BuildingProduct.ProductName.Contains("Barracks"));

  if (barracks == null)
 {
      Debug.LogWarning($"AI ({aiTeam}): Cannot produce {unitName} - No Barracks found! Buildings: {myBuildings.Count}");
     return;
    }

   ProductionComponent production = barracks.GetComponent<ProductionComponent>();
 if (production == null)
{
      Debug.LogError($"AI ({aiTeam}): Barracks '{barracks.gameObject.name}' has no ProductionComponent!");
     return;
  }

        // DEBUG: Log Barracks status
  Debug.Log($"AI ({aiTeam}): Barracks found - Name: {barracks.gameObject.name}, Production Queue: {production.QueueCount}/{production.MaxQueueSize}, Available Products: {production.AvailableProducts.Count}");

        if (production.QueueCount >= production.MaxQueueSize)
   {
   Debug.Log($"AI ({aiTeam}): Cannot produce {unitName} from Barracks - Queue full ({production.QueueCount}/{production.MaxQueueSize})");
    return;
        }

   // Find product
   Product product = production.AvailableProducts.Find(p => !p.IsBuilding && p.ProductName.Contains(unitName));
        if (product == null)
        {
     Debug.LogWarning($"AI ({aiTeam}): Barracks cannot produce '{unitName}'. Available units: {string.Join(", ", production.AvailableProducts.Where(p => !p.IsBuilding).Select(p => p.ProductName))}");
return;
      }

     // Check affordability
  if (resourceManager.Gold < product.GoldCost)
 {
Debug.Log($"AI ({aiTeam}): Cannot afford {unitName} from Barracks - Need {product.GoldCost}, have {resourceManager.Gold}");
  return;
   }

  bool success = production.AddToQueue(product);
        if (success)
 {
     Debug.Log($"✓ AI ({aiTeam}): Barracks producing {unitName} (Cost: {product.GoldCost} gold, Queue: {production.QueueCount}/{production.MaxQueueSize})");
   }
        else
        {
   Debug.LogError($"AI ({aiTeam}): Barracks AddToQueue({unitName}) FAILED!");
        }
    }

    /// <summary>
    /// Build a structure
    /// </summary>
    private void BuildStructure(string buildingName)
    {
     // NEW: Check if already in progress!
        if (buildingsInProgress.Contains(buildingName))
   {
       Debug.Log($"AI ({aiTeam}): {buildingName} already in progress (HashSet block)!");

     return;
    }

  // Check build cooldown
     if (Time.time - lastBuildTime < buildCooldown)
  {
  Debug.Log($"AI ({aiTeam}): Build cooldown active ({buildCooldown - (Time.time - lastBuildTime):F1}s remaining)");
  return;
 }

   if (hqProduction == null)
      {
   Debug.LogWarning($"AI ({aiTeam}): Cannot build {buildingName} - No ProductionComponent!");
     return;
   }

   if (hqProduction.QueueCount >= hqProduction.MaxQueueSize)
        {
          Debug.Log($"AI ({aiTeam}): Cannot build {buildingName} - Queue full");
      return;
   }

    Product product = hqProduction.AvailableProducts.Find(p => p.IsBuilding && p.ProductName.Contains(buildingName));

   if (product == null)
   {
  Debug.LogWarning($"AI ({aiTeam}): Cannot find building containing '{buildingName}'. Available buildings: {string.Join(", ", hqProduction.AvailableProducts.Where(p => p.IsBuilding).Select(p => p.ProductName))}");
  return;
   }

        // Check if can afford
   if (resourceManager.Gold < product.GoldCost)
      {
     Debug.Log($"AI ({aiTeam}): Cannot afford {buildingName} - Need {product.GoldCost} gold, have {resourceManager.Gold}");
   return;
        }

    // ADD TO HASHSET BEFORE AddToQueue!
        buildingsInProgress.Add(buildingName);
  Debug.Log($"AI ({aiTeam}): Added '{buildingName}' to buildingsInProgress HashSet");

        bool success = hqProduction.AddToQueue(product);
  if (success)
   {
       lastBuildTime = Time.time; // Set cooldown!
lastBuiltStructure = buildingName; // Track what we just built!
    Debug.Log($"✓ AI ({aiTeam}): Building {buildingName} (Cost: {product.GoldCost} gold) - Build cooldown started ({buildCooldown}s), Tracking: '{buildingName}', InProgress: [{string.Join(", ", buildingsInProgress)}]");
        }
        else
        {
      // REMOVE FROM HASHSET if AddToQueue failed!
       buildingsInProgress.Remove(buildingName);
      Debug.LogWarning($"{buildingName} could not be built by AI ({aiTeam}) - AddToQueue failed, removed from HashSet");
   }
    }

    /// <summary>
    /// Assign harvesters to gather resources
    /// </summary>
    private void AssignHarvestersToResources()
    {
        Collectable[] collectables = FindObjectsOfType<Collectable>();
        if (collectables.Length == 0) return;

        foreach (var unit in myUnits)
        {
            HarvesterUnit harvester = unit.GetComponent<HarvesterUnit>();
            if (harvester != null && harvester.CurrentState == HarvesterState.Idle)
            {
                // Find nearest collectable
                Collectable nearest = collectables
               .Where(c => !c.IsDepleted)
              .OrderBy(c => Vector3.Distance(harvester.transform.position, c.transform.position))
                   .FirstOrDefault();

                if (nearest != null)
                {
                    harvester.GatherFrom(nearest);
                }
            }
        }
    }

    /// <summary>
    /// Check if should launch attack
    /// </summary>
    private bool ShouldAttack()
    {
        // Don't attack if too weak
        if (currentArmySize < minArmySize)
        {
            return false;
        }

        // Find enemy units
        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();
        int enemyCount = allUnits.Count(u =>
     {
         TeamComponent team = u.GetComponent<TeamComponent>();
         return team != null && team.CurrentTeam != aiTeam && team.CurrentTeam != Team.Neutral;
     });

        // Attack if we have advantage
        float strengthRatio = enemyCount > 0 ? (float)currentArmySize / enemyCount : 1f;
        return strengthRatio >= attackThreshold;
    }

    /// <summary>
    /// Launch attack on enemy
    /// </summary>
    private void LaunchAttack()
    {
        // Find enemy headquarters or units
        BaseUnit[] enemyUnits = FindObjectsOfType<BaseUnit>()
                .Where(u =>
         {
             TeamComponent team = u.GetComponent<TeamComponent>();
             return team != null && team.CurrentTeam != aiTeam && team.CurrentTeam != Team.Neutral;
         })
       .ToArray();

        if (enemyUnits.Length == 0) return;

        // Find target (prioritize headquarters)
        BaseUnit target = enemyUnits.FirstOrDefault(u => u.IsBuilding) ?? enemyUnits[0];

        // Send military units to attack
        foreach (var unit in myUnits)
        {
            WeaponController weapon = unit.GetComponent<WeaponController>();
            Controllable controllable = unit.GetComponent<Controllable>();

            if (weapon != null && controllable != null)
            {
                controllable.MoveTo(target.transform.position);
                Debug.Log($"AI ({aiTeam}): Attacking {target.UnitName}");
            }
        }
    }

    /// <summary>
    /// Handle defensive situation
    /// </summary>
    private void HandleDefense()
    {
        // Pull back military units to defend
        Vector3 defensePoint = headquarters != null ? headquarters.transform.position : transform.position;

        foreach (var unit in myUnits)
        {
            WeaponController weapon = unit.GetComponent<WeaponController>();
            Controllable controllable = unit.GetComponent<Controllable>();

            if (weapon != null && controllable != null)
            {
                controllable.MoveTo(defensePoint);
            }
        }

        // Build more defense towers if possible
        if (resourceManager.Gold >= 200)
        {
            BuildStructure("DefenceTower");
        }
    }

    /// <summary>
    /// Check if AI is under attack
    /// </summary>
    private bool CheckIfUnderAttack()
    {
        foreach (var building in myBuildings)
        {
            Health health = building.GetComponent<Health>();
            if (health != null && health.CurrentHealth < health.MaxHealth * 0.8f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Evaluate and potentially switch strategy
    /// </summary>
    private void EvaluateStrategy()
    {
        // Switch strategy based on situation
        if (resourceManager.Gold > 1000)
        {
            currentStrategy = AIStrategy.Military; // Rich, go aggressive
        }
        else if (currentArmySize < minArmySize)
        {
            currentStrategy = AIStrategy.Military; // Need army
        }
        else if (currentHarvesterCount < idealHarvesterCount / 2)
        {
            currentStrategy = AIStrategy.Economic; // Need economy
        }
        else
        {
            currentStrategy = AIStrategy.Balanced;
        }

        Debug.Log($"AI ({aiTeam}): Strategy changed to {currentStrategy}");
    }

    /// <summary>
    /// Find AI headquarters
    /// </summary>
    private void FindHeadquarters()
    {
        BuildingComponent[] buildings = FindObjectsOfType<BuildingComponent>();
        foreach (var building in buildings)
        {
            TeamComponent team = building.GetComponent<TeamComponent>();
            if (team != null && team.CurrentTeam == aiTeam && building.IsHeadquarter)
            {
                headquarters = building.gameObject;
                hqProduction = building.GetComponent<ProductionComponent>();
                Debug.Log($"AI ({aiTeam}): Found headquarters");
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (headquarters != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headquarters.transform.position, 5f);

            UnityEditor.Handles.Label(headquarters.transform.position + Vector3.up * 8f,
      $"AI ({aiTeam})\n" +
                $"State: {currentState}\n" +
              $"Strategy: {currentStrategy}\n" +
            $"Harvesters: {currentHarvesterCount}/{idealHarvesterCount}\n" +
   $"Army: {currentArmySize}/{idealArmySize}\n" +
          $"Gold: {(resourceManager != null ? resourceManager.Gold : 0)}");
        }
#endif
    }
}

/// <summary>
/// AI difficulty levels
/// </summary>
public enum AIDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert
}

/// <summary>
/// AI strategy types
/// </summary>
public enum AIStrategy
{
    Economic,   // Focus on resource gathering
    Military,   // Focus on army building
    Balanced    // Mix of both
}

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
