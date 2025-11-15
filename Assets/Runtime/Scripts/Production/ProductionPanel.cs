using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("Animation Settings")]
    [SerializeField] private float slideAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve slideAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float offScreenOffsetX = 100f; // Additional offset beyond screen edge

    [Header("Audio")]
    [SerializeField] private AudioClip panelFadeInSound;
    [SerializeField] private AudioClip panelFadeOutSound;
    [SerializeField] private AudioClip productionStartSound;
    [SerializeField] private AudioClip productionCompleteSound;
    [SerializeField] private AudioClip productionCancelSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    private ProductionComponent currentProductionComponent;
    private List<ProductionSlot> productSlots = new List<ProductionSlot>();
    private List<ProductionSlot> queueSlots = new List<ProductionSlot>();

    private RectTransform panelRectTransform;
    private Vector2 onScreenPosition;
    private Vector2 offScreenPosition;
    private Coroutine currentSlideCoroutine;

    void Awake()
    {
        // Get RectTransform for animation
        if (panelRoot != null)
        {
            panelRectTransform = panelRoot.GetComponent<RectTransform>();
            if (panelRectTransform != null)
            {
                // Store the on-screen position (from the editor/prefab)
                onScreenPosition = panelRectTransform.anchoredPosition;

                // Calculate off-screen position (to the right)
                offScreenPosition = new Vector2(
                   onScreenPosition.x + panelRectTransform.rect.width + offScreenOffsetX,
                  onScreenPosition.y
                        );

                // Start off-screen
                panelRectTransform.anchoredPosition = offScreenPosition;
            }
        }

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
            // Only update progress, not recreate slots every frame
            UpdateQueueProgress();
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

        // Check if panel is already visible
        bool wasVisible = panelRoot != null && panelRoot.activeSelf;

        // If panel is already visible, unsubscribe from old events first
        if (wasVisible && currentProductionComponent != null)
        {
            var oldComponent = currentProductionComponent;
            if (oldComponent != null)
            {
                oldComponent.OnQueueChanged -= UpdateQueueDisplay;
                oldComponent.OnProductionStarted -= OnProductionStarted;
                oldComponent.OnProductionCompleted -= OnProductionCompleted;
                oldComponent.OnProductionCancelled -= OnProductionCancelled;
            }
        }

        // Set title
        if (titleText != null)
        {
            titleText.text = $"{baseUnit.UnitName} - Production";
        }

        // Create product slots
        CreateProductSlots();

        // Show panel with animation only if it wasn't visible before
        if (panelRoot != null)
        {
            if (!wasVisible)
            {
                panelRoot.SetActive(true);
                AnimateSlideIn();
            }
            // If already visible, just ensure it stays active
            else
            {
                panelRoot.SetActive(true);
            }
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
        if (panelRoot != null && panelRoot.activeSelf)
        {
            AnimateSlideOut();
        }
        else
        {
            CleanupAndHide();
        }
    }

    /// <summary>
    /// Animate panel sliding in from the right
    /// </summary>
    private void AnimateSlideIn()
    {
        if (panelRectTransform == null) return;

        // Play fade in sound
        PlaySound(panelFadeInSound);

        // Stop any existing animation
        if (currentSlideCoroutine != null)
        {
            StopCoroutine(currentSlideCoroutine);
        }

        currentSlideCoroutine = StartCoroutine(SlideCoroutine(offScreenPosition, onScreenPosition));
    }

    /// <summary>
    /// Animate panel sliding out to the right
    /// </summary>
    private void AnimateSlideOut()
    {
        if (panelRectTransform == null)
        {
            CleanupAndHide();
            return;
        }

        // Play fade out sound
        PlaySound(panelFadeOutSound);

        // Stop any existing animation
        if (currentSlideCoroutine != null)
        {
            StopCoroutine(currentSlideCoroutine);
        }

        currentSlideCoroutine = StartCoroutine(SlideCoroutine(panelRectTransform.anchoredPosition, offScreenPosition, true));
    }

    /// <summary>
    /// Coroutine for smooth sliding animation
    /// </summary>
    private IEnumerator SlideCoroutine(Vector2 startPos, Vector2 endPos, bool hideOnComplete = false)
    {
        float elapsed = 0f;

        while (elapsed < slideAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideAnimationDuration);
            float curveValue = slideAnimationCurve.Evaluate(t);

            panelRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        // Ensure final position is set
        panelRectTransform.anchoredPosition = endPos;

        if (hideOnComplete)
        {
            CleanupAndHide();
        }

        currentSlideCoroutine = null;
    }

    /// <summary>
    /// Cleanup and hide the panel without animation
    /// </summary>
    private void CleanupAndHide()
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

        // Reset position to off-screen
        if (panelRectTransform != null)
        {
            panelRectTransform.anchoredPosition = offScreenPosition;
        }
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

        // Update queue slots (recreate if count changed)
        UpdateQueueSlots();
    }

    /// <summary>
    /// Update only the progress of queue slots (called every frame)
    /// </summary>
    private void UpdateQueueProgress()
    {
      if (currentProductionComponent == null) return;

        // Update button states
if (cancelCurrentButton != null)
        {
   cancelCurrentButton.interactable = currentProductionComponent.IsProducing;
 }

        if (clearQueueButton != null)
   {
     clearQueueButton.interactable = currentProductionComponent.QueueCount > 0;
     }

        // Update progress for existing slots without recreating them
        if (queueSlots.Count > 0)
        {
     for (int i = 0; i < queueSlots.Count; i++)
            {
     if (queueSlots[i] != null)
   {
        // Update progress for the first item (currently producing)
        if (i == 0 && currentProductionComponent.IsProducing)
      {
    queueSlots[i].UpdateProgress(currentProductionComponent.CurrentProductionProgress);
  }
         else
                {
          queueSlots[i].UpdateProgress(0f);
        }
       }
   }
        }
    }

    /// <summary>
    /// Update queue slot visuals
    /// </summary>
    private void UpdateQueueSlots()
    {
  if (queueContainer == null || queueSlotPrefab == null) return;

        // Get queued products
     List<Product> queuedProducts = currentProductionComponent.GetQueuedProducts();

        // If slot count doesn't match, recreate all slots
      if (queueSlots.Count != queuedProducts.Count)
 {
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

       // Set initial progress
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
        // If count matches, just update progress (this is now handled by UpdateQueueProgress)
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
