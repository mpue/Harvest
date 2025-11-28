using UnityEngine;

/// <summary>
/// Controls pause menu toggling with ESC key
/// This script should be active in the scene to detect input even when panel is hidden
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameEndPanel pauseMenuPanel;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool enablePauseMenu = true;
    
    private bool isPaused = false;
    
    void Update()
    {
        // Only handle pause if enabled
   if (!enablePauseMenu) return;

        // Check for pause key
        if (Input.GetKeyDown(pauseKey))
     {
         // Don't allow pausing if game has ended (Victory/Defeat)
     if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
    {
  return;
      }
            
 TogglePause();
        }
    }
    
    /// <summary>
    /// Toggle pause menu
    /// </summary>
    private void TogglePause()
    {
        if (pauseMenuPanel == null)
 {
          Debug.LogWarning("PauseMenuController: No pause menu panel assigned!");
            return;
        }
  
        isPaused = !isPaused;
        
        if (isPaused)
        {
         pauseMenuPanel.ShowPanel();
    }
 else
 {
            pauseMenuPanel.HidePanel();
        }
    }
    
    /// <summary>
    /// Show pause menu (called from other scripts)
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel == null) return;
   
        isPaused = true;
      pauseMenuPanel.ShowPanel();
    }
    
    /// <summary>
    /// Hide pause menu (called from other scripts)
    /// </summary>
    public void HidePauseMenu()
  {
    if (pauseMenuPanel == null) return;
        
        isPaused = false;
        pauseMenuPanel.HidePanel();
    }
    
    /// <summary>
    /// Enable/disable pause menu
    /// </summary>
    public void SetPauseMenuEnabled(bool enabled)
    {
enablePauseMenu = enabled;
    }
    
    /// <summary>
    /// Get current pause state
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
}
