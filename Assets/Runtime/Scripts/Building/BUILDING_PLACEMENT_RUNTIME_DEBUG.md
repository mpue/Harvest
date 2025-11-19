# ?? BUILDING PLACEMENT RUNTIME DEBUG PROTOCOL

## Problem Report
```
Status: Building Placement unreliable in Runtime Build
Severity: CRITICAL
Impact: Game unplayable if buildings can't be placed
```

## Diagnostic Checklist

### Phase 1: Camera Detection
```
Test 1: Camera.main
  ? Camera exists in scene
  ? Camera has "MainCamera" tag
  ? Camera is enabled
  ? Camera is in active scene
  
Test 2: Fallback Systems
  ? FindGameObjectWithTag("MainCamera") works
  ? FindObjectOfType<Camera>() works
  ? Manual camera assignment works
  
Expected Log:
  "BuildingPlacement: Using camera 'Main Camera'"
  
If Missing:
  "BuildingPlacement: NO CAMERA FOUND!"
  ? CRITICAL: Fix camera setup!
```

### Phase 2: Input System
```
Test 1: Mouse Click Detection
  ? Input.GetMouseButtonDown(0) triggers
  ? Input.GetMouseButtonDown(1) triggers
  ? Input.GetKeyDown(KeyCode.Escape) triggers
  
Test 2: New vs Old Input System
? Project Settings ? Player ? Active Input Handling
  ? Should be: "Input Manager (Old)" or "Both"
  ? NOT: "Input System Package (New)" only
  
Expected Behavior:
  Click ? "Cannot place building here!" or Building placed
  
If Not Working:
  No response to clicks
  ? CRITICAL: Fix input system!
```

### Phase 3: Raycast/Ground Detection
```
Test 1: Ground Layer
  ? Ground/Terrain has layer set
  ? BuildingPlacement.groundLayer includes correct layer
  ? Raycast hits ground
  
Test 2: Raycast Debug
  ? Add Debug.DrawRay in UpdateBuildingPreview()
  ? Visualize raycast in Scene View during play
  
Expected Log:
  "Found valid position at (X, Y, Z)"
  
If Failing:
  "Raycast failed at (X, Y, Z) (no ground hit)"
  ? CRITICAL: Fix ground layer!
```

### Phase 4: Preview Rendering
```
Test 1: Preview Visibility
  ? Preview building spawns
  ? Preview follows mouse
  ? Preview shows green/red color
  
Test 2: Material/Shader
  ? Standard shader available in build
  ? Transparent materials work
  ? No pink/missing shader textures
  
Expected Behavior:
  Semi-transparent preview follows mouse
  
If Not Visible:
  Preview spawned but invisible
  ? WARNING: Fix materials/shaders!
```

## Common Build-Specific Issues

### Issue 1: Camera.main Returns Null
```csharp
Cause: Camera doesn't have "MainCamera" tag in build

Fix:
1. Open scene with Main Camera
2. Select Main Camera GameObject
3. Inspector ? Tag dropdown ? "MainCamera"
4. File ? Save
5. Rebuild

Verification:
GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
Debug.Log($"Main Camera found: {mainCam != null}");
```

### Issue 2: Input System Mismatch
```csharp
Cause: New Input System enabled, old Input.GetMouseButton() doesn't work

Fix:
1. Edit ? Project Settings ? Player
2. Other Settings ? Active Input Handling
3. Change to: "Input Manager (Old)" or "Both"
4. Rebuild

Verification:
void Update() {
    if (Input.GetMouseButtonDown(0)) {
        Debug.Log("LEFT CLICK WORKS!");
    }
}
```

### Issue 3: LayerMask Serialization
```csharp
Cause: LayerMask = -1 (Everything) causes issues in build

Fix:
1. Select BuildingPlacement GameObject
2. Inspector ? Ground Layer
3. Select specific layer: "Ground" or "Terrain"
4. NOT "Everything" or "Nothing"
5. Save scene
6. Rebuild

Verification:
Debug.Log($"Ground Layer Value: {groundLayer.value}");
Debug.Log($"Ground Layer Includes Layer 0: {((groundLayer.value & (1 << 0)) != 0)}");
```

### Issue 4: Missing Prefab References
```csharp
Cause: Product.Prefab is null in build

Fix:
1. Check all Product ScriptableObjects
2. Prefab field must be assigned
3. Prefab must be in Resources folder OR referenced in scene
4. Rebuild

Verification:
if (product.Prefab == null) {
    Debug.LogError($"Product {product.ProductName} has NULL prefab!");
}
```

### Issue 5: Scene Not in Build Settings
```csharp
Cause: Scene not included in build

Fix:
1. File ? Build Settings
2. Scenes in Build list
3. Add current scene if missing
4. Rebuild

Verification:
Debug.Log($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
```

## Enhanced Debugging System

### Add Debug Overlay (Temporary)
```csharp
// Add to BuildingPlacement.cs
void OnGUI() {
    // Existing placement UI...
    
    // DEBUG OVERLAY (remove after fixing!)
    if (Application.isPlaying && !Application.isEditor) {
        GUIStyle debugStyle = new GUIStyle(GUI.skin.box);
        debugStyle.fontSize = 12;
        debugStyle.normal.textColor = Color.white;
   debugStyle.alignment = TextAnchor.UpperLeft;
     
      string debugInfo = "=== BUILD DEBUG ===\n";
        debugInfo += $"Camera: {(targetCamera != null ? targetCamera.name : "NULL")}\n";
        debugInfo += $"IsPlacing: {isPlacing}\n";
  debugInfo += $"Preview: {(currentBuildingPreview != null ? "Active" : "NULL")}\n";
   debugInfo += $"CanPlace: {canPlace}\n";
        debugInfo += $"Mouse Pos: {Input.mousePosition}\n";
 debugInfo += $"Ground Layer: {groundLayer.value}\n";
        
        // Input test
        if (Input.GetMouseButtonDown(0)) debugInfo += "CLICK DETECTED!\n";
        
      GUI.Box(new Rect(10, 100, 300, 200), debugInfo, debugStyle);
    }
}
```

### Add Raycast Visualization
```csharp
// Add to UpdateBuildingPreview()
private void UpdateBuildingPreview() {
    if (targetCamera == null) {
        Debug.LogError("Camera is null!");
        return;
    }

    Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
    
    // VISUAL DEBUG (shows in Scene View during Play Mode)
    Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 0.1f);
    
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit, 1000f, groundLayer)) {
        // VISUAL DEBUG (shows hit point)
     Debug.DrawLine(hit.point, hit.point + Vector3.up * 5f, Color.green, 0.1f);
     
        // ... rest of code ...
    }
    else {
        Debug.LogWarning($"Raycast MISS! Origin: {ray.origin}, Direction: {ray.direction}");
    }
}
```

### Add Click Detection Test
```csharp
// Add to Update()
void Update() {
    // TEMPORARY DEBUG
    if (Input.GetMouseButtonDown(0)) {
        Debug.Log("=== LEFT CLICK DETECTED ===");
        Debug.Log($"Camera: {targetCamera?.name ?? "NULL"}");
        Debug.Log($"IsPlacing: {isPlacing}");
        Debug.Log($"Preview: {currentBuildingPreview?.name ?? "NULL"}");
    }
    
    // ... rest of update code ...
}
```

## Step-by-Step Debugging Process

### Step 1: Verify Camera
```
1. Build game
2. Run .exe
3. Check Player.log for:
   "BuildingPlacement: Using camera 'Main Camera'"
   
If NOT found:
   ? Open Unity scene
   ? Select Main Camera
   ? Set Tag to "MainCamera"
   ? Save scene
   ? Rebuild
```

### Step 2: Verify Input
```
1. In build, click anywhere
2. Check Player.log for:
   "LEFT CLICK DETECTED"
   
If NOT found:
   ? Project Settings ? Player ? Active Input Handling
   ? Change to "Both" or "Input Manager (Old)"
   ? Rebuild
```

### Step 3: Verify Raycast
```
1. Start building placement
2. Move mouse over ground
3. Check Player.log for:
   "Found valid position at (...)"
   
If seeing "Raycast MISS!":
   ? Check Ground Layer setting
   ? Ensure ground has collider
   ? Rebuild
```

### Step 4: Verify Preview
```
1. Start building placement
2. Look for semi-transparent building preview
   
If NOT visible:
   ? Check console for "Started placing [ProductName]"
   ? Check if preview spawns but is invisible
   ? Check material/shader issues
```

## Player.log Location

### Windows
```
%USERPROFILE%\AppData\LocalLow\[CompanyName]\[ProductName]\Player.log

Example:
C:\Users\YourName\AppData\LocalLow\DefaultCompany\Harvest\Player.log
```

### Mac
```
~/Library/Logs/[CompanyName]/[ProductName]/Player.log
```

### Linux
```
~/.config/unity3d/[CompanyName]/[ProductName]/Player.log
```

## Quick Fix Checklist

Before rebuilding, ensure:
```
? Main Camera has "MainCamera" tag
? BuildingPlacement GameObject exists in scene
? Ground Layer set to specific layer (not Everything)
? Input System set to "Old" or "Both"
? All Product prefabs assigned
? Scene in Build Settings
? No console errors in Editor play mode
```

## Emergency Fallback

If nothing works, try this minimal test:
```csharp
// Create new script: BuildingPlacementTest.cs
using UnityEngine;

public class BuildingPlacementTest : MonoBehaviour {
    void Update() {
     // Test 1: Click detection
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("CLICK OK");
        }
   
        // Test 2: Camera
        if (Camera.main != null) {
   Debug.Log($"Camera OK: {Camera.main.name}");
        } else {
      Debug.LogError("Camera NULL!");
        }
        
        // Test 3: Raycast
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
  RaycastHit hit;
   if (Physics.Raycast(ray, out hit, 1000f)) {
Debug.Log($"Raycast OK: Hit {hit.collider.name} at {hit.point}");
    }
    }
}

// Attach to any GameObject in scene
// Build and check logs
// All three should work!
```

## Success Criteria

Working build shows these logs:
```
BuildingPlacement: Using camera 'Main Camera'
BuildingPlacement: Initialized successfully
Started placing EnergyBlock. Use mouse to position...
[On click] Found valid position at (X, Y, Z)
? Placed EnergyBlock at (X, Y, Z)
```

## Contact Points for Issues

If still not working after all fixes:
1. Check Player.log (most important!)
2. Test in Editor with "Maximize on Play" (simulates build)
3. Enable Development Build for debugging
4. Add debug overlay (code above)
5. Test minimal scene (just camera + ground + BuildingPlacement)
