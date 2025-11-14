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

  private Product product;
    private System.Action<Product> onProductSelected;

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
        if (progressBar != null)
 {
     progressBar.SetActive(progress > 0f);
        }

 if (progressFill != null)
     {
       progressFill.fillAmount = progress;
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
