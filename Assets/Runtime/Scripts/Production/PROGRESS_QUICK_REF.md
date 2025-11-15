# Progress Display - Quick Reference

## ?? Features auf einen Blick

| Feature | Was es macht | Wann aktiv |
|---------|-------------|------------|
| **Progress Bar** | Horizontaler Balken | Progress > 0% |
| **Progress Text** | "75% (15s)" | Progress > 0% |
| **Color Lerp** | Gelb ? Grün | Progress 0-100% |
| **Glow Pulse** | Leuchteffekt pulsiert | isProducing == true |
| **Overlay Dimmer** | Bild abdunkeln | Progress > 0% |
| **Rotation Indicator** | Drehendes Icon | isProducing == true |

## ?? Standard-Einstellungen

```ini
[Animation]
Animate Progress = true
Glow Pulse Speed = 2.0 Hz

[Colors]
Progress Start = Yellow (1, 1, 0)
Progress End   = Green (0, 1, 0)

[Timings]
Rotation Speed = 180° / second
Pulse Frequency = 2 Hz
```

## ?? Progress Calculation

```csharp
// Progress (0.0 - 1.0)
float progress = currentProductionProgress;

// Percentage
int percent = Mathf.RoundToInt(progress * 100);

// Remaining Time
float remaining = duration * (1f - progress);
int seconds = Mathf.CeilToInt(remaining);

// Display
string text = $"{percent}% ({seconds}s)";
```

## ?? Inspector Setup

```
ProductionSlot Component
??? Enhanced Progress Display
    ??? Progress Text        ? TextMeshProUGUI
    ??? Progress Glow        ? Image
    ??? Overlay Dimmer       ? Image
    ??? Production Indicator ? GameObject
```

## ?? Visual States

| Progress | Bar Fill | Color | Dimmer | Animation |
|----------|----------|-------|--------|-----------|
| 0%       | Empty    | -     | 0%     | None      |
| 25%      | ????     | Yellow| 10%    | Active    |
| 50%      | ????     | Y-G   | 15%    | Active    |
| 75% | ??????   | Green | 20%    | Active    |
| 100%     | ???????? | Green | 30%    | Active    |

## ?? Code Usage

### Enable/Disable Progress
```csharp
// Show progress
slot.UpdateProgress(0.5f); // 50%

// Hide progress
slot.UpdateProgress(0f);
```

### Get Current State
```csharp
bool isProducing = slot.IsProducing;
float progress = slot.CurrentProgress;
```

## ?? Animation Formulas

### Glow Pulse
```csharp
pulse = (Sin(time * speed) + 1) * 0.5
alpha = 0.3 + (pulse * 0.3)
// Range: 0.3 - 0.6
```

### Rotation
```csharp
angle -= 180 * deltaTime
// Half rotation per second
```

### Color Lerp
```csharp
color = Lerp(start, end, progress)
// Smooth transition
```

## ?? Debugging

```csharp
// Log progress updates
Debug.Log($"Progress: {progress:P0}");

// Check references
if (progressText == null) 
    Debug.LogError("Progress Text not assigned!");

// Verify animation state
if (!animateProgress)
    Debug.LogWarning("Animations disabled!");
```

## ?? Customization Examples

### Change Colors
```csharp
// In Inspector:
Progress Color Start = Blue
Progress Color End = Red
```

### Change Animation Speed
```csharp
// Faster pulse
Glow Pulse Speed = 4.0

// Slower pulse
Glow Pulse Speed = 1.0
```

### Disable Animations
```csharp
// Turn off all animations
Animate Progress = false
```

## ?? Prefab Structure (Quick)

```
ProductSlot
??? Background
??? OverlayDimmer
??? ProductImage
?   ??? ProductionIndicator
??? Texts (Name, Cost, Duration)
??? ProgressBar
    ??? Background
    ??? ProgressFill
    ??? ProgressGlow
    ??? ProgressText
```

## ? Performance Tips

? Animations nur bei `isProducing`
? SetActive statt Alpha = 0
? Keine String-Allocation im Update
? Cached Referenzen

## ?? Troubleshooting Checklist

- [ ] All references assigned in Inspector?
- [ ] TextMeshPro package imported?
- [ ] Canvas in scene?
- [ ] Animate Progress enabled?
- [ ] Progress value between 0-1?
- [ ] ProductionPanel calling UpdateProgress()?

## ?? Common Use Cases

### Case 1: Single Unit Production
```csharp
slot.UpdateProgress(component.CurrentProductionProgress);
```

### Case 2: Queue Display
```csharp
if (isFirstInQueue && isProducing)
    slot.UpdateProgress(progress);
else
    slot.UpdateProgress(0f);
```

### Case 3: Custom Progress
```csharp
float custom = (Time.time % 10f) / 10f;
slot.UpdateProgress(custom);
```

## ?? Related Files

- `ProductionSlot.cs` - Main logic
- `ProductionPanel.cs` - Updates progress
- `ProductionComponent.cs` - Progress calculation
- `PROGRESS_DISPLAY.md` - Full documentation
- `PROGRESS_STATES_VISUAL.md` - Visual guide

---

**Quick Access:** Tools ? RTS ? Production System Setup
