using UnityEngine;

/// <summary>
/// Base class for all AI tasks
/// </summary>
public abstract class AITask
{
    public string TaskName { get; protected set; }
    public int Priority { get; set; }
    public bool IsComplete { get; protected set; }
    public bool CanExecute { get; protected set; }

    protected IAIController controller;
    protected ResourceManager resourceManager;

    public AITask(IAIController controller, int priority = 0)
    {
        this.controller = controller;
        this.resourceManager = controller.ResourceManager;
        this.Priority = priority;
        this.IsComplete = false;
        this.CanExecute = true;
    }

    /// <summary>
    /// Check if this task can be executed right now
    /// </summary>
    public abstract bool CheckConditions();

    /// <summary>
    /// Execute the task
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Reset the task to be executed again
    /// </summary>
    public virtual void Reset()
    {
        IsComplete = false;
        CanExecute = true;
    }

    /// <summary>
    /// Get debug info about this task
    /// </summary>
    public virtual string GetDebugInfo()
    {
        return $"[{TaskName}] Priority={Priority}, Complete={IsComplete}, CanExecute={CanExecute}";
    }
}
