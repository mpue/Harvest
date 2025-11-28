using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main game manager - handles game initialization and player setup
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int numberOfPlayers = 2;

    [Header("Headquarter Setup")]
    [SerializeField] private GameObject headquarterPrefab;
    [SerializeField] private Vector3[] playerStartPositions;
    [SerializeField] private float startPositionSpacing = 50f;

    [Header("Resource Management")]
    [SerializeField] private ResourceManager[] playerResourceManagers;

    [Header("Building System")]
    [SerializeField] private BuildingPlacement buildingPlacement;

    [Header("Game State")]
    [SerializeField] private bool checkWinLossConditions = true;
    [SerializeField] private float checkInterval = 2f; // Check every 2 seconds
    [SerializeField] private Team playerTeam = Team.Player;
    [SerializeField] private Team enemyTeam = Team.Enemy;

    [Header("UI Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private bool pauseOnGameEnd = true;

    private List<BaseUnit> playerHeadquarters = new List<BaseUnit>();
    private static GameManager instance;
    private float checkTimer = 0f;
    private bool gameEnded = false;

    public static GameManager Instance => instance;
    public ResourceManager GetPlayerResourceManager(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerResourceManagers.Length)
        {
            return playerResourceManagers[playerIndex];
        }
        return null;
    }

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find or create building placement
        if (buildingPlacement == null)
        {
            buildingPlacement = FindObjectOfType<BuildingPlacement>();
            if (buildingPlacement == null)
            {
                GameObject placementObj = new GameObject("BuildingPlacement");
                buildingPlacement = placementObj.AddComponent<BuildingPlacement>();
            }
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (!gameEnded && checkWinLossConditions)
        {
            checkTimer += Time.deltaTime;

            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                CheckWinLossConditions();
            }
        }
    }

    /// <summary>
    /// Initialize the game - setup players and headquarters
    /// </summary>
    private void InitializeGame()
    {
        // Initialize resource managers for each player
        InitializeResourceManagers();

        // Setup headquarters for each player
        SetupHeadquarters();

        Debug.Log($"Game initialized with {numberOfPlayers} players");
    }

    /// <summary>
    /// Initialize resource managers
    /// </summary>
    private void InitializeResourceManagers()
    {
        if (playerResourceManagers == null || playerResourceManagers.Length == 0)
        {
            playerResourceManagers = new ResourceManager[numberOfPlayers];
            for (int i = 0; i < numberOfPlayers; i++)
            {
                GameObject rmObj = new GameObject($"Player{i + 1}_ResourceManager");
                rmObj.transform.SetParent(transform);
                playerResourceManagers[i] = rmObj.AddComponent<ResourceManager>();
            }
        }
    }

    /// <summary>
    /// Setup headquarters for all players
    /// </summary>
    private void SetupHeadquarters()
    {
        if (headquarterPrefab == null)
        {
            Debug.LogError("Headquarter prefab is not assigned!");
            return;
        }

        // Generate start positions if not specified
        if (playerStartPositions == null || playerStartPositions.Length < numberOfPlayers)
        {
            GenerateStartPositions();
        }

        // Create headquarters for each player
        for (int i = 0; i < numberOfPlayers; i++)
        {
            Vector3 position = playerStartPositions[i];
            GameObject hqObj = Instantiate(headquarterPrefab, position, Quaternion.identity);
            hqObj.name = $"Player{i + 1}_Headquarter";

            // Setup BaseUnit component
            BaseUnit baseUnit = hqObj.GetComponent<BaseUnit>();
            if (baseUnit == null)
            {
                baseUnit = hqObj.AddComponent<BaseUnit>();
            }

            // Setup ProductionComponent
            ProductionComponent productionComp = hqObj.GetComponent<ProductionComponent>();
            if (productionComp == null)
            {
                productionComp = hqObj.AddComponent<ProductionComponent>();
            }

            // Setup BuildingComponent
            BuildingComponent buildingComp = hqObj.GetComponent<BuildingComponent>();
            if (buildingComp == null)
            {
                buildingComp = hqObj.AddComponent<BuildingComponent>();
            }

            // Assign resource manager
            if (i < playerResourceManagers.Length)
            {
                // We need to expose a way to set the resource manager
                // This will be done through reflection or by making it public
                var field = typeof(ProductionComponent).GetField("resourceManager",
         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(productionComp, playerResourceManagers[i]);

                // Also set building placement
                var placementField = typeof(ProductionComponent).GetField("buildingPlacement",
                  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                placementField?.SetValue(productionComp, buildingPlacement);
            }

            // Setup team component
            TeamComponent teamComp = hqObj.GetComponent<TeamComponent>();
            if (teamComp == null)
            {
                teamComp = hqObj.AddComponent<TeamComponent>();
            }
            // Set team ID
            var teamField = typeof(TeamComponent).GetField("team",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            teamField?.SetValue(teamComp, i);

            playerHeadquarters.Add(baseUnit);

            Debug.Log($"Created headquarter for Player {i + 1} at {position}");
        }
    }

    /// <summary>
    /// Generate evenly spaced start positions
    /// </summary>
    private void GenerateStartPositions()
    {
        playerStartPositions = new Vector3[numberOfPlayers];

        if (numberOfPlayers == 1)
        {
            playerStartPositions[0] = Vector3.zero;
        }
        else if (numberOfPlayers == 2)
        {
            playerStartPositions[0] = new Vector3(-startPositionSpacing, 0, 0);
            playerStartPositions[1] = new Vector3(startPositionSpacing, 0, 0);
        }
        else
        {
            // Arrange in a circle
            float angleStep = 360f / numberOfPlayers;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                playerStartPositions[i] = new Vector3(
                  Mathf.Cos(angle) * startPositionSpacing,
                       0,
                    Mathf.Sin(angle) * startPositionSpacing
                  );
            }
        }
    }

    /// <summary>
    /// Get headquarter for a specific player
    /// </summary>
    public BaseUnit GetPlayerHeadquarter(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerHeadquarters.Count)
   {
   return playerHeadquarters[playerIndex];
  }
  return null;
    }
 
    /// <summary>
    /// Check win/loss conditions
    /// </summary>
    private void CheckWinLossConditions()
    {
   // Find all units and buildings by team
     TeamComponent[] allTeamComponents = FindObjectsOfType<TeamComponent>();
   
        int playerUnitsCount = 0;
  int enemyUnitsCount = 0;
        
   foreach (TeamComponent teamComp in allTeamComponents)
        {
     if (teamComp == null) continue;
  
      // Nur Einheiten zählen (mit BaseUnit oder BuildingComponent)
       BaseUnit baseUnit = teamComp.GetComponent<BaseUnit>();
       BuildingComponent building = teamComp.GetComponent<BuildingComponent>();
   
    if (baseUnit == null && building == null) continue;
     
    if (teamComp.CurrentTeam == playerTeam)
   {
       playerUnitsCount++;
     }
       else if (teamComp.CurrentTeam == enemyTeam)
 {
      enemyUnitsCount++;
  }
  }
  
        // Check win condition: Alle feindlichen Einheiten eliminiert
  if (enemyUnitsCount == 0 && playerUnitsCount > 0)
    {
      OnVictory();
  }
  // Check loss condition: Alle eigenen Einheiten eliminiert
  else if (playerUnitsCount == 0 && enemyUnitsCount > 0)
    {
     OnDefeat();
        }
    }
    
    /// <summary>
    /// Called when player wins
    /// </summary>
    private void OnVictory()
    {
     if (gameEnded) return;
     
   gameEnded = true;
        
  Debug.Log("?? VICTORY! All enemy units have been eliminated!");
   
   // Show victory panel
   if (victoryPanel != null)
   {
     victoryPanel.SetActive(true);
   }
    else
     {
      Debug.LogWarning("Victory Panel is not assigned in GameManager!");
        }
        
   // Pause game if enabled
  if (pauseOnGameEnd)
        {
     Time.timeScale = 0f;
     Debug.Log("Game paused");
  }
    }
    
    /// <summary>
    /// Called when player loses
    /// </summary>
    private void OnDefeat()
    {
   if (gameEnded) return;
        
      gameEnded = true;
   
  Debug.Log("?? DEFEAT! All your units have been eliminated!");
   
  // Show defeat panel
   if (defeatPanel != null)
      {
 defeatPanel.SetActive(true);
   }
   else
  {
            Debug.LogWarning("Defeat Panel is not assigned in GameManager!");
      }
     
   // Pause game if enabled
        if (pauseOnGameEnd)
 {
 Time.timeScale = 0f;
   Debug.Log("Game paused");
        }
    }
    
    /// <summary>
    /// Restart the game (call from UI button)
    /// </summary>
    public void RestartGame()
    {
  Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
       UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
 
    /// <summary>
    /// Quit to main menu (call from UI button)
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
  // Load main menu scene (adjust scene name as needed)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
}
    
    /// <summary>
    /// Resume game (if paused)
 /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Get game ended status
    /// </summary>
    public bool IsGameEnded()
    {
  return gameEnded;
    }
}
