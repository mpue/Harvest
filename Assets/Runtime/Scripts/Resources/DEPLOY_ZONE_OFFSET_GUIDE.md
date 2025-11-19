# ?? DEPLOY ZONE CONFIGURATION GUIDE

## Configurable Deploy Point Offset

### Inspector Settings
```
DeployZone Component:
  Zone Settings:
    ?? Deploy Radius: 5m
    ?? Deploy Point: (Transform, optional)
    ?? Deploy Point Offset: (0, 0, 3) ? NEW! CONFIGURABLE!
    ?? Show Gizmos: ?
```

### What is Deploy Point Offset?

```
Deploy Point Offset = Local position offset for the deploy zone
  = Relative to ResourceCollector position
  = Allows positioning drop-off point anywhere around building

Example Offsets:
  (0, 0, 3)   = 3 meters in FRONT of building
  (0, 0, -3)  = 3 meters BEHIND building
  (3, 0, 0)   = 3 meters to the RIGHT
  (-3, 0, 0)  = 3 meters to the LEFT
  (2, 0, 2)   = Diagonal front-right
```

### Visual Example
```
ResourceCollector at (0, 0, 0):

Deploy Point Offset: (0, 0, 3)
     ?
     [ResourceCollector]
    ? forward
 [Deploy Zone]  ? 3m in front

Deploy Point Offset: (3, 0, 0)
      ?
     [ResourceCollector] ? [Deploy Zone]  ? 3m to right

Deploy Point Offset: (0, 0, -5)
    ?
        [Deploy Zone]
 ? behind
     [ResourceCollector]  ? 5m behind building
```

## Setup Guide

### Method 1: Default Offset (Recommended)
```
1. Add DeployZone component to ResourceCollector
2. Inspector shows default: Deploy Point Offset (0, 0, 3)
3. This places deploy zone 3m in front of building
4. Adjust as needed!
5. Done! ?
```

### Method 2: Custom Offset
```
1. Add DeployZone component
2. Set Deploy Point Offset to your desired position:
   Examples:
     - Loading dock on side: (5, 0, 0)
     - Back door: (0, 0, -4)
     - Corner: (3, 0, 3)
3. Gizmos show exact position in Scene View
4. Test and adjust!
```

### Method 3: Manual Deploy Point (Advanced)
```
1. Create empty GameObject "DeployPoint"
2. Position it where you want drop-off
3. Assign to DeployZone ? Deploy Point field
4. DeployZone will use this position + offset
```

## Harvester Integration

### How Harvester Finds DeployZone

```csharp
Harvester Logic:

1. Harvester becomes full (currentCarried = 50)
   ? ReturnToCollector() called

2. Find nearest ResourceCollector
   ? targetCollector = FindNearestCollector()

3. Move to ResourceCollector
   ? State: MovingToCollector

4. Check for DeployZone component
   DeployZone deployZone = targetCollector.GetComponent<DeployZone>();

5a. If DeployZone exists:
    ? Use deployZone.DeployPoint as target
    ? Use deployZone.DeployRadius as range
    ? Call deployZone.StartDeploy(this)
    ? DeployZone handles resource transfer ?

5b. If NO DeployZone:
    ? Fallback to legacy system
    ? Use targetCollector.transform.position
    ? Use targetCollector.UnloadRange
    ? Call targetCollector.DepositResources(...) ?
```

### Dual System Support

```
NEW: DeployZone System (Recommended)
  ? Configurable offset
  ? Queue management
  ? Visual gizmos
  ? Team-based access
  ? Better control

OLD: Legacy System (Fallback)
  ? Simple deposit
  ? No queue
  ? Direct to ResourceManager
  ? Works without DeployZone

Both systems work! 
? Add DeployZone for better features
? Remove DeployZone to use legacy
```

## Configuration Examples

### Example 1: Front Loading Dock
```
Scenario: Factory with loading dock in front

ResourceCollector Position: (0, 0, 0)
Deploy Point Offset: (0, 0, 5)

Result:
  [Factory Building]
  ? 5m
  [?? Deploy Zone ??]  ? Harvesters line up here

Perfect for: Factories, warehouses
```

### Example 2: Side Access
```
Scenario: Building with side entrance

ResourceCollector Position: (10, 0, 10)
Deploy Point Offset: (4, 0, 0)

Result:
  [Building] ??? [?? Deploy Zone]  ? Side access

Perfect for: Buildings with limited space
```

### Example 3: Rear Service Area
```
Scenario: Back-door delivery

ResourceCollector Position: (20, 0, 20)
Deploy Point Offset: (0, 0, -6)

Result:
  [?? Deploy Zone]  ? Behind building
      ? 6m
  [Building]

Perfect for: Keeping harvesters out of sight
```

### Example 4: Multi-Direction Access
```
Scenario: Large warehouse with multiple collectors

Collector #1: Offset (5, 0, 0)   ? East side
Collector #2: Offset (-5, 0, 0)  ? West side
Collector #3: Offset (0, 0, 5)   ? North side

Result:
  [?? West][Building]  [?? East]
 ? North
      [?? Deploy]

Perfect for: Large bases with many harvesters
```

## Testing the Offset

### In Scene View
```
1. Select ResourceCollector with DeployZone
2. Gizmos show:
   ?? Green Sphere = Deploy point (with offset!)
   ?? Green Circle = Deploy radius
   ?? Yellow Spheres = Queue positions

3. Adjust Deploy Point Offset in Inspector
4. Gizmos update in real-time ?
5. Visual feedback instant!
```

### In Play Mode
```
1. Start game
2. Select ResourceCollector
3. Gizmos show deploy zone position
4. Spawn harvester
5. Command harvester to gather
6. Watch harvester return to deploy zone
7. Harvester should move to green sphere! ?
```

### Console Logs
```
Expected Logs (Success):
  Harvester_01: Started deploying at DeployZone
  DeployZone: Started deploy from Harvester_01 (Queue: 1/5)
  DeployZone: Harvester deployed 50 Gold
  Harvester_01: Deploy complete via DeployZone

Expected Logs (Legacy):
  Harvester_01: Started unloading (legacy)
  ResourceCollector: Depositing 50 Gold...
  Harvester_01: Unloaded resources (legacy)
```

## Debugging

### Problem: Harvester doesn't find DeployZone
```
Check:
1. ResourceCollector HAS DeployZone component? ?
2. DeployZone is enabled? ?
3. Console shows "Started deploying at DeployZone"?
   - YES ? Working! ?
   - NO, shows "Started unloading (legacy)" ? Using fallback
   - NO logs ? Harvester not reaching collector

Solution:
  Add Debug.Log in HarvesterUnit.UpdateMovingToCollector():
  Debug.Log($"DeployZone found: {deployZone != null}");
```

### Problem: Deploy zone is in wrong position
```
Check:
1. Scene View Gizmos enabled? (top-right)
2. Green sphere shows where deploy zone is
3. Adjust Deploy Point Offset to move it

Example Fix:
  Current: (0, 0, 3) ? Zone in front
  Want: Behind building
  Change to: (0, 0, -5) ? Zone moved behind!
  
Gizmos update instantly ?
```

### Problem: Harvesters overlap at deploy zone
```
Check:
1. Deploy Radius too small?
   - Increase: 5m ? 8m

2. Max Queue Size too large?
   - Decrease: 5 ? 3

3. Queue positions too close?
   - Increase Deploy Radius
   - Queue positions auto-spread around circle

Gizmos show queue positions as yellow spheres ?
```

## Advanced: Multiple Deploy Zones

### Scenario: Large Base with 2 ResourceCollectors

```
Collector A (Main):
  Position: (0, 0, 0)
  Offset: (0, 0, 5)    ? Front
  Radius: 8m
  Queue: 5 harvesters

Collector B (Overflow):
  Position: (20, 0, 0)
  Offset: (0, 0, -5)   ? Back
  Radius: 6m
  Queue: 3 harvesters

Result:
  Main Collector handles first 5 harvesters
  Overflow handles next 3 harvesters
  Total capacity: 8 harvesters!
```

### Harvester Logic
```csharp
// Finds NEAREST collector
targetCollector = FindNearestCollector();

If closest is full (queue maxed):
  ? Harvester waits
  ? Or finds next closest collector

Optimization:
  Spread ResourceCollectors around base
  Harvesters auto-balance to nearest!
```

## Performance Impact

```
Deploy Point Offset:
  ? Zero performance cost
  ? Calculated once in Awake()
  ? Cached in deployPoint Transform
  ? No runtime overhead

Harvester Integration:
  ? Single GetComponent<DeployZone>() call
  ? Cached during movement
  ? No per-frame lookups
  ? Minimal overhead
```

## Best Practices

### 1. Offset Guidelines
```
? Keep offset reasonable (2-10 meters)
? Consider building size
? Leave space for queue (5 positions)
? Test in play mode
? Watch gizmos for visual feedback
```

### 2. Radius Guidelines
```
Small buildings: 3-5m radius
Medium buildings: 5-8m radius
Large buildings: 8-12m radius

Rule: Radius should fit queue positions comfortably
```

### 3. Queue Guidelines
```
Few harvesters (1-3): Queue size 3
Normal load (3-5): Queue size 5
Heavy load (5+): Multiple collectors!
```

## Summary

? **Deploy Point Offset = Fully Configurable**
? **Set in Inspector = (X, Y, Z) local position**
? **Visual Gizmos = See exact position**
? **Harvester Auto-Detection = Works automatically**
? **Dual System Support = DeployZone OR Legacy**
? **Zero Performance Cost = Calculated once**

**Configure Deploy Point Offset to position drop-off zone perfectly for your base layout!** ?????
