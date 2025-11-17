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

    // Properties
    public ResourceType ResourceType => resourceType;
    public int CurrentAmount => currentAmount;
  public int ResourceAmount => resourceAmount;
    public bool IsDepleted => isDepleted;
    public float HarvestTime => harvestTime;
 public int AmountPerHarvest => amountPerHarvest;

    void Awake()
    {
        currentAmount = resourceAmount;
        if (visualModel != null)
        {
            originalScale = visualModel.transform.localScale;
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
    }

    void OnDrawGizmosSelected()
    {
     // Draw resource amount indicator
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

     #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
   $"{resourceType}\n{currentAmount}/{resourceAmount}");
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
