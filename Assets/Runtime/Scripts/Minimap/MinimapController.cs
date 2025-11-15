using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Harvest.Minimap
{
    /// <summary>
    /// Main controller for the minimap system.
    /// Manages minimap camera, icons, and user interaction.
    /// </summary>
    public class MinimapController : MonoBehaviour, IPointerClickHandler
    {
     [Header("Minimap Camera")]
        [Tooltip("Camera used for minimap rendering")]
        [SerializeField] private Camera minimapCamera;

   [Tooltip("Height of the minimap camera above the terrain")]
     [SerializeField] private float cameraHeight = 100f;
        
        [Tooltip("Orthographic size of the minimap camera")]
        [SerializeField] private float cameraSize = 50f;

        [Header("Minimap UI")]
        [Tooltip("RenderTexture for the minimap camera output")]
        [SerializeField] private RenderTexture minimapRenderTexture;
        
  [Tooltip("UI RawImage displaying the minimap")]
        [SerializeField] private RawImage minimapImage;
        
        [Tooltip("RectTransform of the minimap container")]
     [SerializeField] private RectTransform minimapContainer;
        
        [Tooltip("Canvas containing minimap icons")]
    [SerializeField] private RectTransform iconContainer;

[Header("World Bounds")]
[Tooltip("Center of the world area shown on minimap")]
        [SerializeField] private Vector3 worldCenter = Vector3.zero;
      
     [Tooltip("Size of the world area (X and Z dimensions)")]
        [SerializeField] private Vector2 worldSize = new Vector2(200f, 200f);

        [Header("Icon Management")]
        [Tooltip("Prefab for minimap icons")]
        [SerializeField] private GameObject iconPrefab;
        
        [Tooltip("Auto-create icons for units with TeamComponent")]
        [SerializeField] private bool autoCreateIcons = true;
        
        [Tooltip("Update icons every N frames (0 = every frame)")]
        [SerializeField] private int updateInterval = 1;

        [Header("Interaction")]
    [Tooltip("Enable click-to-move-camera on minimap")]
        [SerializeField] private bool enableClickNavigation = true;
        
   [Tooltip("Main camera that will be moved when clicking minimap")]
        [SerializeField] private Camera mainCamera;
        
        [Tooltip("Height offset for camera position when navigating")]
    [SerializeField] private float cameraHeightOffset = 20f;

        [Header("Camera View Indicator")]
    [Tooltip("Show main camera view area on minimap")]
    [SerializeField] private bool showCameraViewIndicator = true;
  
        [Tooltip("RectTransform for camera view indicator")]
        [SerializeField] private RectTransform cameraViewIndicator;
        
        [Tooltip("Color of the camera view indicator")]
        [SerializeField] private Color viewIndicatorColor = new Color(1f, 1f, 1f, 0.3f);

      // Icon tracking
        private List<MinimapIcon> registeredIcons = new List<MinimapIcon>();
        private Dictionary<Transform, MinimapIcon> unitToIconMap = new Dictionary<Transform, MinimapIcon>();
        
  // Update management
        private int frameCounter = 0;
        
        // Cached values
        private Rect worldBounds;
      private Vector2 minimapSize;

#region Unity Lifecycle

        private void Awake()
        {
  SetupMinimapCamera();
          CalculateWorldBounds();
     }

 private void Start()
  {
      if (autoCreateIcons)
            {
 AutoCreateIconsForUnits();
   }

      if (mainCamera == null)
            {
          mainCamera = Camera.main;
  }

            SetupCameraViewIndicator();
        }

        private void LateUpdate()
        {
            frameCounter++;
   
            if (updateInterval > 0 && frameCounter % updateInterval != 0)
  return;

         UpdateCameraViewIndicator();
 }

        #endregion

        #region Setup

        /// <summary>
        /// Setup the minimap camera
        /// </summary>
        private void SetupMinimapCamera()
        {
      if (minimapCamera == null)
            {
                // Create minimap camera if it doesn't exist
      GameObject camObj = new GameObject("MinimapCamera");
     camObj.transform.SetParent(transform);
    minimapCamera = camObj.AddComponent<Camera>();
            }

            // Configure camera
 minimapCamera.orthographic = true;
    minimapCamera.orthographicSize = cameraSize;
       minimapCamera.transform.position = worldCenter + Vector3.up * cameraHeight;
      minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    minimapCamera.cullingMask = LayerMask.GetMask("Minimap"); // Only render minimap layer
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = Color.black;

 // Setup render texture if needed
         if (minimapRenderTexture == null && minimapImage != null)
        {
     minimapRenderTexture = new RenderTexture(512, 512, 16);
      minimapRenderTexture.name = "MinimapRT";
          minimapImage.texture = minimapRenderTexture;
            }

            if (minimapRenderTexture != null)
            {
    minimapCamera.targetTexture = minimapRenderTexture;
    }
    }

        /// <summary>
        /// Setup camera view indicator
        /// </summary>
        private void SetupCameraViewIndicator()
        {
            if (!showCameraViewIndicator || cameraViewIndicator == null)
      return;

   Image indicatorImage = cameraViewIndicator.GetComponent<Image>();
   if (indicatorImage != null)
 {
      indicatorImage.color = viewIndicatorColor;
          }
        }

   /// <summary>
     /// Calculate world bounds for coordinate conversion
  /// </summary>
        private void CalculateWorldBounds()
        {
     worldBounds = new Rect(
worldCenter.x - worldSize.x / 2f,
    worldCenter.z - worldSize.y / 2f,
                worldSize.x,
     worldSize.y
         );

            if (minimapContainer != null)
       {
          minimapSize = minimapContainer.rect.size;
            }
 }

        #endregion

      #region Icon Management

        /// <summary>
        /// Auto-create minimap icons for all units with TeamComponent
     /// </summary>
        public void AutoCreateIconsForUnits()
    {
          TeamComponent[] allTeamComponents = FindObjectsOfType<TeamComponent>();
            
        foreach (TeamComponent teamComp in allTeamComponents)
    {
    if (!unitToIconMap.ContainsKey(teamComp.transform))
  {
                  CreateIconForUnit(teamComp.transform);
    }
}

   Debug.Log($"MinimapController: Created {unitToIconMap.Count} minimap icons");
}

        /// <summary>
        /// Create a minimap icon for a specific unit
        /// </summary>
        public MinimapIcon CreateIconForUnit(Transform unit, MinimapIcon.IconShape shape = MinimapIcon.IconShape.Circle)
      {
   if (unit == null || iconContainer == null)
                return null;

            // Check if icon already exists
   if (unitToIconMap.ContainsKey(unit))
          {
        return unitToIconMap[unit];
        }

// Create icon
       GameObject iconObj;
    if (iconPrefab != null)
 {
        iconObj = Instantiate(iconPrefab, iconContainer);
         }
            else
    {
iconObj = new GameObject($"Icon_{unit.name}");
  iconObj.transform.SetParent(iconContainer);
  iconObj.AddComponent<Image>();
      }

     // Setup MinimapIcon component
            MinimapIcon icon = iconObj.GetComponent<MinimapIcon>();
            if (icon == null)
       {
     icon = iconObj.AddComponent<MinimapIcon>();
            }

            icon.SetTargetUnit(unit);
   icon.SetIconShape(shape);
          icon.Initialize();

       // Register icon
   unitToIconMap[unit] = icon;
            
            return icon;
        }

 /// <summary>
      /// Register an icon with the controller
        /// </summary>
        public void RegisterIcon(MinimapIcon icon)
        {
         if (!registeredIcons.Contains(icon))
            {
 registeredIcons.Add(icon);
            }
        }

    /// <summary>
        /// Unregister an icon from the controller
        /// </summary>
        public void UnregisterIcon(MinimapIcon icon)
        {
        registeredIcons.Remove(icon);
            
            Transform unit = icon.GetTargetUnit();
         if (unit != null && unitToIconMap.ContainsKey(unit))
          {
       unitToIconMap.Remove(unit);
       }
        }

    /// <summary>
 /// Remove icon for a specific unit
        /// </summary>
        public void RemoveIconForUnit(Transform unit)
        {
            if (unitToIconMap.TryGetValue(unit, out MinimapIcon icon))
     {
    if (icon != null)
             {
             Destroy(icon.gameObject);
       }
         unitToIconMap.Remove(unit);
  }
        }

        /// <summary>
     /// Clear all icons
        /// </summary>
      public void ClearAllIcons()
        {
     foreach (MinimapIcon icon in registeredIcons)
  {
                if (icon != null)
      {
        Destroy(icon.gameObject);
  }
       }

       registeredIcons.Clear();
            unitToIconMap.Clear();
        }

 #endregion

        #region Coordinate Conversion

        /// <summary>
        /// Convert world position to minimap position
/// </summary>
        public Vector2 WorldToMinimapPoint(Vector3 worldPos)
{
          if (minimapContainer == null)
  return Vector2.zero;

          // Normalize world position to [0,1] range
            float normalizedX = Mathf.InverseLerp(worldBounds.xMin, worldBounds.xMax, worldPos.x);
            float normalizedY = Mathf.InverseLerp(worldBounds.yMin, worldBounds.yMax, worldPos.z);

       // Convert to minimap coordinates (centered at 0,0)
  float minimapX = (normalizedX - 0.5f) * minimapSize.x;
            float minimapY = (normalizedY - 0.5f) * minimapSize.y;

            return new Vector2(minimapX, minimapY);
      }

        /// <summary>
        /// Convert minimap position to world position
        /// </summary>
        public Vector3 MinimapToWorldPoint(Vector2 minimapPos)
  {
         if (minimapContainer == null)
     return worldCenter;

      // Convert from minimap coordinates to normalized [0,1]
            float normalizedX = (minimapPos.x / minimapSize.x) + 0.5f;
            float normalizedY = (minimapPos.y / minimapSize.y) + 0.5f;

            // Convert to world coordinates
            float worldX = Mathf.Lerp(worldBounds.xMin, worldBounds.xMax, normalizedX);
            float worldZ = Mathf.Lerp(worldBounds.yMin, worldBounds.yMax, normalizedY);

         return new Vector3(worldX, 0f, worldZ);
      }

        /// <summary>
        /// Check if world position is within minimap bounds
        /// </summary>
        public bool IsPositionInBounds(Vector3 worldPos)
    {
     return worldBounds.Contains(new Vector2(worldPos.x, worldPos.z));
        }

        #endregion

        #region Camera View Indicator

        /// <summary>
      /// Update the camera view indicator position and size
        /// </summary>
   private void UpdateCameraViewIndicator()
  {
  if (!showCameraViewIndicator || cameraViewIndicator == null || mainCamera == null)
     return;

            // Get camera frustum corners in world space
        Vector3 cameraPos = mainCamera.transform.position;
 
        // Simple approximation: use camera position
            Vector2 minimapPos = WorldToMinimapPoint(cameraPos);
        cameraViewIndicator.anchoredPosition = minimapPos;

     // Calculate indicator size based on camera view
       if (mainCamera.orthographic)
            {
             float cameraWorldSize = mainCamera.orthographicSize * 2f;
  float indicatorSize = (cameraWorldSize / worldSize.x) * minimapSize.x;
                cameraViewIndicator.sizeDelta = new Vector2(indicatorSize, indicatorSize);
            }
        }

   #endregion

     #region Interaction

        /// <summary>
        /// Handle pointer click on minimap
        /// </summary>
      public void OnPointerClick(PointerEventData eventData)
        {
            if (!enableClickNavigation || mainCamera == null)
       return;

            // Convert screen point to local point in minimap
RectTransformUtility.ScreenPointToLocalPointInRectangle(
      minimapContainer,
        eventData.position,
       eventData.pressEventCamera,
      out Vector2 localPoint
    );

  // Convert to world position
  Vector3 worldPos = MinimapToWorldPoint(localPoint);
            worldPos.y = cameraHeightOffset;

            // Move main camera
            MoveCameraToPosition(worldPos);
        }

        /// <summary>
  /// Move main camera to world position
 /// </summary>
        public void MoveCameraToPosition(Vector3 worldPos)
    {
   if (mainCamera == null)
     return;

         Vector3 targetPos = worldPos;
        targetPos.y = mainCamera.transform.position.y; // Keep camera height

    mainCamera.transform.position = targetPos;
     }

    #endregion

        #region Public API

        /// <summary>
   /// Set world bounds for the minimap
        /// </summary>
  public void SetWorldBounds(Vector3 center, Vector2 size)
        {
        worldCenter = center;
            worldSize = size;
            CalculateWorldBounds();

      // Update camera
  if (minimapCamera != null)
   {
             minimapCamera.transform.position = worldCenter + Vector3.up * cameraHeight;
        minimapCamera.orthographicSize = Mathf.Max(size.x, size.y) / 2f;
  }
        }

      /// <summary>
        /// Get icon for specific unit
      /// </summary>
      public MinimapIcon GetIconForUnit(Transform unit)
    {
   unitToIconMap.TryGetValue(unit, out MinimapIcon icon);
      return icon;
        }

        /// <summary>
        /// Refresh all icons (useful after major changes)
   /// </summary>
  public void RefreshAllIcons()
        {
            foreach (MinimapIcon icon in registeredIcons)
   {
  if (icon != null)
     {
    icon.UpdateColor();
                }
            }
      }

  #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnValidate()
        {
     CalculateWorldBounds();
  
            if (Application.isPlaying && minimapCamera != null)
   {
       minimapCamera.orthographicSize = cameraSize;
          minimapCamera.transform.position = worldCenter + Vector3.up * cameraHeight;
            }
}

 private void OnDrawGizmosSelected()
        {
       // Draw world bounds
       Gizmos.color = Color.cyan;
            Vector3 center = worldCenter;
       center.y = 0f;
            Gizmos.DrawWireCube(center, new Vector3(worldSize.x, 0.1f, worldSize.y));

            // Draw minimap camera position
 if (minimapCamera != null)
            {
    Gizmos.color = Color.yellow;
       Gizmos.DrawWireSphere(minimapCamera.transform.position, 2f);
          }
        }
#endif

     #endregion
  }
}
