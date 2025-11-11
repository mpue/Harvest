using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper to automatically setup health bars for units
/// Add this to a unit to auto-create a healthbar
/// </summary>
[RequireComponent(typeof(Health))]
public class HealthBarSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private GameObject healthBarPrefab;

    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector2 healthBarSize = new Vector2(1f, 0.1f);
    [SerializeField] private bool hideWhenFull = true;

    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    private GameObject healthBarInstance;

    void Start()
    {
        if (autoSetup)
        {
            SetupHealthBar();
        }
    }

    /// <summary>
    /// Create and setup the health bar
    /// </summary>
    public void SetupHealthBar()
    {
        Health health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError($"HealthBarSetup on {gameObject.name} requires a Health component!");
            return;
        }

        // Use prefab if available, otherwise create manually
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            healthBarInstance.transform.localPosition = healthBarOffset;
        }
        else
        {
            healthBarInstance = CreateHealthBarManually();
        }
    }

    /// <summary>
    /// Manually create a health bar from scratch
    /// </summary>
    private GameObject CreateHealthBarManually()
    {
        // Create root object
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = healthBarOffset;

        // Add Canvas
        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Add CanvasScaler
        CanvasScaler scaler = healthBarObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Set canvas size
        RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize * 100f; // Scale up for better resolution

        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(healthBarObj.transform);
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullHealthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // HealthBar component will be added separately if needed

        return healthBarObj;
    }

    /// <summary>
    /// Get the health bar instance
    /// </summary>
    public GameObject GetHealthBar()
    {
        return healthBarInstance;
    }
}
