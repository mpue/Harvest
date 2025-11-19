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
    }

    /// <summary>
    /// Deposit resources from harvester
    /// </summary>
    public void DepositResources(ResourceType resourceType, int amount, ResourceManager resourceManager)
    {
        if (amount <= 0)
        {
            return;
        }

        // Check if we accept this resource type
        if (!acceptAllResources && !System.Array.Exists(acceptedResources, r => r == resourceType))
        {
            Debug.LogWarning($"{gameObject.name}: Does not accept {resourceType}!");
            return;
        }

        // Check if resourceManager is valid
        if (resourceManager == null)
        {
            Debug.LogError($"{gameObject.name}: ResourceManager is NULL! Cannot deposit resources!");
            return;
        }

        Debug.Log($"{gameObject.name}: Depositing {amount} {resourceType} to ResourceManager '{resourceManager.gameObject.name}'");

        // Add to resource manager
        switch (resourceType)
        {
            case ResourceType.Gold:
                int goldBefore = resourceManager.Gold;
                resourceManager.AddResources(0, 0, 0, amount);
                int goldAfter = resourceManager.Gold;
                totalCollectedGold += amount;
                Debug.Log($"  ? Gold: {goldBefore} + {amount} = {goldAfter} ?");
                break;
            case ResourceType.Food:
                resourceManager.AddResources(amount, 0, 0, 0);
                totalCollectedFood += amount;
                Debug.Log($"  ? Food: +{amount} ?");
                break;
            case ResourceType.Wood:
                resourceManager.AddResources(0, amount, 0, 0);
                totalCollectedWood += amount;
                Debug.Log($"  ? Wood: +{amount} ?");
                break;
            case ResourceType.Stone:
                resourceManager.AddResources(0, 0, amount, 0);
                totalCollectedStone += amount;
                Debug.Log($"  ? Stone: +{amount} ?");
                break;
        }

        Debug.Log($"{gameObject.name}: Total {resourceType} collected: {GetTotalCollected(resourceType)}");

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
