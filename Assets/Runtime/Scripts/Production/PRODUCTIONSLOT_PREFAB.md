# ProductionSlot Prefab - Aufbau und Struktur

## Hierarchie im Unity Editor

```
ProductSlot (RectTransform + ProductionSlot.cs + Button)
??? Background (Image) - Hintergrund des Slots
??? ProductImage (Image) - Icon/Bild der Einheit
??? ProductName (TextMeshProUGUI) - Name der Einheit
??? CostText (TextMeshProUGUI) - Kosten (z.B. "100 Gold 50 Food")
??? DurationText (TextMeshProUGUI) - Produktionsdauer (z.B. "30s")
??? ProgressBar (GameObject)
    ??? ProgressFill (Image - Type: Filled) - Fortschrittsbalken
```

## Visual Layout (wie es aussieht)

```
???????????????????????
?      ?  ? Background (dunkelgrau)
?   ?????????????     ?
?   ?   ??      ?     ?  ? ProductImage (Icon der Einheit)
?   ?????????????     ?
?     ?
?  ???????????????    ?  ? ProgressBar (nur bei Queue sichtbar)
?         ?
?    Soldat           ?  ? ProductName (weiß)
?              ?
?   100 Gold          ?  ? CostText (gelb)
?    ?
?  30s ?  ? DurationText (cyan)
???????????????????????
   100px x 120px
```

## Komponenten-Details

### 1. ProductSlot (Root GameObject)
- **RectTransform**: 100x120px
- **ProductionSlot.cs**: Logik-Script
- **Button**: Macht den ganzen Slot klickbar

### 2. Background (Image)
- **Anchor**: Stretch (0,0) bis (1,1)
- **Color**: RGBA(0.2, 0.2, 0.2, 0.9) - Dunkelgrau mit Transparenz
- **Funktion**: Visueller Hintergrund + Button Target Graphic

### 3. ProductImage (Image)
- **Anchor**: X: 10%-90%, Y: 50%-90%
- **Color**: Weiß (damit Sprite-Farben durchscheinen)
- **Funktion**: Zeigt das Product.PreviewImage Sprite an

### 4. ProductName (TextMeshProUGUI)
- **Anchor**: X: 5%-95%, Y: 35%-45%
- **Font Size**: 12
- **Alignment**: Center
- **Color**: Weiß
- **Funktion**: Zeigt Product.ProductName an

### 5. CostText (TextMeshProUGUI)
- **Anchor**: X: 5%-95%, Y: 20%-30%
- **Font Size**: 10
- **Alignment**: Center
- **Color**: Gelb
- **Funktion**: Zeigt Ressourcenkosten an (Product.GetCostString())

### 6. DurationText (TextMeshProUGUI)
- **Anchor**: X: 5%-95%, Y: 5%-15%
- **Font Size**: 10
- **Alignment**: Center
- **Color**: Cyan
- **Funktion**: Zeigt Produktionsdauer an (Product.ProductionDuration)

### 7. ProgressBar (GameObject)
- **Anchor**: X: 5%-95%, Y: 48%-52%
- **Initially**: Deaktiviert (SetActive(false))
- **Funktion**: Container für Progress Fill

### 8. ProgressFill (Image)
- **Type**: Filled
- **Fill Method**: Horizontal (von links nach rechts)
- **Color**: Grün
- **Funktion**: Zeigt Produktionsfortschritt (0.0 bis 1.0)

## Wo wird es verwendet?

### Im ProductionPanel

1. **ProductsContainer** (verfügbare Produkte zum Auswählen):
 - Instantiiert für jedes `Product` in `ProductionComponent.AvailableProducts`
   - ProgressBar ist versteckt
   - Button ist klickbar ? fügt zur Queue hinzu

2. **QueueContainer** (Produkte in Warteschlange):
   - Instantiiert für jedes Produkt in der Queue
   - ProgressBar ist sichtbar beim ersten Item (aktuelle Produktion)
   - Button ist deaktiviert (keine Funktion in der Queue)

## Automatische Erstellung

Das Prefab kann automatisch erstellt werden:

### Unity Editor:
1. Menü: **Tools ? RTS ? Production System Setup**
2. Button: **"Create Slot Prefabs"**
3. Prefab wird erstellt in: **Assets/Prefabs/UI/ProductSlot.prefab**

### Code:
Der `ProductionSystemSetupWindow` Editor erstellt das Prefab programmatisch mit allen Komponenten und Referenzen.

## Anpassung

Nach der Erstellung kannst du das Prefab anpassen:

### Farben ändern:
- Background farbe
- Text-Farben
- ProgressBar-Farbe

### Layout ändern:
- Positionen der einzelnen Elemente
- Größe des gesamten Slots
- Font-Größen

### Zusätzliche Elemente:
- Hotkey-Anzeige
- Requirement-Icons
- Lock-Overlay (für noch nicht verfügbare Units)

## Wichtig!

**Das ProductionSlot Prefab MUSS ein Canvas als Parent haben** wenn es im Scene View bearbeitet wird. Unity UI funktioniert nur innerhalb eines Canvas!

### Zum Bearbeiten im Scene:
1. Öffne eine Szene mit Canvas
2. Drag & Drop das Prefab ins Canvas
3. Bearbeite es
4. Apply Changes to Prefab
5. Lösche die Instanz aus dem Canvas

## Referenzen im ProductionSlot Script

Das `ProductionSlot.cs` Script hat folgende serialisierte Felder, die auf die Child-Objekte verweisen müssen:

```csharp
[SerializeField] private Image productImage;          // ? ProductImage
[SerializeField] private TextMeshProUGUI productNameText;  // ? ProductName
[SerializeField] private TextMeshProUGUI costText;        // ? CostText
[SerializeField] private TextMeshProUGUI durationText;    // ? DurationText
[SerializeField] private Button productButton;            // ? ProductSlot (Root)
[SerializeField] private Image progressFill;              // ? ProgressFill
[SerializeField] private GameObject progressBar;          // ? ProgressBar
```

Diese werden automatisch vom Setup-Tool verknüpft!
