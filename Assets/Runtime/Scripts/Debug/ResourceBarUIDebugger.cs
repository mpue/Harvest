using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Debug component to monitor ResourceBarUI directly
/// </summary>
[RequireComponent(typeof(ResourceBarUI))]
public class ResourceBarUIDebugger : MonoBehaviour
{
    private ResourceBarUI resourceBarUI;
    private ResourceManager resourceManager;
    
[Header("Debug Display")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private bool logUpdates = true;
    
    private int updateCount = 0;
    private string lastUpdate = "";
    private float lastUpdateTime;
    
    void Awake()
    {
        resourceBarUI = GetComponent<ResourceBarUI>();
    }
    
    void Start()
    {
        // Get ResourceManager from ResourceBarUI via reflection
        var field = typeof(ResourceBarUI).GetField("resourceManager", 
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
       resourceManager = field.GetValue(resourceBarUI) as ResourceManager;
 }
        
        if (resourceManager == null)
        {
            Debug.LogError("ResourceBarUIDebugger: Could not find ResourceManager in ResourceBarUI!");
        }
   else
        {
          Debug.Log($"? ResourceBarUIDebugger: Monitoring ResourceBarUI on '{gameObject.name}'");
     Debug.Log($"  ResourceManager: {resourceManager.gameObject.name}");

            // Subscribe to events to see if they arrive
            resourceManager.OnResourcesChanged += OnResourcesChangedDebug;
       resourceManager.OnEnergyChanged += OnEnergyChangedDebug;
        }
    }
    
    private void OnResourcesChangedDebug(int food, int wood, int stone, int gold)
  {
        updateCount++;
   lastUpdate = $"Resources: Gold={gold}";
        lastUpdateTime = Time.time;
        
        if (logUpdates)
        {
      Debug.Log($"?? ResourceBarUI received OnResourcesChanged: Gold={gold}");
        }
    }
    
    private void OnEnergyChangedDebug(int current, int max)
    {
 updateCount++;
        int available = max - current;
        lastUpdate = $"Energy: {current}/{max} (Available: {available})";
        lastUpdateTime = Time.time;
        
 if (logUpdates)
        {
            Debug.Log($"? ResourceBarUI received OnEnergyChanged: Current={current}, Max={max}, Available={available}");
     
      // Check if UI text components exist
            CheckUIComponents();
        }
    }
    
    private void CheckUIComponents()
    {
        // Use reflection to check UI components
        var goldTextField = typeof(ResourceBarUI).GetField("goldText", 
       System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var energyTextField = typeof(ResourceBarUI).GetField("energyText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
        if (goldTextField != null)
        {
 var goldTextComponent = goldTextField.GetValue(resourceBarUI);
     if (goldTextComponent == null)
            {
      Debug.LogWarning("   ? goldText is NULL!");
        }
            else
          {
      if (goldTextComponent is TextMeshProUGUI tmp)
         {
   Debug.Log($"   ? goldText (TMP): '{tmp.text}'");
    }
     else if (goldTextComponent is Text text)
     {
      Debug.Log($"   ? goldText (Legacy): '{text.text}'");
     }
            }
        }
        
        if (energyTextField != null)
        {
        var energyTextComponent = energyTextField.GetValue(resourceBarUI);
          if (energyTextComponent == null)
    {
                Debug.LogWarning("   ? energyText is NULL!");
       }
       else
{
       if (energyTextComponent is TextMeshProUGUI tmp)
    {
         Debug.Log($"   ? energyText (TMP): '{tmp.text}'");
    }
     else if (energyTextComponent is Text text)
              {
       Debug.Log($" ? energyText (Legacy): '{text.text}'");
     }
  }
        }
    }
    
    void OnGUI()
    {
     if (!showDebug) return;
        
    GUIStyle style = new GUIStyle(GUI.skin.box);
 style.fontSize = 11;
        style.alignment = TextAnchor.UpperLeft;
        
        string info = "=== ResourceBar UI Debug ===\n\n";
        info += $"Update Count: {updateCount}\n";
        
  if (Time.time - lastUpdateTime < 3f)
   {
info += $"\nLast Update ({(Time.time - lastUpdateTime):F1}s ago):\n";
            info += $"{lastUpdate}\n";
   }
        
if (resourceManager != null)
{
          info += $"\nDirect Manager Values:\n";
      info += $"  Gold: {resourceManager.Gold}\n";
   info += $"  Energy: {resourceManager.CurrentEnergy}/{resourceManager.MaxEnergy}\n";
            info += $"  Available: {resourceManager.AvailableEnergy}\n";
        }
        
    GUI.Box(new Rect(420, 450, 300, 180), info, style);
        
        // Force update button
        if (GUI.Button(new Rect(420, 640, 140, 30), "Force UI Update"))
{
  var method = typeof(ResourceBarUI).GetMethod("UpdateDisplay", 
   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
if (method != null)
 {
       method.Invoke(resourceBarUI, null);
 Debug.Log("?? Forced ResourceBarUI.UpdateDisplay()");
         }
        }
        
 if (GUI.Button(new Rect(570, 640, 150, 30), "Check UI Components"))
        {
  CheckUIComponents();
        }
    }
    
 void OnDestroy()
    {
 if (resourceManager != null)
        {
        resourceManager.OnResourcesChanged -= OnResourcesChangedDebug;
        resourceManager.OnEnergyChanged -= OnEnergyChangedDebug;
      }
    }
    
    [ContextMenu("Check UI Components Now")]
    void CheckUIComponentsNow()
    {
        CheckUIComponents();
    }
    
    [ContextMenu("Force Update Display")]
    void ForceUpdateDisplay()
    {
      var method = typeof(ResourceBarUI).GetMethod("UpdateDisplay", 
       System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
   method.Invoke(resourceBarUI, null);
            Debug.Log("?? Forced ResourceBarUI.UpdateDisplay()");
        }
    }
}
