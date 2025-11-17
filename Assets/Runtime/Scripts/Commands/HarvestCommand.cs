using UnityEngine;

/// <summary>
/// Command handler for harvest orders
/// </summary>
public class HarvestCommand : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask collectableLayer;
    [SerializeField] private float maxHarvestDistance = 100f;

    private UnitSelector unitSelector;
    private Camera mainCamera;

    void Awake()
    {
  unitSelector = FindObjectOfType<UnitSelector>();
     mainCamera = Camera.main;
        if (mainCamera == null)
    {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    void Update()
    {
        // Right-click to harvest (if harvesters are selected)
        if (Input.GetMouseButtonDown(1))
 {
            if (unitSelector != null && unitSelector.SelectedUnits.Count > 0)
            {
            // Check if clicked on collectable
           Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;

                if (Physics.Raycast(ray, out hit, maxHarvestDistance, collectableLayer))
   {
         Collectable collectable = hit.collider.GetComponent<Collectable>();
             if (collectable != null && !collectable.IsDepleted)
             {
              // Command selected harvesters to gather
          CommandHarvest(collectable);
            }
             }
      }
        }

        // Keyboard shortcut: H for auto-harvest
   if (Input.GetKeyDown(KeyCode.H))
        {
     CommandAutoHarvest();
        }
    }

    /// <summary>
    /// Command selected harvesters to harvest specific collectable
/// </summary>
    private void CommandHarvest(Collectable collectable)
  {
        if (unitSelector == null) return;

        int harvesterCount = 0;
        foreach (var unit in unitSelector.SelectedUnits)
     {
            HarvesterUnit harvester = unit.GetComponent<HarvesterUnit>();
            if (harvester != null)
            {
      harvester.GatherFrom(collectable);
  harvesterCount++;
   }
        }

        if (harvesterCount > 0)
        {
Debug.Log($"Commanded {harvesterCount} harvester(s) to gather from {collectable.ResourceType}");
        }
        else
        {
 Debug.LogWarning("No harvesters selected!");
        }
    }

    /// <summary>
    /// Command selected harvesters to auto-harvest nearest resource
    /// </summary>
    private void CommandAutoHarvest()
    {
        if (unitSelector == null) return;

        int harvesterCount = 0;
    foreach (var unit in unitSelector.SelectedUnits)
        {
            HarvesterUnit harvester = unit.GetComponent<HarvesterUnit>();
            if (harvester != null)
  {
        harvester.AutoGather();
              harvesterCount++;
       }
        }

        if (harvesterCount > 0)
        {
       Debug.Log($"Commanded {harvesterCount} harvester(s) to auto-harvest");
        }
        else
        {
     Debug.LogWarning("No harvesters selected!");
        }
    }

    void OnGUI()
    {
        if (unitSelector != null && unitSelector.SelectedUnits.Count > 0)
        {
          // Check if any harvesters selected
   bool hasHarvesters = false;
  foreach (var unit in unitSelector.SelectedUnits)
     {
          if (unit.GetComponent<HarvesterUnit>() != null)
      {
                 hasHarvesters = true;
         break;
  }
            }

      if (hasHarvesters)
  {
         GUIStyle style = new GUIStyle(GUI.skin.box);
    style.fontSize = 14;
       style.alignment = TextAnchor.MiddleLeft;

      string instructions = "Harvester Commands:\n";
  instructions += "Right-Click on Resource: Harvest\n";
 instructions += "H: Auto-Harvest nearest resource";

        GUI.Box(new Rect(10, Screen.height - 100, 300, 80), instructions, style);
      }
        }
    }
}
