# RTS Kamera & Bewegungssteuerung - Konfliktlösung

## Problem
Rechtsklick wird für zwei Funktionen verwendet:
- ?? **Kamera Free-Look** (Rotation)
- ?? **Unit Bewegung** (Movement Command)

## Implementierte Lösung: Smart Control Mode

### 3 Control Modes verfügbar:

#### 1. **Smart Mode** (Standard - Empfohlen für RTS)
```
Keine Units selektiert  ? Rechtsklick = Kamera Rotation
Units selektiert  ? Rechtsklick = Units bewegen
```

**Vorteile:**
- ? Intuitiv und kontextabhängig
- ? Keine zusätzlichen Tasten nötig
- ? Funktioniert wie moderne RTS-Games

**Verwendung:**
- Im RTSCamera Inspector: `Control Mode` ? **Smart**
- Funktioniert automatisch!

#### 2. **Modifier Key Mode** (Für Fortgeschrittene)
```
Rechtsklick    ? Units bewegen
Alt + Rechtsklick        ? Kamera Rotation
```

**Vorteile:**
- ? Volle Kontrolle
- ? Kamera und Unit-Befehle jederzeit verfügbar
- ? Ähnlich wie Unity Editor

**Verwendung:**
- Im RTSCamera Inspector: `Control Mode` ? **Modifier Key**
- Halten Sie Alt + Rechtsklick für Kamera-Rotation

#### 3. **Always Free-Look Mode** (Für Shooter-Style)
```
Rechtsklick      ? Kamera Rotation (immer)
Units bewegen         ? Andere Taste (z.B. M + Klick)
```

**Vorteile:**
- ? Kamera-Kontrolle immer verfügbar
- ? Gut für Action-RTS

**Nachteil:**
- ?? Unit-Bewegung benötigt andere Taste (erfordert Code-Änderung)

**Verwendung:**
- Im RTSCamera Inspector: `Control Mode` ? **Always Free Look**

---

## Setup & Konfiguration

### RTSCamera Component:

```
[Control Mode]
??? Smart           ? Empfohlen für Standard-RTS
??? Modifier Key    ? Für präzise Kontrolle
??? Always FreeLook ? Für Action-RTS
```

### Inspector Settings:

| Setting | Beschreibung | Empfohlener Wert |
|---------|--------------|------------------|
| **Control Mode** | Wie Rechtsklick funktioniert | Smart |
| **Free Look Key** | Taste für Free-Look | Mouse1 (Rechtsklick) |
| **Mouse Sensitivity** | Rotations-Geschwindigkeit | 3 |

---

## Alternative Lösungen (nicht implementiert)

### Option A: Mittlere Maustaste für Kamera
```
Linksklick       ? Selektion
Rechtsklick    ? Unit Bewegung
Mittlere Maustaste   ? Kamera Rotation
```
**Pro:** Klare Trennung  
**Contra:** Mittlere Maustaste bereits für Pan verwendet

### Option B: Edge Scrolling + Rotation Key
```
Linksklick         ? Selektion
Rechtsklick        ? Unit Bewegung
Mausrand       ? Kamera Bewegung
Q/E oder R      ? Kamera Rotation
```
**Pro:** Kein Konflikt  
**Contra:** Weniger direkte Kontrolle

### Option C: Kamera-Toggle Modus
```
Linksklick  ? Selektion
Rechtsklick        ? Unit Bewegung (Standard)
Leertaste halten   ? Kamera-Modus aktivieren
  ? Dann: Rechtsklick ? Kamera Rotation
```
**Pro:** Flexible Modi  
**Contra:** Komplexer für Spieler

---

## Steuerung Übersicht (Smart Mode)

### Ohne selektierte Units:
| Eingabe | Aktion |
|---------|--------|
| **Linksklick** | Einheit auswählen |
| **Rechtsklick + Maus** | ?? Kamera drehen (Free-Look) |
| **Mittlere Maustaste** | Kamera Pan |
| **WASD** | Kamera bewegen |
| **Q/E** | Kamera hoch/runter |

### Mit selektierten Units:
| Eingabe | Aktion |
|---------|--------|
| **Linksklick** | Neue Einheit auswählen |
| **Shift + Linksklick** | Zur Auswahl hinzufügen |
| **Rechtsklick auf Boden** | ?? Units bewegen |
| **Mittlere Maustaste** | Kamera Pan |
| **WASD** | Kamera bewegen |
| **ESC** | Alle abwählen |

---

## Code-Anpassungen für eigene Lösungen

### Beispiel: Andere Taste für Unit-Bewegung

```csharp
// In UnitSelector.cs, ersetzen Sie in HandleMouseInput():

// Statt:
if (Input.GetMouseButtonDown(1))

// Verwenden Sie:
if (Input.GetKeyDown(KeyCode.M) && Input.GetMouseButton(0))
// M-Taste + Linksklick für Bewegung
```

### Beispiel: Kamera mit Space + Rechtsklick

```csharp
// In RTSCamera.cs, in HandleFreeLook():

case CameraControlMode.ModifierKey:
    // Ändern Sie:
    shouldEnableFreeLook = Input.GetMouseButton(1) && Input.GetKey(KeyCode.Space);
    // Space + Rechtsklick
    break;
```

### Beispiel: F-Taste für Free-Look Toggle

```csharp
// In RTSCamera.cs, fügen Sie hinzu:

private bool freeLookToggled = false;

void Update()
{
    if (Input.GetKeyDown(KeyCode.F))
    {
        freeLookToggled = !freeLookToggled;
    }
    
    // Dann in HandleFreeLook():
    shouldEnableFreeLook = freeLookToggled && Input.GetMouseButton(1);
}
```

---

## Empfehlung nach Spiel-Genre

### Standard RTS (Starcraft, Age of Empires Style):
```
? Smart Mode
   - Rechtsklick für Units wenn selektiert
   - Rechtsklick für Kamera wenn nichts selektiert
   
Alternative:
? Modifier Key Mode (Alt + Rechtsklick)
   - Professioneller, mehr Kontrolle
```

### Action RTS (Dawn of War, Company of Heroes Style):
```
? Smart Mode + Edge Scrolling
   - Dynamische Kamera
   - Schnelle Unit-Befehle
```

### MOBA Style (League of Legends, Dota):
```
?? Custom Solution:
   - Rechtsklick nur für Bewegung
   - Kamera: WASD oder Mouse Drag
   - Keine Free-Look Rotation
```

### City Builder / Simulation:
```
? Always Free-Look Mode
   - Kamera-Inspektion ist wichtiger
- Units weniger zentral
```

---

## Debug & Testing

### Testen Sie den Smart Mode:

1. **Start Play Mode**
2. **Keine Units selektiert:**
   - Rechtsklick halten ? Kamera sollte rotieren
   - Cursor sollte verschwinden
   
3. **Unit selektieren:**
   - Linksklick auf Unit
   - Rechtsklick auf Boden ? Unit sollte sich bewegen
   - Cursor bleibt sichtbar
   
4. **Units abwählen (ESC):**
   - Rechtsklick sollte wieder Kamera rotieren

### Console Logging hinzufügen:

```csharp
// In RTSCamera.cs, in HandleFreeLook():
if (shouldEnableFreeLook && !isFreeLook)
{
    Debug.Log("Free-Look aktiviert");
}

// In UnitSelector.cs, in HandleMoveCommand():
Debug.Log($"Bewegungsbefehl für {selectedUnits.Count} Units");
```

---

## Performance Hinweise

Die Smart Mode Logik hat **minimal Performance Impact**:
- ? Nur eine Referenz-Check pro Frame
- ? Kein zusätzlicher Raycast
- ? Keine zusätzlichen Berechnungen

```csharp
// Sehr effizient:
bool cameraInFreeLook = rtsCamera != null && rtsCamera.IsInFreeLookMode();
```

---

## FAQ

**Q: Kann ich die Taste für Free-Look ändern?**  
A: Ja! Im RTSCamera Inspector ? `Free Look Key` (derzeit nicht voll implementiert, aber vorbereitet)

**Q: Was wenn ich Edge Scrolling will statt WASD?**  
A: Sie können in RTSCamera.cs die HandleWASDMovement() Methode erweitern:

```csharp
void HandleEdgeScrolling()
{
    float edgeSize = 20f;
    Vector3 move = Vector3.zero;
    
    if (Input.mousePosition.x < edgeSize)
        move.x = -1;
    if (Input.mousePosition.x > Screen.width - edgeSize)
        move.x = 1;
    if (Input.mousePosition.y < edgeSize)
        move.z = -1;
    if (Input.mousePosition.y > Screen.height - edgeSize)
 move.z = 1;
      
    transform.position += move * moveSpeed * Time.deltaTime;
}
```

**Q: Funktioniert das auch mit Touch/Mobile?**  
A: Derzeit nur Maus/Keyboard. Für Touch benötigen Sie Touch-Input Handling.

**Q: Kann ich Free-Look mit einer Taste statt Rechtsklick?**  
A: Ja! Ändern Sie in RTSCamera ? HandleFreeLook():
```csharp
shouldEnableFreeLook = Input.GetKey(KeyCode.LeftAlt);
```

---

## Fazit

**Die Smart Mode Lösung bietet:**
- ? Keine Konflikte zwischen Kamera und Unit-Befehlen
- ? Intuitive, kontextbasierte Steuerung
- ? Flexibilität durch 3 verschiedene Modi
- ? Einfach im Inspector umschaltbar
- ? Performance-effizient

**Empfehlung:** Verwenden Sie **Smart Mode** für Standard-RTS Gameplay!
