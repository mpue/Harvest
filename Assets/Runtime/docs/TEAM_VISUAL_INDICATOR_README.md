# Team Visual Indicator System

## Übersicht

Das Team Visual Indicator System bietet eine klare, gut sichtbare optische Kennzeichnung für Units basierend auf ihrer Team-Zugehörigkeit. Spieler können so leicht erkennen, welche Units zu ihrem Team gehören und welche zu anderen Teams.

## Features

### Indikator-Typen

1. **Color Ring** (Farb-Ring)
   - Farbiger Ring am Boden der Unit
   - Optionale Rotation und Puls-Effekte
   - Sehr gut sichtbar, besonders bei Gruppen

2. **Shield Icon** (Schild-Icon)
   - Icon über der Unit (Billboard-Effekt)
   - Gut für Units in dichtem Terrain
   - Bleibt immer zur Kamera ausgerichtet

3. **Material Tint** (Material-Färbung)
   - Färbt die Materialien der Unit
   - Subtiler Effekt
   - Kombinierbar mit anderen Indikatoren

4. **Outline** (Umriss)
   - Outline-Effekt um die Unit
   - Benötigt Shader-Unterstützung
   - Sehr deutlich sichtbar

5. **Combined** (Kombiniert)
   - Mehrere Indikatoren gleichzeitig
- Maximale Sichtbarkeit

## Installation

### Automatische Installation (empfohlen)

1. Fügen Sie einem GameObject eine `TeamComponent` hinzu
2. Die `TeamVisualIndicator`-Komponente wird automatisch in `BaseUnit.Awake()` hinzugefügt
3. Konfigurieren Sie die Einstellungen im Inspector

### Manuelle Installation

1. Fügen Sie die `TeamComponent` zu Ihrer Unit hinzu
2. Fügen Sie die `TeamVisualIndicator`-Komponente hinzu
3. Wählen Sie einen Indikator-Typ
4. Passen Sie die Einstellungen an

## Verwendung

### Grundeinrichtung

```csharp
// TeamComponent ist erforderlich
TeamComponent teamComp = gameObject.AddComponent<TeamComponent>();
teamComp.SetTeam(Team.Player);
teamComp.SetTeamColor(Color.blue);

// TeamVisualIndicator wird automatisch von BaseUnit hinzugefügt
// Oder manuell:
TeamVisualIndicator indicator = gameObject.AddComponent<TeamVisualIndicator>();
```

### Team-Farben Konfigurieren

Standard-Farben für Teams:
- **Player**: Blau
- **Enemy**: Rot  
- **Ally**: Grün
- **Neutral**: Grau

```csharp
// Team-Farbe ändern
TeamComponent team = unit.GetComponent<TeamComponent>();
team.SetTeamColor(Color.cyan);

// Indikator aktualisieren
TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
indicator.UpdateTeamColor();
```

### Indikator-Typ zur Laufzeit Ändern

```csharp
TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
indicator.SetIndicatorType(TeamVisualIndicator.IndicatorType.ColorRing);
```

### Indikator Ein-/Ausblenden

```csharp
TeamVisualIndicator indicator = unit.GetComponent<TeamVisualIndicator>();
indicator.SetIndicatorVisible(false); // Verstecken
indicator.SetIndicatorVisible(true);  // Anzeigen
```

## Konfiguration

### Color Ring Einstellungen

- **Ring Rotation Speed**: Rotationsgeschwindigkeit des Rings
- **Pulse Effect**: Aktiviert Puls-Effekt
- **Pulse Speed**: Geschwindigkeit des Pulsierens
- **Pulse Amount**: Stärke des Pulsierens
- **Color Ring Prefab**: Optionales Custom Prefab für den Ring

### Shield Icon Einstellungen

- **Billboard Icon**: Icon rotiert immer zur Kamera
- **Shield Icon Prefab**: Optionales Custom Prefab für das Icon
- **Indicator Height Offset**: Höhe über der Unit

### Material Tint Einstellungen

- **Tint Materials**: Aktiviert Material-Färbung
- **Tint Strength**: Stärke der Färbung (0-1)
- **Material Property Names**: Shader-Properties die gefärbt werden

### Outline Einstellungen

- **Use Outline**: Aktiviert Outline-Effekt
- **Outline Width**: Breite des Outlines
- **Erfordert Shader mit Outline-Support**

## Best Practices

### Empfohlene Einstellungen für verschiedene Spielstile

#### RTS (Real-Time Strategy)
```
Indikator-Typ: Color Ring + Material Tint (Combined)
Ring Rotation: 30
Pulse Effect: Aktiviert
Tint Strength: 0.3
```

#### MOBA
```
Indikator-Typ: Color Ring
Ring Rotation: 0 (statisch)
Pulse Effect: Deaktiviert
Größerer Indicator Scale: 1.5
```

#### Top-Down Action
```
Indikator-Typ: Outline + Shield Icon
Billboard: Aktiviert
Outline Width: 0.05
```

## Performance

- **Color Ring**: Sehr performant (1 Draw Call pro Unit)
- **Shield Icon**: Performant, Billboard kostet minimal
- **Material Tint**: Sehr performant (MaterialPropertyBlock)
- **Outline**: Abhängig vom Shader
- **Combined**: Moderate Performance-Auswirkung

## Troubleshooting

### Problem: Indikator wird nicht angezeigt

**Lösung:**
1. Prüfen Sie, ob `TeamComponent` vorhanden ist
2. Prüfen Sie, ob `Show Indicator` aktiviert ist
3. Prüfen Sie die `Indicator Scale` (zu klein?)
4. Prüfen Sie die Layer-Einstellungen der Kamera

### Problem: Farben sind falsch

**Lösung:**
1. Prüfen Sie die Team-Farbe in `TeamComponent`
2. Rufen Sie `UpdateTeamColor()` nach Farbänderungen auf
3. Prüfen Sie `Tint Strength` bei Material Tint

### Problem: Ring verschwindet im Boden

**Lösung:**
1. Erhöhen Sie die Y-Position leicht (0.01f ? 0.05f)
2. Passen Sie die Terrain/Ground Layer-Einstellungen an
3. Verwenden Sie einen anderen Indikator-Typ

### Problem: Performance-Probleme bei vielen Units

**Lösung:**
1. Verwenden Sie `Color Ring` ohne Pulse
2. Deaktivieren Sie Rotation bei statischen Units
3. Verwenden Sie Material Tint statt zusätzlicher GameObjects
4. Implementieren Sie LOD für entfernte Units

## Integration mit bestehenden Systemen

### Mit Selection System

Das System arbeitet nahtlos mit dem Selection System zusammen:
- Selection Indicator und Team Indicator überlagern sich nicht
- Unterschiedliche Farben für Selection (grün) und Team
- Team-Indikator bleibt während Selection sichtbar

### Mit Production System

Units die produziert werden erhalten automatisch:
1. Die TeamComponent des produzierenden Gebäudes
2. Den entsprechenden TeamVisualIndicator
3. Die korrekte Team-Farbe

```csharp
// In ProductionComponent.CompleteProduction()
GameObject spawnedUnit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
// Copy team from producer
TeamComponent producerTeam = GetComponent<TeamComponent>();
TeamComponent spawnedTeam = spawnedUnit.GetComponent<TeamComponent>();
if (spawnedTeam != null && producerTeam != null)
{
    spawnedTeam.SetTeam(producerTeam.CurrentTeam);
    spawnedTeam.SetTeamColor(producerTeam.TeamColor);
}
```

## Custom Prefabs

### Eigenen Ring Erstellen

1. Erstellen Sie ein Prefab mit einem MeshRenderer
2. Material sollte Unlit oder Emissive sein
3. Weisen Sie das Prefab im Inspector zu
4. Das System färbt es automatisch mit der Team-Farbe

### Eigenes Icon Erstellen

1. Erstellen Sie ein Quad mit Ihrer Icon-Textur
2. Verwenden Sie einen Unlit Shader
3. Weisen Sie das Prefab im Inspector zu
4. Aktivieren Sie Billboard für beste Sichtbarkeit

## Erweiterungen

### Eigene Indikator-Typen

```csharp
// In TeamVisualIndicator.cs erweitern:
public enum IndicatorType
{
  ColorRing,
    ShieldIcon,
    MaterialTint,
    Outline,
    Combined,
    Custom // Ihr eigener Typ
}

// Dann in CreateVisualIndicator():
case IndicatorType.Custom:
    CreateCustomIndicator();
    break;
```

### Animations-Integration

```csharp
// Indikator bei bestimmten Aktionen hervorheben
public void HighlightUnit(float duration)
{
    StartCoroutine(HighlightCoroutine(duration));
}

private IEnumerator HighlightCoroutine(float duration)
{
    float originalScale = indicatorScale;
    indicatorScale *= 1.5f;
    
    yield return new WaitForSeconds(duration);
    
    indicatorScale = originalScale;
}
```

## Siehe auch

- `TeamComponent.cs` - Team-Zugehörigkeits-System
- `BaseUnit.cs` - Basis-Unit-Klasse mit Team-Integration
- `ProductionComponent.cs` - Unit-Produktion mit Team-Vererbung
