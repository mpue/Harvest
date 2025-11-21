using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages and executes AI tasks in priority order
/// </summary>
public class AITaskManager
{
    private List<AITask> tasks = new List<AITask>();
    private IAIController controller;
    private float taskCheckInterval = 0.5f;
    private float lastTaskCheckTime = 0f;

    public AITaskManager(IAIController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// Add a task to the manager
    /// </summary>
    public void AddTask(AITask task)
    {
        if (!tasks.Contains(task))
        {
            tasks.Add(task);
            SortTasks();
            Debug.Log($"AI TaskManager: Added task '{task.TaskName}' with priority {task.Priority}");
        }
    }

    /// <summary>
    /// Remove a task
    /// </summary>
    public void RemoveTask(AITask task)
    {
        tasks.Remove(task);
    }

    /// <summary>
    /// Clear all tasks
    /// </summary>
    public void ClearTasks()
    {
        tasks.Clear();
    }

    /// <summary>
    /// Sort tasks by priority (higher = more important)
    /// </summary>
    private void SortTasks()
    {
        tasks = tasks.OrderByDescending(t => t.Priority).ToList();
    }

    /// <summary>
    /// Update and execute tasks
    /// </summary>
    public void Update()
    {
        // Check tasks at intervals to reduce CPU load
        if (Time.time - lastTaskCheckTime < taskCheckInterval)
        {
            return;
        }

        lastTaskCheckTime = Time.time;

        // Remove completed tasks
        tasks.RemoveAll(t => t.IsComplete);

        // Sort by priority
        SortTasks();

        // Try to execute highest priority task that can run
        foreach (var task in tasks)
        {
            if (task.CheckConditions())
            {
                task.Execute();
                break; // Execute only one task per update
            }
        }
    }

    /// <summary>
    /// Get debug info about all tasks
    /// </summary>
    public string GetDebugInfo()
    {
        if (tasks.Count == 0)
        {
            return "No active tasks";
        }

        string info = $"Active Tasks ({tasks.Count}):\n";
        foreach (var task in tasks)
        {
            info += $"  {task.GetDebugInfo()}\n";
        }
        return info;
    }

    /// <summary>
    /// Get count of active tasks
    /// </summary>
    public int ActiveTaskCount => tasks.Count(t => !t.IsComplete);

    /// <summary>
    /// Get highest priority task
    /// </summary>
    public AITask GetNextTask()
    {
        return tasks.FirstOrDefault(t => !t.IsComplete && t.CanExecute);
    }
}
