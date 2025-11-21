using UnityEngine;

namespace Harvest.Minimap
{
    /// <summary>
    /// Auto-setup component for units that should appear on the minimap.
    /// Add this to any unit and it will automatically create a minimap icon.
    /// </summary>
    [RequireComponent(typeof(TeamComponent))]
    public class MinimapUnit : MonoBehaviour
    {
        [Header("Icon Settings")]
        [Tooltip("Shape of the icon on the minimap")]
        [SerializeField] private MinimapIcon.IconShape iconShape = MinimapIcon.IconShape.Circle;

        [Tooltip("Size of the icon (default: 10)")]
        [SerializeField] private float iconSize = 10f;

        [Tooltip("Should icon rotate with unit?")]
        [SerializeField] private bool rotateWithUnit = false;

        [Tooltip("Auto-create icon on Start")]
        [SerializeField] private bool autoCreate = true;

        [Header("Runtime Info")]
        [SerializeField] private MinimapIcon minimapIcon;

        private MinimapController minimapController;
        private TeamComponent teamComponent;

        private void Awake()
        {
            teamComponent = GetComponent<TeamComponent>();
        }

        private void Start()
        {
            if (autoCreate)
            {
                CreateIcon();
            }
        }

        private void OnDestroy()
        {
            RemoveIcon();
        }

        /// <summary>
        /// Create the minimap icon for this unit
        /// </summary>
        public void CreateIcon()
        {
            if (minimapIcon != null)
                return; // Already created

            // Find minimap controller
            if (minimapController == null)
            {
                minimapController = FindFirstObjectByType<MinimapController>();
            }

            if (minimapController == null)
            {
                Debug.LogWarning("MinimapUnit: No MinimapController found in scene!", this);
                return;
            }

            // Create icon through controller
            minimapIcon = minimapController.CreateIconForUnit(transform, iconShape);

            if (minimapIcon != null)
            {
                minimapIcon.SetIconSize(iconSize);
            }
        }

        /// <summary>
        /// Remove the minimap icon
        /// </summary>
        public void RemoveIcon()
        {
            if (minimapIcon != null)
            {
                if (minimapController != null)
                {
                    minimapController.RemoveIconForUnit(transform);
                }
                else
                {
                    Destroy(minimapIcon.gameObject);
                }

                minimapIcon = null;
            }
        }

        /// <summary>
        /// Update icon appearance
        /// </summary>
        public void UpdateIcon()
        {
            if (minimapIcon != null)
            {
                minimapIcon.UpdateColor();
            }
        }

        /// <summary>
        /// Set icon shape
        /// </summary>
        public void SetIconShape(MinimapIcon.IconShape shape)
        {
            iconShape = shape;
            if (minimapIcon != null)
            {
                minimapIcon.SetIconShape(shape);
            }
        }

        /// <summary>
        /// Set icon size
        /// </summary>
        public void SetIconSize(float size)
        {
            iconSize = size;
            if (minimapIcon != null)
            {
                minimapIcon.SetIconSize(size);
            }
        }

        /// <summary>
        /// Get the minimap icon
        /// </summary>
        public MinimapIcon GetIcon()
        {
            return minimapIcon;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && minimapIcon != null)
            {
                minimapIcon.SetIconSize(iconSize);
                minimapIcon.SetIconShape(iconShape);
            }
        }
#endif
    }
}
