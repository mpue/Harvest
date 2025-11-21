using UnityEngine;

/// <summary>
/// Task to produce units from a specific building
/// </summary>
public class ProduceUnitTask : AITask
{
    private string unitName;
    private string producerBuildingName;
    private int requiredGold;
    private int requiredEnergy;
    private int targetCount;
    private System.Func<int> getCurrentCount;

    public ProduceUnitTask(
        IAIController controller,
        string unitName,
        string producerBuildingName,
        int requiredGold,
        int requiredEnergy,
        int priority,
        int targetCount,
        System.Func<int> getCurrentCount
  ) : base(controller, priority)
    {
        this.unitName = unitName;
        this.producerBuildingName = producerBuildingName;
        this.requiredGold = requiredGold;
        this.requiredEnergy = requiredEnergy;
        this.targetCount = targetCount;
        this.getCurrentCount = getCurrentCount;
        TaskName = $"Produce_{unitName}";
    }

    public override bool CheckConditions()
    {
        // Check if we've reached target count
        int currentCount = getCurrentCount();
        if (currentCount >= targetCount)
        {
            IsComplete = true;
            CanExecute = false;
            Debug.Log($"ProduceUnitTask: {unitName} target reached ({currentCount}/{targetCount}) - marking complete");
            return false;
        }

        // Check if producer building exists
        if (!controller.HasBuilding(producerBuildingName))
        {
            CanExecute = false;
            Debug.LogWarning($"ProduceUnitTask: {unitName} cannot be produced - no {producerBuildingName} found");
            return false;
        }

        // Check resources
        if (resourceManager.Gold < requiredGold)
        {
            CanExecute = false;
            return false;
        }

        if (resourceManager.AvailableEnergy < requiredEnergy)
        {
            CanExecute = false;
            return false;
        }

        // Check if producer queue is full
        if (controller.IsProductionQueueFull(producerBuildingName))
        {
            CanExecute = false;
            return false;
        }

        CanExecute = true;
        return true;
    }

    public override void Execute()
    {
        int currentCount = getCurrentCount();
        Debug.Log($"AI Task: Producing {unitName} from {producerBuildingName} ({currentCount}/{targetCount}, Priority: {Priority})");

        bool success = controller.ProduceUnit(producerBuildingName, unitName);

        if (success)
        {
            // Don't mark complete - let it produce multiple times until target reached
        }
        else
        {
            Debug.LogWarning($"AI Task: Failed to produce {unitName}");
        }
    }

    public override void Reset()
    {
        base.Reset();
        // This task can be reset to produce more units
    }

    public override string GetDebugInfo()
    {
        int current = getCurrentCount();
        return $"{base.GetDebugInfo()}, Unit={unitName}, Producer={producerBuildingName}, Count={current}/{targetCount}, Gold={resourceManager.Gold}/{requiredGold}, Energy={resourceManager.AvailableEnergy}/{requiredEnergy}";
    }
}
