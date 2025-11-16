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

    private List<BaseUnit> playerHeadquarters = new List<BaseUnit>();
    private static GameManager instance;

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
}
