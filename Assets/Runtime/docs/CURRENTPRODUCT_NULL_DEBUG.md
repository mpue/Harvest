# ?? CURRENTPRODUCT NULL BUG - DEBUG GUIDE

## Problem
```
Symptom: "Cannot place building - currentProduct is NULL"
Impact: Building placement fails even when started correctly
Critical: YES - breaks core gameplay
```

## Root Cause Analysis

### Possible Causes

#### Cause 1: StartPlacement Never Called
```
Scenario: User tries to place but StartPlacement() was never invoked

Check Console for:
  ? Missing: "=== StartPlacement called ==="
  
If Missing:
  ? ProductionComponent.CompleteProduction() didn't call it
  ? Or building placement triggered differently
  
Fix:
  Ensure CompleteProduction() reaches StartPlacement call
```

#### Cause 2: StartPlacement Called with NULL Product
```
Scenario: StartPlacement(null, manager) called

Console shows:
  "=== StartPlacement called === Product: NULL"
  "Cannot start placement: Product=False"
  
Cause:
  completedProduct is null in ProductionComponent
  
Fix:
  Check currentProduction.product is not null
  Check Product ScriptableObject exists
```

#### Cause 3: CancelPlacement Called Immediately After Start
```
Scenario: StartPlacement() sets currentProduct, then CancelPlacement() clears it

Console shows:
  "=== StartPlacement called === Product: EnergyBlock"
  "? Set currentProduct to: EnergyBlock"
  "=== CancelPlacement called ==="  ? IMMEDIATE!
  "? Placement cancelled"
  
Cause:
  Something triggers CancelPlacement right after StartPlacement
  - User input (right click, ESC)
  - Another system calling it
  - Error in StartPlacement causing auto-cancel
  
Fix:
  Find what calls CancelPlacement
  Check call stack in logs
```

#### Cause 4: currentProduct Cleared During Update
```
Scenario: currentProduct becomes null DURING placement

Console shows:
  "? Started placing EnergyBlock"
  [Later frame]
  "CRITICAL: currentProduct is NULL during Update!"
  
Cause:
  CancelPlacement() called by:
    - User cancelling
    - Camera becoming null
    - Preview destroyed
    - External script
    
Fix:
  Check what triggered CancelPlacement
  Add breakpoint in CancelPlacement()
```

#### Cause 5: Multiple Placement Systems
```
Scenario: Two BuildingPlacement instances fighting

Console shows:
  "StartPlacement called" (multiple times)
  "CancelPlacement called" (from other instance)
  
Cause:
  Multiple BuildingPlacement components in scene
  Each cancelling others
  
Fix:
  Ensure only ONE BuildingPlacement in scene
  Use Singleton pattern or DontDestroyOnLoad
```

## Diagnostic Logs

### Expected WORKING Flow
```
=== StartPlacement called === Product: EnergyBlock, Manager: ResourceManagerPlayer
? Set currentProduct to: EnergyBlock
? Set resourceManager to: ResourceManagerPlayer
? Created preview: EnergyBlock(Clone)
? Started placing EnergyBlock. isPlacing=True, currentProduct=True

[User moves mouse]
[User clicks]
LEFT CLICK: canPlace=True, currentProduct=EnergyBlock
[Building placed successfully]

=== CancelPlacement called === isPlacing=True, preview=True, product=EnergyBlock
  ? Destroyed preview
  ? Hid grid
? Placement cancelled. Was placing: EnergyBlock
```

### BROKEN Flow (currentProduct NULL)
```
=== StartPlacement called === Product: EnergyBlock, Manager: ResourceManagerPlayer
? Set currentProduct to: EnergyBlock
? Created preview: EnergyBlock(Clone)
? Started placing EnergyBlock. isPlacing=True, currentProduct=True

=== CancelPlacement called === isPlacing=True, preview=True, product=EnergyBlock  ? WHY?!
  ? Destroyed preview
? Placement cancelled. Was placing: EnergyBlock

[Next frame]
CRITICAL: currentProduct is NULL during Update!  ? ERROR!
```

### BROKEN Flow (StartPlacement not called)
```
[Production completes]
Building EnergyBlock ready for placement - calling StartPlacement()
  Product: EnergyBlock
  ResourceManager: ResourceManagerPlayer
  BuildingPlacement: NULL  ? PROBLEM!

Cannot place building {completedProduct.ProductName} - No BuildingPlacement system found in scene!
```

## Step-by-Step Debugging

### Step 1: Verify StartPlacement is Called
```
1. Start game
2. Produce a building (e.g., EnergyBlock)
3. Wait for production to complete
4. Check Console

Expected:
  "Building EnergyBlock ready for placement - calling StartPlacement()"
  "=== StartPlacement called === Product: EnergyBlock"
  
If Missing:
  ? ProductionComponent.CompleteProduction() not reaching Player branch
  ? Check: Is building for AI instead of Player?
  ? Check: Is IsBuilding flag set on Product?
```

### Step 2: Verify currentProduct is Set
```
After StartPlacement call, check Console:

Expected:
  "? Set currentProduct to: EnergyBlock"
  "? Started placing EnergyBlock. currentProduct=True"
  
If Missing:
  ? StartPlacement returned early (null checks failed)
  ? Check: product.IsBuilding = true?
  ? Check: product.Prefab assigned?
```

### Step 3: Track CancelPlacement Calls
```
Watch for unexpected CancelPlacement:

Expected (normal):
[User presses ESC or right-click]
  "Cancelling placement (user input)"
  "=== CancelPlacement called ==="
  
Unexpected (bug):
  [No user input]
  "=== CancelPlacement called ==="  ? WHY?!
  
Causes:
  - Camera became null
  - Preview destroyed externally
  - Error in StartPlacement
  - Another system called it
```

### Step 4: Monitor currentProduct in Update
```
During placement, watch for NULL:

Expected:
  [No errors in Update]
  
Unexpected:
  "CRITICAL: currentProduct is NULL during Update!"

When this appears:
  ? Look at previous log line
  ? Should show "=== CancelPlacement called ==="
  ? Find WHY it was called
```

## Common Scenarios

### Scenario A: BuildingPlacement Missing
```
Console:
  "Cannot place building EnergyBlock - No BuildingPlacement system found in scene!"
  
Cause:
  buildingPlacement field is null in ProductionComponent
  
Fix:
  1. Add BuildingPlacement component to scene
  2. Usually attached to GameObject named "BuildingPlacement" or "GameManager"
  3. ProductionComponent.Awake() should auto-find it
  4. Or manually assign in Inspector
  
Verification:
  ProductionComponent ? buildingPlacement field should show object reference
```

### Scenario B: Camera Null During Placement
```
Console:
  "=== StartPlacement called ==="
  "No camera found for building placement!"
  
Cause:
  targetCamera is null when StartPlacement called
  
Fix:
  1. Ensure Main Camera has "MainCamera" tag
  2. Check BuildingPlacement.Awake() logs:
     "BuildingPlacement: Using camera 'Main Camera'"
  3. If not, camera finding failed
  
Verification:
  Should see "? Set currentProduct to: EnergyBlock"
  Should NOT see "No camera found"
```

### Scenario C: Existing Placement Cancelled
```
Console:
  "=== StartPlacement called ==="
  "Cancelling existing placement before starting new one"
  "=== CancelPlacement called ==="
  "? Started placing EnergyBlock"
  
Analysis:
  This is NORMAL if user was already placing something
  New placement should still start successfully
  
Expected:
  Final log: "? Started placing EnergyBlock. currentProduct=True"
```

### Scenario D: Preview Creation Failed
```
Console:
  "=== StartPlacement called ==="
  "? Set currentProduct to: EnergyBlock"
  [No "? Created preview" log]
  
Cause:
  Instantiate(product.Prefab) failed
  - Prefab is null
  - Prefab has errors
  - Memory issue
  
Fix:
  Check Product ScriptableObject ? Prefab field assigned
  Check Prefab has no errors
```

## Quick Fix Checklist

Before debugging, verify:
```
? BuildingPlacement component exists in scene
? Main Camera has "MainCamera" tag
? Product.IsBuilding = true
? Product.Prefab assigned
? Only ONE BuildingPlacement in scene
? ProductionComponent.buildingPlacement assigned (auto or manual)
? Player (not AI) is producing the building
```

## Console Filter Commands

To find specific issues:
```
Filter: "StartPlacement"
  ? See all StartPlacement calls
  ? Should appear when building completes

Filter: "CancelPlacement"
  ? See all cancellations
  ? Look for unexpected ones

Filter: "currentProduct is NULL"
  ? Critical error
  ? Shows WHEN it became null

Filter: "ready for placement"
  ? Shows ProductionComponent calling StartPlacement
  ? Should appear for Player buildings
```

## Breakpoint Strategy

If using debugger:
```
1. Breakpoint: BuildingPlacement.StartPlacement() line 1
   ? Verify product parameter is not null
   ? Verify method is called
 
2. Breakpoint: BuildingPlacement.CancelPlacement() line 1
   ? Check call stack
   ? Who called it?
   ? Is it expected?
   
3. Breakpoint: BuildingPlacement.Update() where currentProduct checked
   ? See when it becomes null
   ? Trace back to CancelPlacement call
```

## Manual Test

Minimal test to verify system:
```csharp
// Add to GameManager or test script:
public BuildingPlacement placement;
public Product testProduct;
public ResourceManager manager;

void Update() {
    if (Input.GetKeyDown(KeyCode.T)) {
      Debug.Log("=== MANUAL TEST: Starting Placement ===");
        placement.StartPlacement(testProduct, manager);
        Debug.Log("=== MANUAL TEST: StartPlacement returned ===");
    }
}

// In Inspector:
// - Assign BuildingPlacement
// - Assign a Product (EnergyBlock ScriptableObject)
// - Assign ResourceManager

// Press T in play mode
// Should start placement successfully
// Check Console for logs
```

## Expected Resolution

After fixes, Console should show:
```
Building EnergyBlock ready for placement - calling StartPlacement()
  Product: EnergyBlock
  ResourceManager: ResourceManagerPlayer
  BuildingPlacement: BuildingPlacement

=== StartPlacement called === Product: EnergyBlock, Manager: ResourceManagerPlayer
? Set currentProduct to: EnergyBlock
? Set resourceManager to: ResourceManagerPlayer
? Created preview: EnergyBlock(Clone)
? Started placing EnergyBlock. isPlacing=True, currentProduct=True

[User places building]
LEFT CLICK: canPlace=True, currentProduct=EnergyBlock
? Placed EnergyBlock at (X, Y, Z)

=== CancelPlacement called === isPlacing=True, preview=True, product=EnergyBlock
  ? Destroyed preview
  ? Hid grid
? Placement cancelled. Was placing: EnergyBlock

? NO "currentProduct is NULL" errors!
? Placement works correctly!
```

## Emergency Workaround

If nothing works, prevent null currentProduct:
```csharp
// In BuildingPlacement.Update(), add:
void Update() {
    if (!isPlacing || currentBuildingPreview == null) return;
    
    // EMERGENCY NULL CHECK
    if (currentProduct == null) {
   Debug.LogError("currentProduct became NULL! Re-finding from preview...");
   
        // Try to recover from preview name
        string previewName = currentBuildingPreview.name.Replace("(Clone)", "");

     // This won't work perfectly but might help debug
        Debug.LogError($"Preview was: {previewName}");
        
   // Force cancel to prevent crash
  CancelPlacement();
        return;
    }
    
    // ... rest of Update ...
}
```

## Success Criteria

System is working when:
```
? StartPlacement is called (log appears)
? currentProduct is set (log appears)
? Preview is created (log appears)
? isPlacing = true
? NO unexpected CancelPlacement calls
? NO "currentProduct is NULL" errors
? Building can be placed successfully
```
