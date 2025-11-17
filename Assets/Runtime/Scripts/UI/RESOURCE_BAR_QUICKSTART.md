# ?? Resource Bar - Quick Start

## ? 30-Sekunden Setup:

```
1. Tools > RTS > Create Resource Bar UI

2. [Settings OK lassen]

3. Click "?? Create Resource Bar"

4. FERTIG! ?
```

## ?? Was Sie sehen:

```
Oben am Bildschirm:
??????????????????????????????????
? ?? 500? 15/30      ?
?          ??????????   ?
??????????????????????????????????
```

## ?? Updates automatisch:

```csharp
// Ihre Code:
resourceManager.AddGold(100);
// ? Bar zeigt sofort: ?? 600

resourceManager.ConsumeEnergy(10);
// ? Bar zeigt: ? 5/30
// ? Balken wird rot (wenig Energie!)
```

## ?? Anpassen:

### Position ändern:
```
ResourceBar > RectTransform:
  Position Y: -50 (weiter unten)
  Size Delta Y: 60 (höher)
```

### Farben ändern:
```
ResourceBar > ResourceBarUI:
  Energy Full Color: Grün
  Energy Mid Color: Gelb
  Energy Low Color: Rot
```

### Format ändern:
```
ResourceBar > ResourceBarUI:
Gold Format: "?? {0}"  ? "?? 500"
  Energy Format: "{0}/{1}" ? "15/30"
```

### Icons ändern:
```
1. Erstelle Sprite (32x32)
2. GoldIcon > Image > Sprite: [Dein Icon]
3. EnergyIcon > Image > Sprite: [Dein Icon]
```

## ?? Features:

? Zeigt Gold  
? Zeigt Energie (Verfügbar/Max)  
? Fortschrittsbalken für Energie  
? Farb-Kodierung:  
   - Grün = Viel Energie (>50%)  
   - Gelb = Mittel (20-50%)  
   - Rot = Wenig (<20%)  
? Animierte Wert-Änderungen  
? Auto-Update bei Ressourcen-Änderung  

## ?? Wenn's nicht funktioniert:

### Bar erscheint nicht:
```
1. Canvas vorhanden?
   ? Tools > RTS > Create Resource Bar UI
   ? Erstellt automatisch

2. ResourceManager vorhanden?
   ? Wird automatisch erstellt
```

### Werte aktualisieren nicht:
```
ResourceBar auswählen:
  ResourceBarUI > Resource Manager: [Zugewiesen?]
  
Falls leer:
  ? Drag ResourceManager GameObject hier rein
```

### Bar an falscher Position:
```
ResourceBar > RectTransform:
  Anchors: Top (0,1) - (1,1)
  Position: (0, 0)
```

## ?? Testing:

```
1. Play Mode

2. Console ausführen:
   GameObject.Find("ResourceManager")
     .GetComponent<ResourceManager>()
     .AddGold(100);
     
3. Sehen Sie Gold-Anzeige updaten? ?

4. Energie verbrauchen:
   resourceManager.ConsumeEnergy(20);
   
5. Balken wird gelb/rot? ?
```

## ?? Anzeige-Logik:

```
Gold:
  Zeigt: Aktueller Betrag
  Format: "500"

Energie:
  Zeigt: Verfügbar / Maximum
  Format: "15/30"
  
  Verfügbar = Max - Verbraucht
  
  Wenn Building 5 Energie braucht:
  30 ? 25 verfügbar
  Anzeige: "25/30"
```

## ?? Empfohlene Anpassungen:

### Minimal:
```
? Tool verwenden
? Standard-Settings
? Fertig!
```

### Schön:
```
? Custom Icons (Münze, Blitz)
? Bar Height: 60
? Farben anpassen
```

### Professionell:
```
? Custom Icons
? Glow/Outline Effekte
? Sound bei Änderung
? Animation Duration: 0.2s (schneller)
? Thematisches Styling
```

## ?? Next Steps:

Nach Setup:
1. Icons erstellen/zuweisen
2. Farben an Spiel-Theme anpassen
3. Testen im Play Mode
4. Eventuell weitere Ressourcen hinzufügen (Food, Wood)

---

**Das war's!** Die Resource Bar ist **production-ready**! ??
