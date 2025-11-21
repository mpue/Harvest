using UnityEngine;

/// <summary>
/// Debug helper to verify ResourceManager events are firing correctly
/// </summary>
public class ResourceManagerDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;

    [Header("Debug Options")]
    [SerializeField] private bool logResourceChanges = true;
    [SerializeField] private bool logEnergyChanges = true;
    [SerializeField] private bool showOnScreenDebug = true;
    [SerializeField] private bool verboseLogging = true;

    private string lastResourceChange = "";
    private string lastEnergyChange = "";
    private float lastResourceChangeTime;
    private float lastEnergyChangeTime;
    private int energyEventCount = 0;
    private int resourceEventCount = 0;

    void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }
    }

    void Start()
    {
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManagerDebugger: No ResourceManager found!");
            return;
        }

        // Subscribe to events
        resourceManager.OnResourcesChanged += OnResourcesChanged;
        resourceManager.OnEnergyChanged += OnEnergyChanged;

        Debug.Log("? ResourceManagerDebugger: Subscribed to ResourceManager events");
        Debug.Log($"  Initial Gold: {resourceManager.Gold}");
        Debug.Log($"  Initial Energy: {resourceManager.CurrentEnergy}/{resourceManager.MaxEnergy}");
        Debug.Log($"  Initial Available Energy: {resourceManager.AvailableEnergy}");
    }

    private void OnResourcesChanged(int food, int wood, int stone, int gold)
    {
        resourceEventCount++;
        lastResourceChange = $"Resources Changed: Food={food}, Wood={wood}, Stone={stone}, Gold={gold}";
        lastResourceChangeTime = Time.time;

        if (logResourceChanges)
        {
            Debug.Log($"?? [{resourceEventCount}] {lastResourceChange}");
        }
    }

    private void OnEnergyChanged(int current, int max)
    {
        energyEventCount++;
        int available = max - current;
        lastEnergyChange = $"Energy Changed: Current={current}, Max={max}, Available={available}";
        lastEnergyChangeTime = Time.time;

        if (logEnergyChanges)
        {
            Debug.Log($"? [{energyEventCount}] {lastEnergyChange}");

            if (verboseLogging)
            {
                // Check if ResourceBarUI is updating
                ResourceBarUI resourceBarUI = FindFirstObjectByType<ResourceBarUI>();
                if (resourceBarUI != null)
                {
                    Debug.Log($"   ? ResourceBarUI found and should update now");
                }
                else
                {
                    Debug.LogWarning($"   ? ResourceBarUI NOT FOUND! UI won't update!");
                }
            }
        }
    }

    void OnGUI()
    {
        if (!showOnScreenDebug || resourceManager == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 12;
        style.alignment = TextAnchor.UpperLeft;

        string debugInfo = "=== Resource Manager Debug ===\n\n";

        debugInfo += "Current Values:\n";
        debugInfo += $"  Gold: {resourceManager.Gold}\n";
        debugInfo += $"  Food: {resourceManager.Food}\n";
        debugInfo += $"  Wood: {resourceManager.Wood}\n";
        debugInfo += $"  Stone: {resourceManager.Stone}\n";
        debugInfo += $"  Energy Current: {resourceManager.CurrentEnergy}\n";
        debugInfo += $"  Energy Max: {resourceManager.MaxEnergy}\n";
        debugInfo += $"  Energy Available: {resourceManager.AvailableEnergy}\n\n";

        // Event counts
        debugInfo += "Event Counts:\n";
        debugInfo += $"  Resource Events: {resourceEventCount}\n";
        debugInfo += $"  Energy Events: {energyEventCount}\n\n";

        // Check for ResourceBarUI
        debugInfo += "Resource Bar UI:\n";
        ResourceBarUI resourceBarUI = FindFirstObjectByType<ResourceBarUI>();
        if (resourceBarUI != null)
        {
            debugInfo += "  ? ResourceBarUI found\n\n";
        }
        else
        {
            debugInfo += "  ? ResourceBarUI NOT FOUND!\n\n";
        }

        if (Time.time - lastResourceChangeTime < 5f)
        {
            debugInfo += $"Last Resource Change ({(Time.time - lastResourceChangeTime):F1}s ago):\n";
            debugInfo += $"  {lastResourceChange}\n\n";
        }

        if (Time.time - lastEnergyChangeTime < 5f)
        {
            debugInfo += $"Last Energy Change ({(Time.time - lastEnergyChangeTime):F1}s ago):\n";
            debugInfo += $"  {lastEnergyChange}\n";
        }

        GUI.Box(new Rect(10, 450, 400, 280), debugInfo, style);

        // Test buttons
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 10;

        if (GUI.Button(new Rect(10, 740, 120, 30), "Test: +100 Gold", buttonStyle))
        {
            resourceManager.AddResources(0, 0, 0, 100);
            Debug.Log("?? TEST: Added 100 Gold");
        }

        if (GUI.Button(new Rect(140, 740, 120, 30), "Test: +10 Energy", buttonStyle))
        {
            resourceManager.IncreaseMaxEnergy(10);
            Debug.Log("?? TEST: Increased Max Energy by 10");
        }

        if (GUI.Button(new Rect(270, 740, 120, 30), "Test: Consume 5", buttonStyle))
        {
            bool success = resourceManager.ConsumeEnergy(5);
            Debug.Log($"?? TEST: Consumed 5 Energy - Success: {success}");
        }

        if (GUI.Button(new Rect(10, 775, 120, 30), "Reset Counters", buttonStyle))
        {
            energyEventCount = 0;
            resourceEventCount = 0;
            Debug.Log("?? Counters reset");
        }
    }

    void OnDestroy()
    {
        if (resourceManager != null)
        {
            resourceManager.OnResourcesChanged -= OnResourcesChanged;
            resourceManager.OnEnergyChanged -= OnEnergyChanged;
        }
    }

    [ContextMenu("Test Add Gold")]
    void TestAddGold()
    {
        if (resourceManager != null)
        {
            resourceManager.AddResources(0, 0, 0, 100);
        }
    }

    [ContextMenu("Test Increase Energy")]
    void TestIncreaseEnergy()
    {
        if (resourceManager != null)
        {
            resourceManager.IncreaseMaxEnergy(10);
        }
    }

    [ContextMenu("Test Spend Resources")]
    void TestSpendResources()
    {
        if (resourceManager != null)
        {
            resourceManager.SpendResources(50);
        }
    }

    [ContextMenu("Log Current State")]
    void LogCurrentState()
    {
        if (resourceManager != null)
        {
            Debug.Log("=== Current Resource Manager State ===");
            Debug.Log($"Gold: {resourceManager.Gold}");
            Debug.Log($"Energy Current: {resourceManager.CurrentEnergy}");
            Debug.Log($"Energy Max: {resourceManager.MaxEnergy}");
            Debug.Log($"Energy Available: {resourceManager.AvailableEnergy}");
            Debug.Log($"Resource Events Fired: {resourceEventCount}");
            Debug.Log($"Energy Events Fired: {energyEventCount}");
        }
    }
}
