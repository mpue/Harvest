using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    [Header("Selection Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask selectableLayer = -1; // All layers by default
    [SerializeField] private float clickThreshold = 0.2f; // Time threshold for click vs drag

    [Header("Box Selection Visual")]
    [SerializeField] private Color boxColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color boxBorderColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private float borderWidth = 2f;

    [Header("Selection Options")]
    [SerializeField] private bool allowMultiSelect = true;
    [SerializeField] private bool selectBuildingsWithBox = true;

    [Header("Movement Settings")]
    [SerializeField] private LayerMask groundLayer = -1; // Layer for ground/terrain
    [SerializeField] private bool useFormations = true;
    [SerializeField] private float formationSpacing = 2f;

    private List<BaseUnit> selectedUnits = new List<BaseUnit>();
    private Vector3 mouseDownPosition;
    private bool isDragging = false;
    private float mouseDownTime;
    private RTSCamera rtsCamera;

    // GUI
    private Rect selectionBox;
    private Texture2D boxTexture;
    private Texture2D borderTexture;

    public List<BaseUnit> SelectedUnits => selectedUnits;
    public int SelectedCount => selectedUnits.Count;

    void Start()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("UnitSelector: No camera assigned and no main camera found!");
            }
        }

        // Find RTSCamera component
        if (mainCamera != null)
        {
            rtsCamera = mainCamera.GetComponent<RTSCamera>();
        }

        // Create textures for box selection
        boxTexture = CreateTexture(boxColor);
        borderTexture = CreateTexture(boxBorderColor);
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // Left mouse button pressed
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
            mouseDownTime = Time.time;
            isDragging = false;
        }

        // Left mouse button held - check for drag
        if (Input.GetMouseButton(0))
        {
            // Check if moved enough to be considered a drag
            float dragDistance = Vector3.Distance(Input.mousePosition, mouseDownPosition);
            float dragTime = Time.time - mouseDownTime;

            if (dragDistance > 5f && dragTime > clickThreshold)
            {
                isDragging = true;
                UpdateSelectionBox();
            }
        }

        // Left mouse button released
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // Box selection
                PerformBoxSelection();
            }
            else
            {
                // Single click selection
                PerformClickSelection();
            }

            isDragging = false;
        }

        // Right mouse button - move command
        if (Input.GetMouseButtonDown(1))
        {
            // Only handle move command if camera is not in free-look mode
            bool cameraInFreeLook = rtsCamera != null && rtsCamera.IsInFreeLookMode();
            if (!cameraInFreeLook && selectedUnits.Count > 0)
            {
                HandleMoveCommand();
            }
        }

        // Deselect all with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectAll();
        }
    }

    /// <summary>
    /// Handles right-click movement commands for selected controllable units
    /// </summary>
    private void HandleMoveCommand()
    {
        if (selectedUnits.Count == 0) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPosition = hit.point;

            // Get all controllable units from selection
            List<Controllable> controllableUnits = new List<Controllable>();
            foreach (BaseUnit unit in selectedUnits)
            {
                Controllable controllable = unit.GetComponent<Controllable>();
                if (controllable != null && !unit.IsBuilding)
                {
                    controllableUnits.Add(controllable);
                }
            }

            if (controllableUnits.Count > 0)
            {
                if (useFormations && controllableUnits.Count > 1)
                {
                    // Move units in formation
                    MoveUnitsInFormation(controllableUnits, targetPosition);
                }
                else
                {
                    // Move single unit or all to same position
                    foreach (Controllable controllable in controllableUnits)
                    {
                        controllable.MoveTo(targetPosition);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Moves multiple units in a formation around the target position
    /// </summary>
    private void MoveUnitsInFormation(List<Controllable> units, Vector3 centerPosition)
    {
        if (units.Count == 1)
        {
            units[0].MoveTo(centerPosition);
            return;
        }

        // Calculate formation positions in a grid
        int unitsPerRow = Mathf.CeilToInt(Mathf.Sqrt(units.Count));
        int currentRow = 0;
        int currentCol = 0;

        for (int i = 0; i < units.Count; i++)
        {
            // Calculate offset from center
            float offsetX = (currentCol - unitsPerRow / 2f) * formationSpacing;
            float offsetZ = (currentRow - unitsPerRow / 2f) * formationSpacing;
            Vector3 offset = new Vector3(offsetX, 0, offsetZ);

            // Move unit to formation position
            units[i].MoveTo(centerPosition, offset);

            // Update grid position
            currentCol++;
            if (currentCol >= unitsPerRow)
            {
                currentCol = 0;
                currentRow++;
            }
        }
    }

    /// <summary>
    /// Performs a single click selection
    /// </summary>
    private void PerformClickSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectableLayer))
        {
            BaseUnit unit = hit.collider.GetComponent<BaseUnit>();
            if (unit == null)
            {
                unit = hit.collider.GetComponentInParent<BaseUnit>();
            }

            if (unit != null)
            {
                // Shift key for additive selection
                bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (!additive)
                {
                    DeselectAll();
                }

                if (unit.IsSelected && additive)
                {
                    // Deselect if already selected and shift is held
                    DeselectUnit(unit);
                }
                else
                {
                    // Select the unit
                    SelectUnit(unit);
                }
            }
            else
            {
                // Clicked on empty space
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    DeselectAll();
                }
            }
        }
        else
        {
            // Clicked on nothing
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                DeselectAll();
            }
        }
    }

    /// <summary>
    /// Performs box selection
    /// </summary>
    private void PerformBoxSelection()
    {
        if (!allowMultiSelect) return;

        // Clear previous selection if not holding shift
        bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (!additive)
        {
            DeselectAll();
        }

        // Find all BaseUnits in the scene
        BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();

        foreach (BaseUnit unit in allUnits)
        {
            // Skip buildings if not allowed
            if (unit.IsBuilding && !selectBuildingsWithBox)
            {
                continue;
            }

            // Check if unit is within selection box
            if (unit.IsWithinSelectionBounds(selectionBox, mainCamera))
            {
                SelectUnit(unit);
            }
        }
    }

    /// <summary>
    /// Updates the selection box rectangle
    /// </summary>
    private void UpdateSelectionBox()
    {
        float minX = Mathf.Min(mouseDownPosition.x, Input.mousePosition.x);
        float maxX = Mathf.Max(mouseDownPosition.x, Input.mousePosition.x);
        float minY = Mathf.Min(mouseDownPosition.y, Input.mousePosition.y);
        float maxY = Mathf.Max(mouseDownPosition.y, Input.mousePosition.y);

        // Convert to screen space (flip Y)
        minY = Screen.height - maxY;
        maxY = Screen.height - Mathf.Min(mouseDownPosition.y, Input.mousePosition.y);

        selectionBox = Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    /// <summary>
    /// Selects a single unit
    /// </summary>
    public void SelectUnit(BaseUnit unit)
    {
        if (unit == null || unit.IsSelected) return;

        if (!allowMultiSelect && selectedUnits.Count > 0)
        {
            DeselectAll();
        }

        unit.Select();
        selectedUnits.Add(unit);
    }

    /// <summary>
    /// Deselects a single unit
    /// </summary>
    public void DeselectUnit(BaseUnit unit)
    {
        if (unit == null || !unit.IsSelected) return;

        unit.Deselect();
        selectedUnits.Remove(unit);
    }

    /// <summary>
    /// Deselects all units
    /// </summary>
    public void DeselectAll()
    {
        foreach (BaseUnit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.Deselect();
            }
        }
        selectedUnits.Clear();
    }

    /// <summary>
    /// Gets all currently selected units of a specific type
    /// </summary>
    public List<T> GetSelectedUnitsOfType<T>() where T : BaseUnit
    {
        List<T> result = new List<T>();
        foreach (BaseUnit unit in selectedUnits)
        {
            if (unit is T typedUnit)
            {
                result.Add(typedUnit);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a simple colored texture
    /// </summary>
    private Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Draws the selection box on screen
    /// </summary>
    void OnGUI()
    {
        if (isDragging && boxTexture != null && borderTexture != null)
        {
            // Draw filled box
            GUI.DrawTexture(selectionBox, boxTexture);

            // Draw border
            DrawBorder(selectionBox, borderWidth, borderTexture);
        }

        // Optional: Display selection count
        if (selectedUnits.Count > 0)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;
            GUI.Label(new Rect(10, 10, 200, 30), $"Selected: {selectedUnits.Count}", style);
        }
    }

    /// <summary>
    /// Draws a border around a rectangle
    /// </summary>
    private void DrawBorder(Rect rect, float thickness, Texture2D texture)
    {
        // Top
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), texture);
        // Bottom
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), texture);
        // Left
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), texture);
        // Right
        GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), texture);
    }

    void OnDestroy()
    {
        // Cleanup textures
        if (boxTexture != null) Destroy(boxTexture);
        if (borderTexture != null) Destroy(borderTexture);
    }
}
