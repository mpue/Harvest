# 🎯 Resource Bar UI - Complete Guide

## Übersicht

Ein vollautomatisches UI-System zur Anzeige von Gold und Energie am oberen Bildschirmrand.

### Features:
- ✅ **Gold-Anzeige** mit Icon
- ✅ **Energie-Anzeige** mit Text und Fortschrittsbalken
- ✅ **Automatische Updates** bei Ressourcen-Änderungen
- ✅ **Animierte Wert-Änderungen** (smooth transitions)
- ✅ **Farb-Kodierung** für Energie-Level (Grün/Gelb/Rot)
- ✅ **Tooltips** (optional)
- ✅ **One-Click Setup** mit Editor-Tool

## ⚡ Quick Setup (30 Sekunden):

### Option 1: Auto-Setup Tool (Empfohlen)

```
1. Tools > RTS > Create Resource Bar UI

2. Settings prüfen:
   ✓ Use TextMeshPro: Ja (bessere Qualität)
   ✓ Include Icons: Ja
   ✓ Include Energy Bar: Ja
   ✓ Bar Height: 50

3. Click "🎯 Create Resource Bar"

4. Fertig! ✓
```

### Option 2: Manuelles Setup

Siehe "Manual Setup" unten.

## 🎨 Was wird erstellt:

```
Canvas
└─ ResourceBar
   ├─ ResourceBarUI (Component)
└─ Content
      ├─ GoldSection
      │  ├─ GoldIcon (💰)
    │  └─ GoldText (500)
      └─ EnergySection
         ├─ TopRow
      │  ├─ EnergyIcon (⚡)
         │  └─ EnergyText (15/30)
  └─ EnergyBarBackground
          └─ EnergyBarFill (████████░░)
```

## 🎯 Layout am Bildschirm:

```
┌──────────────────────────────────────────────┐
│ 💰 500  ⚡ 15/30           │
│          ████████░░   │
├──────────────────────────────────────────────┤
│      │
│         [Spiel-Bereich]        │
│  │
││
└──────────────────────────────────────────────┘
```

## 🔧 ResourceBarUI Component:

### Inspector-Einstellungen:

```
ResourceBarUI Component:

Resource Manager:
  Resource Manager: [Auto-assigned]

Gold Display:
  Gold Text: [GoldText]
  Gold Icon: [GoldIcon]

Energy Display:
  Energy Text: [EnergyText]
  Energy Icon: [EnergyIcon]
  Energy Fill Bar: [EnergyBarFill]

Display Settings:
  Show Gold: ✓
  Show Energy: ✓
  Gold Format: "{0}"           → "500"
  Energy Format: "{0}/{1}"     → "15/30"

Energy Bar Colors:
  Energy Full Color: Green
  Energy Mid Color: Yellow
  Energy Low Color: Red
  Low Energy Threshold: 0.2    (20%)
  Mid Energy Threshold: 0.5    (50%)

Animation:
Animate Changes: ✓
  Animation Duration: 0.3s

Tooltips (Optional):
  Show Tooltips: ✓
  Gold Tooltip: "Gold: Used for..."
  Energy Tooltip: "Energy: Required to..."
```

## 🎨 Customization:

### Format Strings:

```
Gold Format:
  "{0}"   → "500"
  "{0} Gold"      → "500 Gold"
  "💰 {0}"   → "💰 500"
  "{0:N0}"        → "1,500" (mit Tausender-Trennung)

Energy Format:
  "{0}/{1}"       → "15/30"
  "{0} / {1}"     → "15 / 30"
  "⚡{0}/{1}"      → "⚡15/30"
  "{0} of {1}"    → "15 of 30"
```

### Farb-Konfiguration:

```
Energie-Balken Farben:

Vollständig (>50%):  Grün   (0, 255, 0)
Mittel (20-50%):     Gelb   (255, 255, 0)
Niedrig (<20%):    Rot    (255, 0, 0)

Anpassbar über:
  Energy Full Color
  Energy Mid Color
  Energy Low Color
  + Thresholds
```

### Position & Größe:

```
ResourceBar RectTransform:
  Anchor: Top (0, 1) - (1, 1)
  Position: (0, 0)
  Size Delta: (0, 50)  ← Höhe anpassbar

Ändern:
  Bar Height: 30-100 (im Setup Tool)
  oder manuell Size Delta.y ändern
```

### Icons:

```
Standard:
  Gold Icon: Gelbes Quadrat
  Energy Icon: Cyan Quadrat

Custom Icons:
1. Erstellen Sie Icon Sprites (32x32 empfohlen)
2. GoldIcon > Image > Sprite: [Ihr Icon]
3. EnergyIcon > Image > Sprite: [Ihr Icon]

Empfohlene Icons:
  Gold: Münze, Goldbarren, Schatz
  Energy: Blitz, Batterie, Energiezelle
```

## 🎭 Animation:

### Standard-Verhalten:

```
Wert ändert sich:
  → 0.3s smooth transition
  → Zahl zählt hoch/runter
  → Balken füllt/leert sich
  → Farbe ändert sich sanft

Deaktivieren:
  Animate Changes: ☐
  → Sofortige Updates
```

### Custom Animation Duration:

```
Schneller:  0.1 - 0.2s (snappy)
Standard:   0.3s (angenehm)
Langsamer:  0.5 - 1.0s (dramatisch)

Ändern:
  Animation Duration: [Sekunden]
```

## 📊 Energie-Balken Details:

### Farb-Logik:

```csharp
Available Energy = MaxEnergy - CurrentEnergy
Fill Amount = Available / Max

Wenn Fill <= 20%:→ Rot    (Kritisch!)
Wenn Fill <= 50%:  → Gelb   (Vorsicht)
Wenn Fill > 50%:   → Grün   (OK)
```

### Visuelles Feedback:

```
Energie VOLL (30/30):
  Text: "30/30"
  Balken: ██████████ (100%, Grün)

Energie MITTEL (15/30):
  Text: "15/30"
  Balken: █████░░░░░ (50%, Gelb)

Energie NIEDRIG (5/30):
  Text: "5/30"
  Balken: █░░░░░░░░░ (17%, Rot)
```

## 🔌 Integration:

### Mit ResourceManager:

```csharp
// ResourceBarUI findet automatisch ResourceManager
// Falls mehrere vorhanden:

ResourceBarUI resourceBar = GetComponent<ResourceBarUI>();
resourceBar.SetResourceManager(myResourceManager);
```

### Events werden automatisch verbunden:

```csharp
resourceManager.OnResourcesChanged += OnResourcesChanged;
resourceManager.OnEnergyChanged += OnEnergyChanged;
```

### Manually trigger update (falls nötig):

```csharp
// ResourceManager ändert Werte
resourceManager.AddResources(100, 0, 0, 50); // +100 Food, +50 Gold
// → ResourceBarUI updated automatisch
```

## 🎮 Runtime Control:

### Visibility:

```csharp
ResourceBarUI bar = GetComponent<ResourceBarUI>();

// Verstecke Gold
bar.showGold = false;

// Verstecke Energie
bar.showEnergy = false;

// Zeige alles
bar.showGold = true;
bar.showEnergy = true;
```

### Format ändern:

```csharp
// Zur Laufzeit ändern
bar.goldFormat = "💰 {0}";
bar.energyFormat = "⚡ {0}/{1}";
```

## 🔧 Troubleshooting:

### Problem: Bar zeigt nicht an

**Prüfen:**
```
1. Canvas vorhanden und aktiv?
   Hierarchy > Canvas > Active: ✓

2. ResourceManager vorhanden?
   Hierarchy > ResourceManager?

3. ResourceBarUI Component aktiv?
   ResourceBar > ResourceBarUI > Enabled: ✓

4. Werte zugewiesen?
   ResourceBarUI > Gold Text: [nicht null]
   ResourceBarUI > Energy Text: [nicht null]
```

### Problem: Werte aktualisieren nicht

**Prüfen:**
```
1. ResourceManager korrekt zugewiesen?
   ResourceBarUI > Resource Manager: [zugewiesen]

2. Events funktionieren?
   Console: Keine Errors?

3. ResourceManager ändert Werte?
   ResourceManager.Gold = 500;
   → Bar sollte updaten
```

### Problem: Animation funktioniert nicht

**Prüfen:**
```
1. Animate Changes: ✓ (angehakt)

2. Animation Duration > 0
   Animation Duration: 0.3

3. Time.timeScale != 0
   (Pause pausiert auch Animationen!)
```

### Problem: Farben falsch

**Prüfen:**
```
1. Energy Fill Bar: [zugewiesen]

2. Thresholds sinnvoll:
 Low: 0.2 (20%)
   Mid: 0.5 (50%)

3. Farben eingestellt:
   Full: Grün
   Mid: Gelb
   Low: Rot
```

### Problem: Icons fehlen

**Lösung:**
```
1. Include Icons war ausgeschaltet
   → Re-create mit Tool, Icons: ✓

2. Oder manuell:
   GoldIcon > Image > Sprite: [Zuweisen]
   EnergyIcon > Image > Sprite: [Zuweisen]
```

## 🎨 Styling Guide:

### Minimalistisch:

```
ResourceBar Background:
  Color: Transparent oder sehr dunkel (0,0,0,0.3)

Text:
  Color: White
  Font: Clean, modern

Icons:
  Simple, monochrome
  Size: 25-30px
```

### Farbig/Thematisch:

```
Gold:
  Icon: Golden (255, 215, 0)
  Text: Golden oder White

Energy:
  Icon: Cyan (0, 255, 255)
  Text: Cyan oder White

Background:
  Thematisch zur Spielwelt
```

### Sci-Fi:

```
Background:
  Dark blue-grey (30, 40, 60, 0.9)
  Add Outline component

Text:
  Cyan or Green
  Glow effect (Outline/Shadow)

Icons:
  Glowing
  Angular shapes
```

### Fantasy:

```
Background:
  Brown/Wood texture (100, 60, 30, 0.9)

Text:
  Golden for gold
  Blue/Purple for energy
  Font: Medieval style

Icons:
  Coin for gold
  Mana crystal for energy
```

## 📦 Extensions:

### Food, Wood, Stone hinzufügen:

```csharp
// In ResourceBarUI.cs erweitern:

[Header("Additional Resources")]
[SerializeField] private TextMeshProUGUI foodText;
[SerializeField] private TextMeshProUGUI woodText;
[SerializeField] private TextMeshProUGUI stoneText;

// In OnResourcesChanged:
if (foodText != null) foodText.text = food.ToString();
if (woodText != null) woodText.text = wood.ToString();
if (stoneText != null) stoneText.text = stone.ToString();
```

### Tooltips erweitern:

```csharp
// Custom Tooltip System anbinden:
using YourTooltipNamespace;

private void AddTooltip(GameObject obj, string text)
{
    YourTooltip tooltip = obj.AddComponent<YourTooltip>();
    tooltip.text = text;
}
```

### Click Events:

```csharp
// Button auf Gold-Bereich:
Button goldButton = goldText.gameObject.AddComponent<Button>();
goldButton.onClick.AddListener(() => {
    Debug.Log("Gold clicked!");
    // Öffne Shop, Statistiken, etc.
});
```

## 🎓 Best Practices:

### 1. Immer ResourceManager verwenden
```
✓ Events automatisch
✓ Zentrale Verwaltung
✓ Einfaches Update
```

### 2. Animation aktivieren
```
✓ Professioneller Look
✓ Besseres Feedback
✓ Minimaler Performance-Overhead
```

### 3. Icons verwenden
```
✓ Schnelle visuelle Erkennung
✓ Sprachunabhängig
✓ Platzsparend
```

### 4. Farb-Kodierung für Energie
```
✓ Sofort erkennbarer Status
✓ Warnung bei niedrig
✓ Standard in vielen Games
```

### 5. Tooltips für neue Spieler
```
✓ Erklärt Ressourcen
✓ Hilfreich für Anfänger
✓ Kann später ausgeblendet werden
```

## 💡 Pro-Tips:

### Tip 1: Counter-Animation
```
Große Änderungen:
  → Animation Duration: 0.5s
  → Spektakulärer

Kleine Änderungen:
  → Animation Duration: 0.2s
  → Snappier
```

### Tip 2: Sound Effects
```
Bei Ressourcen-Änderung:
  Gold +100 → "Coin" Sound
  Energie niedrig → "Warning" Sound
```

### Tip 3: Pulse-Effekt bei kritisch
```
Energie < 20%:
  → Roter Text pulsiert
  → Mehr Aufmerksamkeit
```

### Tip 4: Notification bei Änderung
```
Gold +500:
  → "+500" fliegt nach oben
  → Positives Feedback
```

### Tip 5: Keyboard Shortcut
```
Taste 'R':
  → Toggle Resource Bar Visibility
  → Für Screenshots
```

## 🎯 Zusammenfassung:

| Feature | Status | Priorität |
|---------|--------|-----------|
| Gold Display | ✅ | ⭐⭐⭐ Essentiell |
| Energy Display | ✅ | ⭐⭐⭐ Essentiell |
| Energy Bar | ✅ | ⭐⭐ Wichtig |
| Icons | ✅ | ⭐⭐ Wichtig |
| Animation | ✅ | ⭐ Nice-to-have |
| Tooltips | ✅ | ⭐ Nice-to-have |
| Auto-Setup | ✅ | ⭐⭐⭐ Essentiell |

---

**Quick Start:**
```
1. Tools > RTS > Create Resource Bar UI
2. Click "Create Resource Bar"
3. Play & Enjoy! 🎮
```

**Position:** Oben am Bildschirm ⬆️  
**Update:** Automatisch 🔄  
**Style:** Anpassbar 🎨
