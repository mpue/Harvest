using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Debug script to diagnose unit selection issues
/// Attach to any GameObject in scene
/// </summary>
public class UnitSelectionDebug : MonoBehaviour
{
    [Header("Debug Settings")]
  [SerializeField] private bool enableDebugLogs = true;
[SerializeField] private KeyCode debugKey = KeyCode.F1;
    
    void Update()
    {
        if (!enableDebugLogs) return;
   
        // Toggle detailed debug with F1
        if (Input.GetKeyDown(debugKey))
        {
            ShowDetailedDebug();
 }
  
     // Show realtime debug on left click
        if (Input.GetMouseButtonDown(0))
        {
          DebugMouseClick();
        }
    }
  
    private void DebugMouseClick()
    {
        Debug.Log("=== MOUSE CLICK DEBUG ===");
    
    // Time scale
     Debug.Log($"Time.timeScale: {Time.timeScale}");
 
    // GameManager
        if (GameManager.Instance != null)
        {
     Debug.Log($"Game Ended: {GameManager.Instance.IsGameEnded()}");
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is NULL!");
        }
        
        // EventSystem
        if (EventSystem.current != null)
        {
    bool overUI = EventSystem.current.IsPointerOverGameObject();
      Debug.Log($"IsPointerOverGameObject: {overUI}");
            
         if (overUI && EventSystem.current.currentSelectedGameObject != null)
            {
            Debug.Log($"Current Selected: {EventSystem.current.currentSelectedGameObject.name}");
      }
        }
        else
        {
   Debug.LogError("EventSystem.current is NULL! Add EventSystem to scene!");
      }
        
        // Raycast to scene
     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
    {
        Debug.Log($"Raycast Hit: {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
            
BaseUnit unit = hit.collider.GetComponent<BaseUnit>();
      if (unit != null)
     {
     Debug.Log($"  ? Found BaseUnit: {unit.UnitName}");
            }
            else
    {
     Debug.Log($"  ? No BaseUnit component");
            }
        }
        else
        {
            Debug.Log("Raycast Hit: NOTHING");
        }
        
        // UnitSelector state
  UnitSelector selector = FindObjectOfType<UnitSelector>();
  if (selector != null)
        {
         Debug.Log($"Selected Units Count: {selector.SelectedCount}");
        }
   else
  {
        Debug.LogError("UnitSelector not found in scene!");
        }
        
        Debug.Log("========================");
    }
    
    private void ShowDetailedDebug()
    {
        Debug.Log("=== DETAILED SYSTEM DEBUG ===");
  
        // Canvas Groups
  CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>();
        Debug.Log($"Canvas Groups in scene: {canvasGroups.Length}");
        foreach (CanvasGroup cg in canvasGroups)
        {
       if (cg.gameObject.activeInHierarchy)
            {
     Debug.Log($"  - {cg.gameObject.name}: BlocksRaycasts={cg.blocksRaycasts}, Interactable={cg.interactable}, Alpha={cg.alpha}");
    }
        }
        
        // GameEndPanels
        GameEndPanel[] panels = FindObjectsOfType<GameEndPanel>(true); // Include inactive
        Debug.Log($"GameEndPanels in scene: {panels.Length}");
  foreach (GameEndPanel panel in panels)
        {
   Debug.Log($"  - {panel.gameObject.name}: Active={panel.gameObject.activeInHierarchy}");
        }
  
        // EventSystem
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Debug.Log($"EventSystems in scene: {eventSystems.Length}");
        if (eventSystems.Length == 0)
        {
            Debug.LogError("NO EventSystem found! Add one to the scene!");
        }
    else if (eventSystems.Length > 1)
        {
         Debug.LogWarning("Multiple EventSystems found! This can cause issues!");
        }
        
        Debug.Log("============================");
    }
    
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
    style.fontSize = 14;
        
        string info = $"F1: Detailed Debug\n";
        info += $"TimeScale: {Time.timeScale}\n";
        
        if (EventSystem.current != null)
        {
   info += $"Over UI: {EventSystem.current.IsPointerOverGameObject()}\n";
        }
   
        UnitSelector selector = FindObjectOfType<UnitSelector>();
 if (selector != null)
     {
            info += $"Selected: {selector.SelectedCount}";
 }
        
        GUI.Label(new Rect(10, Screen.height - 100, 300, 100), info, style);
    }
}
