using UnityEngine;

/// <summary>
/// Task to build a specific structure
/// </summary>
public class BuildStructureTask : AITask
{
    private string structureName;
    private int requiredGold;
    private int requiredEnergy;
    private int maxCount;
    private bool checkExisting;

    public BuildStructureTask(
        IAIController controller,
        string structureName,
        int requiredGold,
        int requiredEnergy,
        int priority,
        int maxCount = 1,
        bool checkExisting = true
    ) : base(controller, priority)
    {
        this.structureName = structureName;
        this.requiredGold = requiredGold;
        this.requiredEnergy = requiredEnergy;
        this.maxCount = maxCount;
        this.checkExisting = checkExisting;
        TaskName = $"Build_{structureName}";
    }

    public override bool CheckConditions()
    {
        // Check if already exists (if we should check)
        if (checkExisting)
        {
            int existingCount = controller.GetBuildingCount(structureName);
            if (existingCount >= maxCount)
            {
                IsComplete = true;
                CanExecute = false;
                return false;
            }
        }

        // Check if already in progress
        if (controller.IsBuildingInProgress(structureName))
        {
            CanExecute = false;
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

        // Check build cooldown
        if (!controller.CanBuildNow())
        {
            CanExecute = false;
            return false;
        }

        CanExecute = true;
        return true;
    }

    public override void Execute()
    {
        Debug.Log($"AI Task: Building {structureName} (Priority: {Priority})");

        bool success = controller.BuildStructure(structureName);

        if (success)
        {
            // Don't mark as complete yet - wait for building to finish
            CanExecute = false; // But prevent re-execution
        }
        else
        {
            Debug.LogWarning($"AI Task: Failed to build {structureName}");
            CanExecute = false;
        }
    }

    public override string GetDebugInfo()
    {
        int existing = checkExisting ? controller.GetBuildingCount(structureName) : 0;
        return $"{base.GetDebugInfo()}, Structure={structureName}, Existing={existing}/{maxCount}, Gold={resourceManager.Gold}/{requiredGold}, Energy={resourceManager.AvailableEnergy}/{requiredEnergy}";
    }
}
