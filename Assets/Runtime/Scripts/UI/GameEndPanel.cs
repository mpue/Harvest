using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Enhanced Victory/Defeat panel with statistics and animations
/// Attach to Victory or Defeat Panel GameObject
/// </summary>
public class GameEndPanel : MonoBehaviour
{
    [Header("Panel Type")]
    [SerializeField] private bool isVictoryPanel = true;
    [SerializeField] private bool isPauseMenu = false; // Can be opened with ESC

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI statisticsText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button resumeButton; // Optional

    [Header("Animation")]
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private string showTrigger = "Show";

    [Header("Audio")]
    [SerializeField] private AudioClip showSound;
    [SerializeField] private AudioManager.AudioCategory audioCategory = AudioManager.AudioCategory.UI;

    private bool isPanelActive = false;

    void Start()
    {
        // Connect buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        // Initialize panel state
        isPanelActive = gameObject.activeSelf;
 
        // Ensure panel is hidden at start
        gameObject.SetActive(false);
        isPanelActive = false;
    }

    /// <summary>
    /// Restart button clicked
    /// </summary>
    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // Fallback
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Quit button clicked
    /// </summary>
    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitToMainMenu();
        }
        else
        {
            // Fallback
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Resume button clicked (if game is paused but not ended)
    /// </summary>
    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }

        HidePanel();
    }

    /// <summary>
    /// Show panel
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        isPanelActive = true;
        
        // Ensure CanvasGroup is properly configured
        ConfigureCanvasGroup(true);

        // Pause game if this is a pause menu
        if (isPauseMenu)
        {
            Time.timeScale = 0f;
            Debug.Log($"Pause Menu shown - Time.timeScale = 0");
        }
    }

    /// <summary>
    /// Hide panel
    /// </summary>
    public void HidePanel()
    {
        // Configure CanvasGroup before hiding
        ConfigureCanvasGroup(false);
        
        gameObject.SetActive(false);
        isPanelActive = false;

        // Resume game if this is a pause menu
        if (isPauseMenu)
        {
            Time.timeScale = 1f;
            Debug.Log($"Pause Menu hidden - Time.timeScale = 1");
        }
    }
    
    /// <summary>
    /// Configure CanvasGroup for proper UI blocking
    /// </summary>
    private void ConfigureCanvasGroup(bool blocking)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        
        // Add CanvasGroup if missing
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log($"Added CanvasGroup to {gameObject.name}");
        }
 
        // Configure based on panel state
        if (blocking)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 1f; // Keep visible for fade-out animations
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
  
    /// <summary>
    /// Called when panel is enabled
    /// </summary>
    void OnEnable()
    {
        isPanelActive = true;
     
        // Play show animation
        if (panelAnimator != null)
        {
          panelAnimator.SetTrigger(showTrigger);
        }

        // Play sound
     if (showSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOneShot2D(showSound, audioCategory);
  }

 // Update statistics
        UpdateStatistics();
    }
    
    /// <summary>
    /// Called when panel is disabled
    /// </summary>
    void OnDisable()
    {
        isPanelActive = false;
    }
    
    /// <summary>
    /// Update statistics display
    /// </summary>
    private void UpdateStatistics()
    {
     if (statisticsText == null) return;

        // Get game time
   float gameTime = Time.time;
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);

        // Count remaining units
        TeamComponent[] allTeams = FindObjectsOfType<TeamComponent>();
        int playerUnits = 0;
        int enemyUnits = 0;

        foreach (TeamComponent teamComp in allTeams)
        {
         if (teamComp == null) continue;

         BaseUnit baseUnit = teamComp.GetComponent<BaseUnit>();
  BuildingComponent building = teamComp.GetComponent<BuildingComponent>();

      if (baseUnit == null && building == null) continue;

            if (teamComp.CurrentTeam == Team.Player)
            {
         playerUnits++;
        }
       else if (teamComp.CurrentTeam == Team.Enemy)
        {
          enemyUnits++;
            }
    }

        // Build statistics string
        string stats = $"Game Time: {minutes:00}:{seconds:00}\n";
  
        if (isVictoryPanel)
        {
       stats += $"Units Remaining: {playerUnits}\n";
  stats += $"Enemies Eliminated: All";
  }
        else if (isPauseMenu)
        {
        stats += $"Player Units: {playerUnits}\n";
   stats += $"Enemy Units: {enemyUnits}";
      }
else
        {
    stats += $"Enemy Units: {enemyUnits}\n";
stats += $"Units Lost: All";
        }

        statisticsText.text = stats;
    }
}
