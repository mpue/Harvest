using UnityEngine;
using UnityEngine.UI;

namespace Harvest.Minimap
{
    /// <summary>
    /// Represents a unit icon on the minimap.
    /// Automatically syncs with TeamComponent for color updates.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MinimapIcon : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The unit this icon represents")]
        [SerializeField] private Transform targetUnit;
      
  [Tooltip("The TeamComponent of the target unit (optional, auto-detected)")]
        [SerializeField] private TeamComponent teamComponent;

        [Header("Icon Settings")]
        [Tooltip("Icon shape for this unit type")]
        [SerializeField] private IconShape iconShape = IconShape.Circle;
        
    [Tooltip("Size of the icon on the minimap")]
        [SerializeField] private float iconSize = 10f;
        
[Tooltip("Should the icon rotate with the unit?")]
        [SerializeField] private bool rotateWithUnit = true;
        
        [Tooltip("Icon visibility (can be controlled by fog of war)")]
        [SerializeField] private bool isVisible = true;

   [Header("Optional Overrides")]
        [Tooltip("Override team color (leave black to use team color)")]
 [SerializeField] private Color colorOverride = Color.black;
 
        [Tooltip("Icon sprite override (leave empty for default shape)")]
        [SerializeField] private Sprite spriteOverride;

        // Components
  private Image iconImage;
        private RectTransform rectTransform;
        private MinimapController minimapController;

        // Cached values
   private Color currentColor;
    private bool isInitialized = false;

        public enum IconShape
   {
    Circle,   // Default for most units
       Square,      // Buildings
            Triangle,    // Military units, vehicles
        Diamond,     // Special units, heroes
            Cross        // Objectives, markers
        }

        #region Unity Lifecycle

     private void Awake()
        {
      iconImage = GetComponent<Image>();
      rectTransform = GetComponent<RectTransform>();
        }

     private void Start()
        {
            Initialize();
    }

    private void LateUpdate()
        {
 if (!isInitialized || targetUnit == null)
 return;

UpdatePosition();

          if (rotateWithUnit)
{
    UpdateRotation();
            }

            // Check if team color changed
            if (teamComponent != null && teamComponent.TeamColor != currentColor)
            {
       UpdateColor();
            }
        }

        private void OnDestroy()
        {
      if (minimapController != null)
        {
    minimapController.UnregisterIcon(this);
  }
}

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the minimap icon
        /// </summary>
        public void Initialize()
        {
   if (isInitialized)
                return;

// Find MinimapController
       minimapController = FindObjectOfType<MinimapController>();
      if (minimapController == null)
       {
     Debug.LogWarning("MinimapIcon: No MinimapController found in scene!", this);
   return;
      }

     // Auto-detect TeamComponent if not set
            if (teamComponent == null && targetUnit != null)
     {
          teamComponent = targetUnit.GetComponent<TeamComponent>();
    }

         // Set up icon appearance
     SetupIconAppearance();

      // Initial position update
 UpdatePosition();
            UpdateColor();

 // Register with controller
       minimapController.RegisterIcon(this);

          isInitialized = true;
        }

        /// <summary>
        /// Set up the icon's visual appearance
      /// </summary>
  private void SetupIconAppearance()
  {
            // Set size
    rectTransform.sizeDelta = new Vector2(iconSize, iconSize);

      // Set sprite
      if (spriteOverride != null)
            {
      iconImage.sprite = spriteOverride;
            }
  else
        {
   iconImage.sprite = GetDefaultSpriteForShape(iconShape);
   }

       // Set initial visibility
 iconImage.enabled = isVisible;
        }

        /// <summary>
        /// Get default sprite for icon shape
     /// </summary>
    private Sprite GetDefaultSpriteForShape(IconShape shape)
        {
  // Unity's built-in sprites
          switch (shape)
            {
          case IconShape.Circle:
   return Resources.Load<Sprite>("UI/Skin/Knob.psd");
                case IconShape.Square:
         return Resources.Load<Sprite>("UI/Skin/UISprite.psd");
          case IconShape.Triangle:
  case IconShape.Diamond:
   case IconShape.Cross:
     default:
    // For now, use circle for all - can be extended with custom sprites
       return Resources.Load<Sprite>("UI/Skin/Knob.psd");
            }
        }

        #endregion

    #region Position and Rotation

        /// <summary>
      /// Update icon position based on unit position
        /// </summary>
      private void UpdatePosition()
        {
    if (targetUnit == null || minimapController == null)
    {
                SetVisible(false);
       return;
 }

            Vector2 minimapPos = minimapController.WorldToMinimapPoint(targetUnit.position);
 rectTransform.anchoredPosition = minimapPos;

            // Check if position is within minimap bounds
            bool inBounds = minimapController.IsPositionInBounds(targetUnit.position);
 SetVisible(isVisible && inBounds);
        }

   /// <summary>
   /// Update icon rotation based on unit rotation
        /// </summary>
        private void UpdateRotation()
 {
    if (targetUnit == null)
    return;

       // Use only Y rotation (top-down view)
            float yRotation = targetUnit.eulerAngles.y;
       rectTransform.localRotation = Quaternion.Euler(0, 0, -yRotation);
        }

      #endregion

        #region Color Management

        /// <summary>
        /// Update icon color from team component
        /// </summary>
        public void UpdateColor()
        {
   if (colorOverride != Color.black)
      {
  currentColor = colorOverride;
     }
            else if (teamComponent != null)
   {
       currentColor = teamComponent.TeamColor;
            }
            else
 {
          currentColor = Color.white;
}

  iconImage.color = currentColor;
   }

    /// <summary>
        /// Set icon color manually
        /// </summary>
        public void SetColor(Color color)
        {
            colorOverride = color;
       UpdateColor();
      }

   #endregion

   #region Visibility

   /// <summary>
        /// Set icon visibility
  /// </summary>
        public void SetVisible(bool visible)
        {
            if (iconImage != null)
            {
    iconImage.enabled = visible;
  }
        }

    /// <summary>
     /// Toggle icon visibility
        /// </summary>
  public void ToggleVisibility()
      {
          isVisible = !isVisible;
            SetVisible(isVisible);
        }

        #endregion

  #region Public API

      /// <summary>
        /// Set the target unit for this icon
        /// </summary>
   public void SetTargetUnit(Transform unit)
        {
          targetUnit = unit;
 
       if (unit != null)
            {
                teamComponent = unit.GetComponent<TeamComponent>();
          UpdateColor();
       }
        }

        /// <summary>
        /// Set the icon shape
        /// </summary>
        public void SetIconShape(IconShape shape)
        {
       iconShape = shape;
            SetupIconAppearance();
        }

      /// <summary>
        /// Set the icon size
        /// </summary>
        public void SetIconSize(float size)
        {
            iconSize = size;
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        }

    /// <summary>
        /// Get the target unit
        /// </summary>
    public Transform GetTargetUnit()
     {
return targetUnit;
      }

    /// <summary>
 /// Get the team component
        /// </summary>
        public TeamComponent GetTeamComponent()
        {
      return teamComponent;
        }

  #endregion

        #region Editor Helper

#if UNITY_EDITOR
    private void OnValidate()
        {
    if (iconImage != null && rectTransform != null)
       {
    rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        UpdateColor();
     }
  }
#endif

        #endregion
    }
}
