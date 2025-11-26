using UnityEngine;

/// <summary>
/// Building that collects resources from harvesters
/// </summary>
[RequireComponent(typeof(BuildingComponent))]
public class ResourceCollector : MonoBehaviour
{
    [Header("Collector Settings")]
    [SerializeField] private float unloadRange = 3f;
    [SerializeField] private float unloadTime = 2f;
    [SerializeField] private bool acceptAllResources = true;
    [SerializeField] private ResourceType[] acceptedResources = { ResourceType.Gold };

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager; // Own ResourceManager reference

    [Header("Visual Feedback")]
    [SerializeField] private GameObject unloadEffect;
    [SerializeField] private AudioClip unloadSound;
    [SerializeField] private Transform unloadPoint;

    [Header("Statistics")]
    [SerializeField] private int totalCollectedGold = 0;
    [SerializeField] private int totalCollectedFood = 0;
    [SerializeField] private int totalCollectedWood = 0;
    [SerializeField] private int totalCollectedStone = 0;

    private BuildingComponent buildingComponent;

    // Properties
    public float UnloadRange => unloadRange;
    public float UnloadTime => unloadTime;
    public int TotalCollectedGold => totalCollectedGold;

    void Awake()
    {
        buildingComponent = GetComponent<BuildingComponent>();

        // Auto-find team-specific ResourceManager if not set
        if (resourceManager == null)
        {
            TeamComponent myTeam = GetComponent<TeamComponent>();

            if (myTeam != null)
            {
                ResourceManager[] allManagers = FindObjectsOfType<ResourceManager>();

                // Find matching team-specific manager
                foreach (var manager in allManagers)
                {
                    bool isAIManager = manager.gameObject.name.Contains("AI");
                    bool needsAIManager = myTeam.CurrentTeam != Team.Player;

                    if (isAIManager == needsAIManager)
                    {
                        resourceManager = manager;
                        Debug.Log($"{gameObject.name}: Found team-specific ResourceManager: {manager.gameObject.name} for team {myTeam.CurrentTeam}");
                        break;
                    }
                }
            }

            // Fallback
            if (resourceManager == null)
            {
                resourceManager = FindObjectOfType<ResourceManager>();
                Debug.LogWarning($"{gameObject.name}: Using fallback ResourceManager: {(resourceManager != null ? resourceManager.gameObject.name : "NONE")}");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name}: ResourceManager already assigned: {resourceManager.gameObject.name}");
        }
    }

    /// <summary>
    /// Deposit resources from harvester
    /// </summary>
    public void DepositResources(ResourceType resourceType, int amount, ResourceManager harvesterResourceManager)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"{gameObject.name}: DepositResources called with amount={amount} - ignoring");
            return;
        }

        // Check if we accept this resource type
        if (!acceptAllResources && !System.Array.Exists(acceptedResources, r => r == resourceType))
        {
            Debug.LogWarning($"{gameObject.name}: Does not accept {resourceType}!");
            return;
        }

        // IMPORTANT: Use OUR OWN ResourceManager, not the one from harvester!
        // This ensures resources go to the correct team
        ResourceManager targetManager = resourceManager; // Use our own!

        if (targetManager == null)
        {
            Debug.LogError($"{gameObject.name}: OWN ResourceManager is NULL! Cannot deposit resources!");
            Debug.LogError($"  Harvester passed ResourceManager: {(harvesterResourceManager != null ? harvesterResourceManager.gameObject.name : "NULL")}");
            return;
        }

        Debug.Log($"=== {gameObject.name}: DepositResources START ===");
        Debug.Log($"  ResourceType: {resourceType}");
        Debug.Log($"  Amount: {amount}");
        Debug.Log($"  Using OWN ResourceManager: {targetManager.gameObject.name}");
        Debug.Log($"  (Harvester ResourceManager was: {(harvesterResourceManager != null ? harvesterResourceManager.gameObject.name : "NULL")})");
        Debug.Log($"  ResourceManager Gold BEFORE: {targetManager.Gold}");

        // Add to resource manager
        switch (resourceType)
        {
            case ResourceType.Gold:
                int goldBefore = targetManager.Gold;
                targetManager.AddResources(0, 0, 0, amount);
                int goldAfter = targetManager.Gold;
                totalCollectedGold += amount;
                Debug.Log($"  ? Gold: {goldBefore} + {amount} = {goldAfter}");
                break;
            case ResourceType.Food:
                targetManager.AddResources(amount, 0, 0, 0);
                totalCollectedFood += amount;
                Debug.Log($"  ? Food: +{amount}");
                break;
            case ResourceType.Wood:
                targetManager.AddResources(0, amount, 0, 0);
                totalCollectedWood += amount;
                Debug.Log($"  ? Wood: +{amount}");
                break;
            case ResourceType.Stone:
                targetManager.AddResources(0, 0, amount, 0);
                totalCollectedStone += amount;
                Debug.Log($"  ? Stone: +{amount}");
                break;
        }

        Debug.Log($"  ResourceManager Gold AFTER: {targetManager.Gold}");
        Debug.Log($"{gameObject.name}: Total {resourceType} collected: {GetTotalCollected(resourceType)}");
        Debug.Log($"=== DepositResources END ===");

        // Visual feedback
        PlayUnloadEffect();
    }

    /// <summary>
    /// Get total collected of specific resource type
    /// </summary>
    public int GetTotalCollected(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Gold:
                return totalCollectedGold;
            case ResourceType.Food:
                return totalCollectedFood;
            case ResourceType.Wood:
                return totalCollectedWood;
            case ResourceType.Stone:
                return totalCollectedStone;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Play unload effect
    /// </summary>
    private void PlayUnloadEffect()
    {
        Vector3 effectPosition = unloadPoint != null ? unloadPoint.position : transform.position;

        if (unloadEffect != null)
        {
            Instantiate(unloadEffect, effectPosition, Quaternion.identity);
        }

        if (unloadSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot2D(unloadSound, AudioManager.AudioCategory.SFX);
            }
            else
            {
                AudioSource.PlayClipAtPoint(unloadSound, effectPosition);
            }
        }
    }

    /// <summary>
    /// Check if harvester is in unload range
    /// </summary>
    public bool IsInUnloadRange(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= unloadRange;
    }

    void OnDrawGizmos()
    {
        // Draw unload range
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, unloadRange);
    }

    void OnDrawGizmosSelected()
    {
        // Draw detailed info
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, unloadRange);

#if UNITY_EDITOR
        string info = $"Resource Collector\n";
        info += $"Gold: {totalCollectedGold}\n";
        info += $"Food: {totalCollectedFood}\n";
        info += $"Wood: {totalCollectedWood}\n";
        info += $"Stone: {totalCollectedStone}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, info);
#endif
    }
}
