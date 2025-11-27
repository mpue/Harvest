using UnityEngine;

/// <summary>
/// Builds task sequences for different AI strategies
/// </summary>
public class AIStrategyBuilder
{
    private IAIController controller;
    private AITaskManager taskManager;

    public AIStrategyBuilder(IAIController controller, AITaskManager taskManager)
    {
        this.controller = controller;
        this.taskManager = taskManager;
    }

    /// <summary>
    /// Build early game strategy (focus on economy)
    /// </summary>
    public void BuildEarlyGameStrategy()
    {
        Debug.Log("AI: Building Early Game Strategy (AGGRESSIVE)");
        taskManager.ClearTasks();

        // PRIORITY 100: Build 3 Energy Blocks (FOUNDATION!)
        taskManager.AddTask(new BuildStructureTask(
            controller,
            "EnergyBlock",
            requiredGold: 100,
            requiredEnergy: 0,
            priority: 100,
            maxCount: 3,
            checkExisting: true
        ));

        // PRIORITY 95: Build Factory FAST
        taskManager.AddTask(new BuildStructureTask(
            controller,
            "Factory",
            requiredGold: 250,
            requiredEnergy: 10,
            priority: 95,
            maxCount: 1,
            checkExisting: true
        ));

        // PRIORITY 90: Build Resource Collector EARLY
        taskManager.AddTask(new BuildStructureTask(
            controller,
            "ResourceCollector",
            requiredGold: 200,
            requiredEnergy: 10,
            priority: 90,
            maxCount: 1,
            checkExisting: true
        ));

        // PRIORITY 85: Only 2 Harvesters initially (REDUCED from 3)
        taskManager.AddTask(new ProduceUnitTask(
            controller,
            unitName: "Harvester",
            producerBuildingName: "Factory",
            requiredGold: 100,
            requiredEnergy: 0,
            priority: 85,
            targetCount: 2, // REDUCED!
            getCurrentCount: () => controller.GetUnitCount("Harvester")
        ));

        // PRIORITY 80: Start building MK3 Tanks EARLY
        taskManager.AddTask(new ProduceUnitTask(
            controller,
            unitName: "MK3",
            producerBuildingName: "Factory",
            requiredGold: 250,
            requiredEnergy: 10,
            priority: 80,
            targetCount: 3, // First wave
            getCurrentCount: () => controller.GetUnitCount("MK3")
        ));

        // PRIORITY 70: Build Barracks
        taskManager.AddTask(new BuildStructureTask(
            controller,
            "Barracks",
            requiredGold: 250,
            requiredEnergy: 10,
            priority: 70,
            maxCount: 1,
            checkExisting: true
        ));

        // PRIORITY 60: More MK3 Tanks (aggressive!)
        taskManager.AddTask(new ProduceUnitTask(
            controller,
            unitName: "MK3",
            producerBuildingName: "Factory",
            requiredGold: 250,
            requiredEnergy: 10,
            priority: 60,
            targetCount: 6, // Second wave
            getCurrentCount: () => controller.GetUnitCount("MK3")
        ));

        // PRIORITY 50: Only add ONE more Harvester later (total 3)
        taskManager.AddTask(new ProduceUnitTask(
            controller,
            unitName: "Harvester",
            producerBuildingName: "Factory",
            requiredGold: 100,
            requiredEnergy: 0,
            priority: 50,
            targetCount: 3, // REDUCED from 5!
            getCurrentCount: () => controller.GetUnitCount("Harvester")
        ));

        // PRIORITY 40: Even MORE tanks!
        taskManager.AddTask(new ProduceUnitTask(
            controller,
            unitName: "MK3",
            producerBuildingName: "Factory",
            requiredGold: 250,
            requiredEnergy: 10,
            priority: 40,
            targetCount: 10, // Big army!
            getCurrentCount: () => controller.GetUnitCount("MK3")
        ));

        Debug.Log($"AI: AGGRESSIVE Early Game Strategy loaded with {taskManager.ActiveTaskCount} tasks");
    }

    /// <summary>
    /// Build mid game strategy (balanced)
    /// </summary>
    public void BuildMidGameStrategy()
    {
        Debug.Log("AI: Building Mid Game Strategy");
        taskManager.ClearTasks();

        // Expand energy
        taskManager.AddTask(new BuildStructureTask(
         controller, "EnergyBlock", 100, 0, 100, 5, true
        ));

        // More collectors
        taskManager.AddTask(new BuildStructureTask(
            controller, "ResourceCollector", 200, 10, 90, 2, true
        ));

        // More harvesters
        taskManager.AddTask(new ProduceUnitTask(
            controller, "Harvester", "Factory", 100, 5, 80, 8,
            () => controller.GetUnitCount("Harvester")
        ));

        // Build army
        taskManager.AddTask(new ProduceUnitTask(
            controller, "MK3", "Factory", 250, 10, 70, 5,
            () => controller.GetUnitCount("MK3")
        ));

        taskManager.AddTask(new ProduceUnitTask(
            controller, "Soldier", "Barracks", 150, 8, 60, 10,
            () => controller.GetUnitCount("Soldier")
        ));

        // Defense
        taskManager.AddTask(new BuildStructureTask(
            controller, "DefenceTower", 200, 8, 50, 3, true
        ));

        Debug.Log($"AI: Mid Game Strategy loaded with {taskManager.ActiveTaskCount} tasks");
    }

    /// <summary>
    /// Build late game strategy (military focused)
    /// </summary>
    public void BuildLateGameStrategy()
    {
        Debug.Log("AI: Building Late Game Strategy");
        taskManager.ClearTasks();

        // Ensure energy
        taskManager.AddTask(new BuildStructureTask(
            controller, "EnergyBlock", 100, 0, 100, 6, true
        ));

        // Mass produce army
        taskManager.AddTask(new ProduceUnitTask(
            controller, "MK3", "Factory", 250, 10, 90, 10,
           () => controller.GetUnitCount("MK3")
         ));

        taskManager.AddTask(new ProduceUnitTask(
            controller, "Soldier", "Barracks", 150, 8, 80, 15,
            () => controller.GetUnitCount("Soldier")
        ));

        // Heavy defense
        taskManager.AddTask(new BuildStructureTask(
    controller, "DefenceTower", 200, 8, 70, 5, true
    ));

        Debug.Log($"AI: Late Game Strategy loaded with {taskManager.ActiveTaskCount} tasks");
    }

    /// <summary>
    /// Build economic strategy (maximum harvesters)
    /// </summary>
    public void BuildEconomicStrategy()
    {
        Debug.Log("AI: Building Economic Strategy");
        taskManager.ClearTasks();

        // Lots of energy
        taskManager.AddTask(new BuildStructureTask(
          controller, "EnergyBlock", 100, 0, 100, 6, true
        ));

        // Multiple collectors
        taskManager.AddTask(new BuildStructureTask(
            controller, "ResourceCollector", 200, 10, 90, 3, true
        ));

        // Maximum harvesters
        taskManager.AddTask(new ProduceUnitTask(
            controller, "Harvester", "Factory", 100, 5, 80, 10,
            () => controller.GetUnitCount("Harvester")
        ));

        // Minimal army
        taskManager.AddTask(new ProduceUnitTask(
                  controller, "Soldier", "Barracks", 150, 8, 50, 5,
                  () => controller.GetUnitCount("Soldier")
         ));

        Debug.Log($"AI: Economic Strategy loaded with {taskManager.ActiveTaskCount} tasks");
    }

    /// <summary>
    /// Build military strategy (rush)
    /// </summary>
    public void BuildMilitaryStrategy()
    {
        Debug.Log("AI: Building Military Strategy");
        taskManager.ClearTasks();

        // Minimal energy
        taskManager.AddTask(new BuildStructureTask(
            controller, "EnergyBlock", 100, 0, 100, 4, true
        ));

        // Fast Barracks
        taskManager.AddTask(new BuildStructureTask(
            controller, "Barracks", 250, 10, 90, 1, true
        ));

        // Rush soldiers
        taskManager.AddTask(new ProduceUnitTask(
            controller, "Soldier", "Barracks", 150, 8, 80, 15,
            () => controller.GetUnitCount("Soldier")
        ));

        // Tanks support
        taskManager.AddTask(new ProduceUnitTask(
            controller, "MK3", "Factory", 250, 10, 70, 5,
            () => controller.GetUnitCount("MK3")
        ));

        // Minimal harvesters
        taskManager.AddTask(new ProduceUnitTask(
         controller, "Harvester", "Factory", 100, 5, 60, 3,
           () => controller.GetUnitCount("Harvester")
         ));

        Debug.Log($"AI: Military Strategy loaded with {taskManager.ActiveTaskCount} tasks");
    }
}
