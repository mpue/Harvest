using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Zone where harvesters can deploy/drop off collected resources
/// </summary>
public class DeployZone : MonoBehaviour
{
    [Header("Zone Settings")]
  [SerializeField] private float deployRadius = 5f;
    [SerializeField] private Transform deployPoint;
    [SerializeField] private Vector3 deployPointOffset = new Vector3(0, 0, 3f); // NEW: Configurable offset!
    [SerializeField] private bool showGizmos = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject deployEffectPrefab;
    [SerializeField] private AudioClip deploySound;
    [SerializeField] private float deploySoundVolume = 1f;
    
    [Header("Resource Settings")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private float deployDuration = 1f;
    
    [Header("Queue Settings")]
    [SerializeField] private int maxQueueSize = 5;
    [SerializeField] private List<Vector3> queuePositions = new List<Vector3>();
    
    private List<HarvesterUnit> currentlyDeploying = new List<HarvesterUnit>();
    private TeamComponent teamComponent;
    
    // Properties
    public Vector3 DeployPoint => deployPoint != null ? deployPoint.position : transform.position;
    public float DeployRadius => deployRadius;
    public bool HasSpace => currentlyDeploying.Count < maxQueueSize;
    public ResourceManager ResourceManager => resourceManager;
    
    void Awake()
    {
     teamComponent = GetComponent<TeamComponent>();
        
        // Auto-find resource manager if not set
        if (resourceManager == null)
  {
 if (teamComponent != null)
            {
        // Find resource manager for this team
                ResourceManager[] allManagers = FindObjectsOfType<ResourceManager>();
      foreach (var manager in allManagers)
     {
         if (teamComponent.CurrentTeam != Team.Player && manager.gameObject.name.Contains("AI"))
         {
                 resourceManager = manager;
      Debug.Log($"DeployZone: Found AI ResourceManager");
   break;
 }
      else if (teamComponent.CurrentTeam == Team.Player && !manager.gameObject.name.Contains("AI"))
     {
          resourceManager = manager;
           Debug.Log($"DeployZone: Found Player ResourceManager");
   break;
              }
     }
            }
         
      if (resourceManager == null)
        {
    resourceManager = FindObjectOfType<ResourceManager>();
     Debug.LogWarning($"DeployZone: Using fallback ResourceManager");
            }
        }
 
        // Create default deploy point if not set
        if (deployPoint == null)
        {
       GameObject deployPointObj = new GameObject("DeployPoint");
            deployPointObj.transform.SetParent(transform);
            deployPointObj.transform.localPosition = deployPointOffset; // Use offset!
  deployPoint = deployPointObj.transform;
      }
        else
        {
      // Apply offset to existing deploy point
            deployPoint.localPosition = deployPointOffset;
        }
        
        // Generate queue positions if empty
        if (queuePositions.Count == 0)
        {
        GenerateQueuePositions();
        }
    }
    
    void Update()
    {
   // Clean up null references
        currentlyDeploying.RemoveAll(h => h == null);
    }
    
    /// <summary>
    /// Check if harvester can deploy here
    /// </summary>
    public bool CanDeploy(HarvesterUnit harvester)
    {
 if (harvester == null) return false;
        
        // Check if harvester has resources
  if (!harvester.HasResources)
        {
 return false;
        }
        
        // Check team
    if (teamComponent != null)
        {
            TeamComponent harvesterTeam = harvester.GetComponent<TeamComponent>();
   if (harvesterTeam != null && harvesterTeam.CurrentTeam != teamComponent.CurrentTeam)
      {
        return false; // Wrong team!
            }
        }
      
        // Check if zone has space
        if (!HasSpace)
        {
            Debug.Log($"DeployZone: Full! ({currentlyDeploying.Count}/{maxQueueSize})");
   return false;
        }
  
        return true;
    }
    
    /// <summary>
    /// Get next available queue position
    /// </summary>
  public Vector3 GetQueuePosition(HarvesterUnit harvester)
    {
     int queueIndex = currentlyDeploying.Count;
        
        if (queueIndex < queuePositions.Count)
 {
     return transform.TransformPoint(queuePositions[queueIndex]);
        }
        
        // Fallback: circle around deploy point
        float angle = queueIndex * (360f / maxQueueSize);
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * deployRadius;
  return DeployPoint + offset;
    }
    
    /// <summary>
    /// Start deploying resources from harvester
    /// </summary>
    public void StartDeploy(HarvesterUnit harvester)
    {
     if (!CanDeploy(harvester))
        {
            Debug.LogWarning($"DeployZone: Cannot deploy from {harvester.name}");
       return;
        }
      
  if (!currentlyDeploying.Contains(harvester))
        {
   currentlyDeploying.Add(harvester);
            Debug.Log($"DeployZone: Started deploy from {harvester.name} (Queue: {currentlyDeploying.Count}/{maxQueueSize})");
        }
        
        // Start deploy coroutine
    StartCoroutine(DeployCoroutine(harvester));
    }
    
    /// <summary>
    /// Deploy resources coroutine
    /// </summary>
    private System.Collections.IEnumerator DeployCoroutine(HarvesterUnit harvester)
    {
  if (harvester == null) yield break;
     
      // Wait for deploy duration
        float elapsed = 0f;
        while (elapsed < deployDuration)
        {
            elapsed += Time.deltaTime;
      yield return null;
        }
        
        // Deploy resources
        if (harvester != null && resourceManager != null)
  {
 int amount = harvester.GetCarriedAmount();
   ResourceType resourceType = harvester.GetCarriedResourceType();
        
      if (amount > 0)
            {
 // Add resources to manager
    switch (resourceType)
     {
         case ResourceType.Food:
              resourceManager.AddResources(amount, 0, 0, 0);
    break;
   case ResourceType.Wood:
    resourceManager.AddResources(0, amount, 0, 0);
         break;
      case ResourceType.Stone:
            resourceManager.AddResources(0, 0, amount, 0);
           break;
       case ResourceType.Gold:
    resourceManager.AddResources(0, 0, 0, amount);
               break;
  }
         
        Debug.Log($"DeployZone: Harvester deployed {amount} {resourceType}");
     
                // Clear harvester resources
     harvester.ClearResources();
          
// Play effects
   PlayDeployEffects();
         }
        }
        
// Remove from queue
        currentlyDeploying.Remove(harvester);
    }
    
    /// <summary>
  /// Play deploy visual and audio effects
    /// </summary>
    private void PlayDeployEffects()
    {
        // Spawn effect
        if (deployEffectPrefab != null)
        {
      GameObject effect = Instantiate(deployEffectPrefab, DeployPoint, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
 // Play sound
        if (deploySound != null)
   {
   AudioSource.PlayClipAtPoint(deploySound, DeployPoint, deploySoundVolume);
   }
    }
    
    /// <summary>
    /// Generate default queue positions
    /// </summary>
    private void GenerateQueuePositions()
    {
        queuePositions.Clear();
        
        for (int i = 0; i < maxQueueSize; i++)
    {
      float angle = i * (360f / maxQueueSize);
   Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * deployRadius * 0.7f;
   queuePositions.Add(offset);
        }
    }
    
    /// <summary>
    /// Check if position is in deploy range
    /// </summary>
    public bool IsInRange(Vector3 position)
    {
        return Vector3.Distance(position, DeployPoint) <= deployRadius;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
     Vector3 center = deployPoint != null ? deployPoint.position : transform.position;
        
        // Draw deploy radius
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        DrawWireDisc(center, deployRadius);
        
        // Draw deploy point
  Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.5f);
        
        // Draw queue positions
        Gizmos.color = Color.yellow;
     if (Application.isPlaying)
        {
       for (int i = 0; i < queuePositions.Count; i++)
            {
          Vector3 queuePos = transform.TransformPoint(queuePositions[i]);
             Gizmos.DrawWireSphere(queuePos, 0.3f);
              
         // Draw line to center
                Gizmos.DrawLine(queuePos, center);
      }
        }
        else
     {
        // Preview in editor
    for (int i = 0; i < maxQueueSize; i++)
            {
        float angle = i * (360f / maxQueueSize);
Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * deployRadius * 0.7f;
           Vector3 queuePos = center + offset;
            Gizmos.DrawWireSphere(queuePos, 0.3f);
            }
        }
    }
    
    void OnDrawGizmosSelected()
{
        if (!showGizmos) return;
 
        Vector3 center = deployPoint != null ? deployPoint.position : transform.position;
        
        // Draw larger radius when selected
 Gizmos.color = Color.green;
        DrawWireDisc(center, deployRadius);
        
    // Draw direction indicator
Gizmos.color = Color.blue;
        Gizmos.DrawRay(center, transform.forward * deployRadius);
    }
    
  /// <summary>
    /// Helper to draw a wire disc
    /// </summary>
    private void DrawWireDisc(Vector3 center, float radius)
    {
     int segments = 32;
        float angleStep = 360f / segments;
        
 Vector3 prevPoint = center + Vector3.forward * radius;
        for (int i = 1; i <= segments; i++)
        {
   float angle = i * angleStep * Mathf.Deg2Rad;
         Vector3 newPoint = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
 }
}
