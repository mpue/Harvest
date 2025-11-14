using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI Panel for displaying and managing unit production
/// </summary>
public class ProductionPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform productSlotsContainer;
    [SerializeField] private GameObject productSlotPrefab;

    [Header("Queue Display")]
    [SerializeField] private Transform queueContainer;
    [SerializeField] private GameObject queueSlotPrefab;
    [SerializeField] private TextMeshProUGUI queueCountText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cancelCurrentButton;
    [SerializeField] private Button clearQueueButton;

    [Header("Audio")]
    [SerializeField] private AudioClip productionStartSound;
    [SerializeField] private AudioClip productionCompleteSound;
    [SerializeField] private AudioClip productionCancelSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    private ProductionComponent currentProductionComponent;
    private List<ProductionSlot> productSlots = new List<ProductionSlot>();
    private List<ProductionSlot> queueSlots = new List<ProductionSlot>();

    void Awake()
    {
        // Hook up button events
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        if (cancelCurrentButton != null)
        {
            cancelCurrentButton.onClick.AddListener(OnCancelCurrentClicked);
        }

        if (clearQueueButton != null)
        {
            clearQueueButton.onClick.AddListener(OnClearQueueClicked);
        }

        // Hide panel at start
        Hide();
    }

    void Update()
    {
        // Update queue display if panel is open
        if (panelRoot != null && panelRoot.activeSelf && currentProductionComponent != null)
        {
            UpdateQueueDisplay();
        }
    }

    /// <summary>
    /// Show the production panel for a specific BaseUnit
    /// </summary>
    public void Show(BaseUnit baseUnit)
    {
        if (baseUnit == null)
        {
            Debug.LogWarning("Cannot show production panel: BaseUnit is null");
            return;
        }

        currentProductionComponent = baseUnit.GetComponent<ProductionComponent>();

        if (currentProductionComponent == null)
        {
            Debug.LogWarning($"BaseUnit {baseUnit.UnitName} does not have a ProductionComponent");
            return;
        }

        // Set title
        if (titleText != null)
        {
            titleText.text = $"{baseUnit.UnitName} - Production";
        }

        // Create product slots
        CreateProductSlots();

        // Show panel
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Subscribe to production events
        currentProductionComponent.OnQueueChanged += UpdateQueueDisplay;
        currentProductionComponent.OnProductionStarted += OnProductionStarted;
        currentProductionComponent.OnProductionCompleted += OnProductionCompleted;
        currentProductionComponent.OnProductionCancelled += OnProductionCancelled;

        // Initial queue update
        UpdateQueueDisplay();
    }

    /// <summary>
    /// Hide the production panel
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Unsubscribe from events
        if (currentProductionComponent != null)
        {
            currentProductionComponent.OnQueueChanged -= UpdateQueueDisplay;
            currentProductionComponent.OnProductionStarted -= OnProductionStarted;
            currentProductionComponent.OnProductionCompleted -= OnProductionCompleted;
            currentProductionComponent.OnProductionCancelled -= OnProductionCancelled;
        }

        currentProductionComponent = null;
        ClearProductSlots();
        ClearQueueSlots();
    }

    /// <summary>
    /// Create UI slots for all available products
    /// </summary>
    private void CreateProductSlots()
    {
        ClearProductSlots();

        if (currentProductionComponent == null || productSlotsContainer == null || productSlotPrefab == null)
        {
            return;
        }

        foreach (Product product in currentProductionComponent.AvailableProducts)
        {
            if (product == null) continue;

            GameObject slotObj = Instantiate(productSlotPrefab, productSlotsContainer);
            ProductionSlot slot = slotObj.GetComponent<ProductionSlot>();

            if (slot != null)
            {
                slot.Initialize(product, OnProductSelected);
                productSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// Clear all product slots
    /// </summary>
    private void ClearProductSlots()
    {
        foreach (var slot in productSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        productSlots.Clear();
    }

    /// <summary>
    /// Update the production queue display
    /// </summary>
    private void UpdateQueueDisplay()
    {
        if (currentProductionComponent == null) return;

        // Update queue count text
        if (queueCountText != null)
        {
            queueCountText.text = $"Queue: {currentProductionComponent.QueueCount}/{currentProductionComponent.MaxQueueSize}";
        }

        // Update button states
        if (cancelCurrentButton != null)
        {
            cancelCurrentButton.interactable = currentProductionComponent.IsProducing;
        }

        if (clearQueueButton != null)
        {
            clearQueueButton.interactable = currentProductionComponent.QueueCount > 0;
        }

        // Update queue slots
        UpdateQueueSlots();
    }

    /// <summary>
    /// Update queue slot visuals
    /// </summary>
    private void UpdateQueueSlots()
    {
        if (queueContainer == null || queueSlotPrefab == null) return;

        // Get queued products
        List<Product> queuedProducts = currentProductionComponent.GetQueuedProducts();

        // Clear existing slots
        ClearQueueSlots();

        // Create slots for queued products
        for (int i = 0; i < queuedProducts.Count; i++)
        {
            Product product = queuedProducts[i];

            GameObject slotObj = Instantiate(queueSlotPrefab, queueContainer);
            ProductionSlot slot = slotObj.GetComponent<ProductionSlot>();

            if (slot != null)
            {
                slot.Initialize(product, null); // No callback needed for queue slots

                // Update progress for the first item (currently producing)
                if (i == 0 && currentProductionComponent.IsProducing)
                {
                    slot.UpdateProgress(currentProductionComponent.CurrentProductionProgress);
                }
                else
                {
                    slot.UpdateProgress(0f);
                }

                queueSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// Clear all queue slots
    /// </summary>
    private void ClearQueueSlots()
    {
        foreach (var slot in queueSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        queueSlots.Clear();
    }

    /// <summary>
    /// Called when a product is selected for production
    /// </summary>
    private void OnProductSelected(Product product)
    {
        if (currentProductionComponent == null || product == null)
        {
            return;
        }

        // TODO: Check if player can afford the product when resource system is implemented

        bool success = currentProductionComponent.AddToQueue(product);

        if (success)
        {
            Debug.Log($"Added {product.ProductName} to production queue");
            PlaySound(buttonClickSound);
        }
        else
        {
            Debug.LogWarning($"Failed to add {product.ProductName} to queue");
            // TODO: Show error message to player
        }
    }

    /// <summary>
    /// Called when production starts
    /// </summary>
    private void OnProductionStarted(Product product)
    {
        if (product != null)
        {
            Debug.Log($"Production started: {product.ProductName}");
            PlaySound(productionStartSound);
        }
    }

    /// <summary>
    /// Called when production completes
    /// </summary>
    private void OnProductionCompleted(Product product, GameObject spawnedUnit)
    {
        if (product != null)
        {
            Debug.Log($"Production completed: {product.ProductName}");
            PlaySound(productionCompleteSound);
        }
    }

    /// <summary>
    /// Called when production is cancelled
    /// </summary>
    private void OnProductionCancelled(Product product)
    {
        if (product != null)
        {
            Debug.Log($"Production cancelled: {product.ProductName}");
            PlaySound(productionCancelSound);
        }
    }

    /// <summary>
    /// Play a UI sound
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot2D(clip, AudioManager.AudioCategory.UI, soundVolume);
        }
        else
        {
            // Fallback if no AudioManager exists
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, soundVolume);
        }
    }

    /// <summary>
    /// Cancel the current production
    /// </summary>
    private void OnCancelCurrentClicked()
    {
        PlaySound(buttonClickSound);
   
        if (currentProductionComponent != null)
        {
        currentProductionComponent.CancelCurrentProduction();
      }
    }

    /// <summary>
    /// Clear the entire production queue
    /// </summary>
    private void OnClearQueueClicked()
  {
     PlaySound(buttonClickSound);
        
 if (currentProductionComponent != null)
{
      currentProductionComponent.CancelQueue();
     }
    }

    /// <summary>
/// Called when close button is clicked
/// </summary>
    private void OnCloseButtonClicked()
    {
  PlaySound(buttonClickSound);
        Hide();
    }

    void OnDestroy()
    {
   // Clean up button listeners
 if (closeButton != null)
   {
  closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (cancelCurrentButton != null)
{
      cancelCurrentButton.onClick.RemoveListener(OnCancelCurrentClicked);
        }

 if (clearQueueButton != null)
        {
    clearQueueButton.onClick.RemoveListener(OnClearQueueClicked);
   }

     // Unsubscribe from production events
   if (currentProductionComponent != null)
        {
            currentProductionComponent.OnQueueChanged -= UpdateQueueDisplay;
     currentProductionComponent.OnProductionStarted -= OnProductionStarted;
 currentProductionComponent.OnProductionCompleted -= OnProductionCompleted;
  currentProductionComponent.OnProductionCancelled -= OnProductionCancelled;
        }
    }
}
