using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI overlay for building placement feedback
/// Supports both TextMeshPro and legacy UI Text
/// </summary>
public class BuildingPlacementUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private GameObject placementPanel;

    [Header("Text Elements (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Text Elements (Legacy UI - Optional)")]
    [SerializeField] private Text buildingNameTextLegacy;
    [SerializeField] private Text statusTextLegacy;
    [SerializeField] private Text instructionsTextLegacy;

    [SerializeField] private Image statusIcon;

    [Header("Status Colors")]
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    [Header("Instructions")]
    [SerializeField] private string validInstructions = "Left Click to Place";
    [SerializeField] private string invalidInstructions = "Invalid Location!";
    [SerializeField] private string controlsInstructions = "Q/E: Rotate | Right Click/ESC: Cancel";

    private bool useTextMeshPro = true;

    void Awake()
    {
        // find all building placements

        BuildingPlacement[] placements = FindObjectsByType<BuildingPlacement>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (BuildingPlacement placement in placements)
        {

            if (!placement.name.Contains("AI"))
            {
                buildingPlacement = placement;
                break;
            }

        }

        // Determine which text system to use
        useTextMeshPro = buildingNameText != null || statusText != null || instructionsText != null;

        if (placementPanel != null)
        {
            placementPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (buildingPlacement == null) return;

        // Show/hide panel based on placement state
        bool isPlacing = buildingPlacement.IsPlacing;

        if (placementPanel != null && placementPanel.activeSelf != isPlacing)
        {
            placementPanel.SetActive(isPlacing);
        }

        // Update UI elements if placing
        if (isPlacing && buildingPlacement.CurrentProduct != null)
        {
            UpdatePlacementUI();
        }
    }

    private void UpdatePlacementUI()
    {
        Product product = buildingPlacement.CurrentProduct;

        // Update building name
        if (product != null)
        {
            SetText(buildingNameText, buildingNameTextLegacy, $"Placing: {product.ProductName}");
        }

        // Update status based on placement validity
        bool canPlace = buildingPlacement.CanPlace;

        Color statusColor = canPlace ? validColor : invalidColor;
        string statusMessage = canPlace ? "Ready to Place" : "Cannot Place Here";

        SetText(statusText, statusTextLegacy, statusMessage, statusColor);

        if (statusIcon != null)
        {
            statusIcon.color = statusColor;
        }

        string instructions = canPlace ? validInstructions : invalidInstructions;
        instructions += "\n" + controlsInstructions;
        SetText(instructionsText, instructionsTextLegacy, instructions);
    }

    /// <summary>
    /// Helper to set text on either TMP or legacy UI Text
    /// </summary>
    private void SetText(TextMeshProUGUI tmpText, Text legacyText, string text, Color? color = null)
    {
        if (useTextMeshPro && tmpText != null)
        {
            tmpText.text = text;
            if (color.HasValue)
            {
                tmpText.color = color.Value;
            }
        }
        else if (legacyText != null)
        {
            legacyText.text = text;
            if (color.HasValue)
            {
                legacyText.color = color.Value;
            }
        }
    }

    /// <summary>
    /// Show placement UI with custom message
    /// </summary>
    public void Show(string buildingName, bool isValid)
    {
        if (placementPanel != null)
        {
            placementPanel.SetActive(true);
        }

        SetText(buildingNameText, buildingNameTextLegacy, $"Placing: {buildingName}");
        UpdateStatus(isValid);
    }

    /// <summary>
    /// Hide placement UI
    /// </summary>
    public void Hide()
    {
        if (placementPanel != null)
        {
            placementPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Update status display
    /// </summary>
    public void UpdateStatus(bool isValid)
    {
        Color color = isValid ? validColor : invalidColor;
        string statusMessage = isValid ? "Ready to Place" : "Cannot Place Here";

        SetText(statusText, statusTextLegacy, statusMessage, color);

        if (statusIcon != null)
        {
            statusIcon.color = color;
        }

        string instructions = isValid ? validInstructions : invalidInstructions;
        instructions += "\n" + controlsInstructions;
        SetText(instructionsText, instructionsTextLegacy, instructions);
    }
}
