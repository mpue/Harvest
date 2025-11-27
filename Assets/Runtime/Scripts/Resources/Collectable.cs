using UnityEngine;

/// <summary>
/// Collectable resource that can be harvested
/// </summary>
public class Collectable : MonoBehaviour
{
    [Header("Resource Settings")]
    [SerializeField] private ResourceType resourceType = ResourceType.Gold;
    [SerializeField] private int resourceAmount = 100;
    [SerializeField] private int currentAmount;

    [Header("Harvest Settings")]
    [SerializeField] private float harvestTime = 2f;
    [SerializeField] private int amountPerHarvest = 10;

    [Header("Harvest Slots")]
    [SerializeField] private int maxHarvesters = 4; // Maximum harvesters that can work simultaneously
    [SerializeField] private float slotRadius = 2f; // Radius around resource for harvest positions
    [SerializeField] private bool showSlotGizmos = true;

    [Header("Visual Settings")]
    [SerializeField] private GameObject visualModel;
    [SerializeField] private bool depleteVisually = true;
    [SerializeField] private Vector3 depletedScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Effects")]
    [SerializeField] private GameObject harvestEffect;
    [SerializeField] private GameObject depleteEffect;
    [SerializeField] private AudioClip harvestSound;
    [SerializeField] private AudioClip depleteSound;

    private Vector3 originalScale;
    private bool isDepleted = false;

    // Harvest slot management
    private HarvesterUnit[] harvestSlots; // Track which harvester is in which slot
    private Vector3[] slotPositions; // Pre-calculated positions around resource

    // Properties
    public ResourceType ResourceType => resourceType;
    public int CurrentAmount => currentAmount;
    public int ResourceAmount => resourceAmount;
    public bool IsDepleted => isDepleted;
    public float HarvestTime => harvestTime;
    public int AmountPerHarvest => amountPerHarvest;
    public bool HasAvailableSlot => GetAvailableSlotIndex() != -1;

    void Awake()
    {
        currentAmount = resourceAmount;
        if (visualModel != null)
        {
            originalScale = visualModel.transform.localScale;
        }

        // Initialize harvest slots
        harvestSlots = new HarvesterUnit[maxHarvesters];
        slotPositions = new Vector3[maxHarvesters];
        CalculateSlotPositions();
    }

    /// <summary>
    /// Calculate harvest slot positions around the resource
    /// </summary>
    private void CalculateSlotPositions()
    {
        for (int i = 0; i < maxHarvesters; i++)
        {
            float angle = (360f / maxHarvesters) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * slotRadius, 0, Mathf.Sin(angle) * slotRadius);
            slotPositions[i] = transform.position + offset;
        }
    }

    /// <summary>
    /// Get an available harvest slot index
    /// </summary>
    private int GetAvailableSlotIndex()
    {
        for (int i = 0; i < harvestSlots.Length; i++)
        {
            if (harvestSlots[i] == null)
                return i;
        }
        return -1; // No available slots
    }

    /// <summary>
    /// Request a harvest position (returns Vector3.zero if no slots available)
    /// </summary>
    public Vector3 RequestHarvestPosition(HarvesterUnit harvester)
    {
        // Check if harvester already has a slot
        for (int i = 0; i < harvestSlots.Length; i++)
        {
            if (harvestSlots[i] == harvester)
            {
                // Debug.Log($"{harvester.name} already has slot {i} at {slotPositions[i]}");
                return slotPositions[i];
            }
        }

        // Find available slot
        int slotIndex = GetAvailableSlotIndex();
        if (slotIndex == -1)
        {
            Debug.LogWarning($"{harvester.name} cannot harvest {gameObject.name} - all slots full!");
            return Vector3.zero;
        }

        // Assign slot
        harvestSlots[slotIndex] = harvester;
        Debug.Log($"✓ {harvester.name} assigned to slot {slotIndex} at {slotPositions[slotIndex]}");
        return slotPositions[slotIndex];
    }

    /// <summary>
    /// Release a harvest slot when harvester leaves
    /// </summary>
    public void ReleaseHarvestSlot(HarvesterUnit harvester)
    {
        for (int i = 0; i < harvestSlots.Length; i++)
        {
            if (harvestSlots[i] == harvester)
            {
                harvestSlots[i] = null;
                Debug.Log($"✓ {harvester.name} released slot {i}");
                return;
            }
        }
    }

    void Update()
    {
        // Clean up null references (harvesters that were destroyed)
        for (int i = 0; i < harvestSlots.Length; i++)
        {
            if (harvestSlots[i] != null && harvestSlots[i].gameObject == null)
            {
                harvestSlots[i] = null;
            }
        }
    }

    /// <summary>
    /// Harvest resources from this collectable
    /// </summary>
    public int Harvest(int requestedAmount)
    {
        if (isDepleted)
        {
            return 0;
        }

        int harvestedAmount = Mathf.Min(requestedAmount, currentAmount);
        currentAmount -= harvestedAmount;

        // Visual feedback
        PlayHarvestEffect();
        UpdateVisuals();

        // Check if depleted
        if (currentAmount <= 0)
        {
            Deplete();
        }

        Debug.Log($"{gameObject.name}: Harvested {harvestedAmount} {resourceType}. Remaining: {currentAmount}");

        return harvestedAmount;
    }

    /// <summary>
    /// Update visual representation based on remaining resources
    /// </summary>
    private void UpdateVisuals()
    {
        if (!depleteVisually || visualModel == null)
        {
            return;
        }

        float percentage = (float)currentAmount / resourceAmount;
        Vector3 targetScale = Vector3.Lerp(depletedScale, originalScale, percentage);
        visualModel.transform.localScale = targetScale;
    }

    /// <summary>
    /// Play harvest effect
    /// </summary>
    private void PlayHarvestEffect()
    {
        if (harvestEffect != null)
        {
            Instantiate(harvestEffect, transform.position, Quaternion.identity);
        }

        if (harvestSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot2D(harvestSound, AudioManager.AudioCategory.SFX);
            }
            else
            {
                AudioSource.PlayClipAtPoint(harvestSound, transform.position);
            }
        }
    }

    /// <summary>
    /// Called when resource is fully depleted
    /// </summary>
    private void Deplete()
    {
        isDepleted = true;

        // Play depletion effect
        if (depleteEffect != null)
        {
            Instantiate(depleteEffect, transform.position, Quaternion.identity);
        }

        if (depleteSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot2D(depleteSound, AudioManager.AudioCategory.SFX);
            }
            else
            {
                AudioSource.PlayClipAtPoint(depleteSound, transform.position);
            }
        }

        Debug.Log($"{gameObject.name}: Depleted!");

        // Destroy after a short delay
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmos()
    {
        if (isDepleted)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = resourceType == ResourceType.Gold ? Color.yellow : Color.white;
        }

        Gizmos.DrawWireSphere(transform.position, 1f);

        // Draw harvest slots
        if (showSlotGizmos && slotPositions != null)
        {
            for (int i = 0; i < slotPositions.Length; i++)
            {
                bool isOccupied = harvestSlots != null && i < harvestSlots.Length && harvestSlots[i] != null;
                Gizmos.color = isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(slotPositions[i], 0.3f);
                Gizmos.DrawLine(transform.position, slotPositions[i]);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw resource amount indicator
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

#if UNITY_EDITOR
        string info = $"{resourceType}\n{currentAmount}/{resourceAmount}";

        if (harvestSlots != null)
        {
            int occupiedSlots = 0;
            for (int i = 0; i < harvestSlots.Length; i++)
            {
                if (harvestSlots[i] != null) occupiedSlots++;
            }
            info += $"\nSlots: {occupiedSlots}/{maxHarvesters}";
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, info);
#endif
    }
}

/// <summary>
/// Types of resources that can be collected
/// </summary>
public enum ResourceType
{
    Gold,
    Food,
    Wood,
    Stone
}
