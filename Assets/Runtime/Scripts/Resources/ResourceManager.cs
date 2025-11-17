using UnityEngine;
using System;

/// <summary>
/// Manages player resources including energy
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private int food = 500;
    [SerializeField] private int wood = 500;
    [SerializeField] private int stone = 500;
    [SerializeField] private int gold = 500;

    [Header("Energy System")]
    [SerializeField] private int currentEnergy = 0;
    [SerializeField] private int maxEnergy = 0;
    [SerializeField] private int startingEnergy = 20;

    // Events for UI updates
    public event Action<int, int, int, int> OnResourcesChanged;
    public event Action<int, int> OnEnergyChanged;

    // Properties
    public int Food => food;
    public int Wood => wood;
    public int Stone => stone;
    public int Gold => gold;
    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;
    public int AvailableEnergy => maxEnergy - currentEnergy;

    void Start()
    {
        // Initialize with starting energy
        maxEnergy = startingEnergy;
        NotifyResourcesChanged();
        NotifyEnergyChanged();
    }

    /// <summary>
    /// Check if player can afford costs
    /// </summary>
    public bool CanAfford(int foodCost, int woodCost, int stoneCost, int goldCost)
    {
        return food >= foodCost &&
     wood >= woodCost &&
          stone >= stoneCost &&
               gold >= goldCost;
    }

    /// <summary>
    /// Check if enough energy is available
    /// </summary>
    public bool HasAvailableEnergy(int energyCost)
    {
        return (currentEnergy + energyCost) <= maxEnergy;
    }

    /// <summary>
    /// Spend resources
    /// </summary>
    public bool SpendResources(int foodCost, int woodCost, int stoneCost, int goldCost)
    {
        if (!CanAfford(foodCost, woodCost, stoneCost, goldCost))
        {
            return false;
        }

        food -= foodCost;
        wood -= woodCost;
        stone -= stoneCost;
        gold -= goldCost;

        NotifyResourcesChanged();
        return true;
    }

    /// <summary>
    /// Add resources
    /// </summary>
    public void AddResources(int foodAmount, int woodAmount, int stoneAmount, int goldAmount)
    {
        food += foodAmount;
        wood += woodAmount;
        stone += stoneAmount;
        gold += goldAmount;

        NotifyResourcesChanged();
    }

    /// <summary>
    /// Increase max energy capacity
    /// </summary>
    public void IncreaseMaxEnergy(int amount)
    {
        maxEnergy += amount;
        NotifyEnergyChanged();
        Debug.Log($"Max energy increased by {amount}. New max: {maxEnergy}");
    }

    /// <summary>
    /// Decrease max energy capacity
    /// </summary>
    public void DecreaseMaxEnergy(int amount)
    {
        maxEnergy = Mathf.Max(0, maxEnergy - amount);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        NotifyEnergyChanged();
        Debug.Log($"Max energy decreased by {amount}. New max: {maxEnergy}");
    }

    /// <summary>
    /// Consume energy for a building
    /// </summary>
    public bool ConsumeEnergy(int amount)
    {
        if (!HasAvailableEnergy(amount))
        {
            return false;
        }

        currentEnergy += amount;
        NotifyEnergyChanged();
        return true;
    }

    /// <summary>
    /// Release energy when a building is destroyed
    /// </summary>
    public void ReleaseEnergy(int amount)
    {
        currentEnergy = Mathf.Max(0, currentEnergy - amount);
        NotifyEnergyChanged();
    }

    private void NotifyResourcesChanged()
    {
        OnResourcesChanged?.Invoke(food, wood, stone, gold);
    }

    private void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}
