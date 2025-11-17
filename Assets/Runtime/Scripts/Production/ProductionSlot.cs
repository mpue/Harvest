using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI element representing a single product slot in the production panel
/// </summary>
public class ProductionSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private Button productButton;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject progressBar;

    [Header("Enhanced Progress Display")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressGlow;
    [SerializeField] private Image overlayDimmer;
    [SerializeField] private GameObject productionIndicator;

    [Header("Progress Animation")]
    [SerializeField] private bool animateProgress = true;
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private Color progressColorStart = Color.yellow;
    [SerializeField] private Color progressColorEnd = Color.green;

    private Product product;
    private System.Action<Product> onProductSelected;
    private float currentProgress = 0f;
    private bool isProducing = false;

    void Awake()
    {
        if (productButton != null)
        {
            productButton.onClick.AddListener(OnButtonClicked);
        }

        if (progressBar != null)
        {
            progressBar.SetActive(false);
        }

        if (overlayDimmer != null)
        {
            overlayDimmer.gameObject.SetActive(false);
        }

        if (productionIndicator != null)
        {
            productionIndicator.SetActive(false);
        }

        if (progressGlow != null)
        {
            progressGlow.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isProducing && animateProgress)
        {
            AnimateProgressEffects();
        }
    }

    /// <summary>
    /// Initialize the slot with product data
    /// </summary>
    public void Initialize(Product product, System.Action<Product> onProductSelected)
    {
        this.product = product;
        this.onProductSelected = onProductSelected;

        UpdateUI();
    }

    /// <summary>
    /// Update the UI elements with product data
    /// </summary>
    private void UpdateUI()
    {
        if (product == null) return;

        // Set product image
        if (productImage != null)
        {
            productImage.sprite = product.PreviewImage;
            productImage.enabled = product.PreviewImage != null;
        }

        // Set product name
        if (productNameText != null)
        {
            productNameText.text = product.ProductName;
        }

        // Set cost text
        if (costText != null)
        {
            costText.text = product.GetCostString();
        }

        // Set duration text
        if (durationText != null)
        {
            durationText.text = $"{product.ProductionDuration:F0}s";
        }
    }

    /// <summary>
    /// Update the progress bar (for queue items)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        isProducing = currentProgress > 0f && currentProgress < 1f;

        // Show/hide progress bar
        if (progressBar != null)
        {
            bool shouldShow = currentProgress > 0f;
            progressBar.SetActive(shouldShow);

            // Debug logging
            if (shouldShow && !progressBar.activeSelf)
            {
                Debug.LogWarning($"ProgressBar should be visible but activeSelf is false. Progress: {currentProgress}");
            }
        }
        else
        {
            if (currentProgress > 0f)
            {
                Debug.LogWarning("ProgressBar is null but progress > 0!");
            }
        }

        // Update fill amount
        if (progressFill != null)
        {
            progressFill.fillAmount = currentProgress;

            // Lerp color based on progress
            progressFill.color = Color.Lerp(progressColorStart, progressColorEnd, currentProgress);
        }
        else
        {
            if (currentProgress > 0f)
            {
                Debug.LogWarning("ProgressFill is null but progress > 0!");
            }
        }

        // Update progress text
        if (progressText != null)
        {
            if (currentProgress > 0f)
            {
                progressText.gameObject.SetActive(true);
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";

                // Calculate remaining time
                if (product != null && currentProgress < 1f)
                {
                    float remainingTime = product.ProductionDuration * (1f - currentProgress);
                    progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}% ({Mathf.CeilToInt(remainingTime)}s)";
                }
            }
            else
            {
                progressText.gameObject.SetActive(false);
            }
        }

        // Show/hide overlay dimmer (makes image slightly darker during production)
        if (overlayDimmer != null)
        {
            overlayDimmer.gameObject.SetActive(currentProgress > 0f);
            Color dimColor = overlayDimmer.color;
            dimColor.a = 0.3f * currentProgress;
            overlayDimmer.color = dimColor;
        }

        // Show production indicator
        if (productionIndicator != null)
        {
            productionIndicator.SetActive(isProducing);
        }

        // Show/hide glow effect
        if (progressGlow != null)
        {
            progressGlow.gameObject.SetActive(isProducing);
        }
    }

    /// <summary>
    /// Animate progress effects (glow, pulse, etc.)
    /// </summary>
    private void AnimateProgressEffects()
    {
        // Pulsating glow effect
        if (progressGlow != null && progressGlow.gameObject.activeSelf)
        {
            float pulse = (Mathf.Sin(Time.time * glowPulseSpeed) + 1f) * 0.5f;
            Color glowColor = progressGlow.color;
            glowColor.a = 0.3f + (pulse * 0.3f);
            progressGlow.color = glowColor;
        }

        // Rotate production indicator
        if (productionIndicator != null && productionIndicator.activeSelf)
        {
            productionIndicator.transform.Rotate(Vector3.forward, -180f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    private void OnButtonClicked()
    {
        if (product != null)
        {
            onProductSelected?.Invoke(product);
        }
    }

    /// <summary>
    /// Set the interactable state of the button
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (productButton != null)
        {
            productButton.interactable = interactable;
        }
    }

    void OnDestroy()
    {
        if (productButton != null)
        {
            productButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
