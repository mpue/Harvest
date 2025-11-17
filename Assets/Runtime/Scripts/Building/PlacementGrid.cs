using UnityEngine;

/// <summary>
/// Displays a grid overlay during building placement
/// </summary>
public class PlacementGrid : MonoBehaviour
{
[Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private int gridWidth = 50;
    [SerializeField] private int gridHeight = 50;
 [SerializeField] private float gridYOffset = 0.1f;

    [Header("Visual Settings")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color centerLineColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private bool showCenterLines = true;

    [Header("Material")]
    [SerializeField] private Material gridMaterial;
    [SerializeField] private bool useShader = true;

    [Header("Fade Settings")]
    [SerializeField] private bool fadeWithDistance = true;
[SerializeField] private float fadeStartDistance = 20f;
    [SerializeField] private float fadeEndDistance = 40f;

    [Header("Performance")]
    [SerializeField] private bool useMeshGrid = true;
    [SerializeField] private int updateInterval = 2; // Update every N frames

    private GameObject gridObject;
    private MeshRenderer gridRenderer;
    private LineRenderer[] lineRenderers;
    private Camera mainCamera;
  private bool isVisible = false;
    private int frameCounter = 0;

    void Awake()
  {
        mainCamera = Camera.main;
        if (mainCamera == null)
 {
mainCamera = FindObjectOfType<Camera>();
   }

     CreateGrid();
Hide(); // Start hidden
    }

    void Update()
    {
     if (!isVisible) return;

        frameCounter++;
        if (updateInterval > 0 && frameCounter % updateInterval != 0)
  return;

        UpdateGridPosition();
        UpdateGridVisibility();
    }

    /// <summary>
    /// Show the grid
  /// </summary>
    public void Show()
    {
        if (gridObject != null)
        {
    gridObject.SetActive(true);
  isVisible = true;
        }
    }

    /// <summary>
    /// Hide the grid
    /// </summary>
    public void Hide()
    {
        if (gridObject != null)
  {
            gridObject.SetActive(false);
    isVisible = false;
        }
    }

    /// <summary>
  /// Set grid center position
    /// </summary>
    public void SetPosition(Vector3 position)
    {
    if (gridObject != null)
        {
    position.y = GetTerrainHeight(position) + gridYOffset;
     gridObject.transform.position = position;
     }
    }

    /// <summary>
    /// Create the grid visual
    /// </summary>
    private void CreateGrid()
    {
        gridObject = new GameObject("PlacementGrid");
        gridObject.transform.SetParent(transform);

        if (useMeshGrid)
   {
         CreateMeshGrid();
    }
        else
        {
        CreateLineGrid();
        }
    }

    /// <summary>
    /// Create grid using mesh (better performance)
    /// </summary>
    private void CreateMeshGrid()
    {
        MeshFilter meshFilter = gridObject.AddComponent<MeshFilter>();
        gridRenderer = gridObject.AddComponent<MeshRenderer>();

   Mesh mesh = GenerateGridMesh();
   meshFilter.mesh = mesh;

    // Setup material
        if (gridMaterial == null)
 {
            gridMaterial = CreateGridMaterial();
        }
        gridRenderer.material = gridMaterial;
        gridRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        gridRenderer.receiveShadows = false;
  }

    /// <summary>
    /// Generate grid mesh
    /// </summary>
private Mesh GenerateGridMesh()
    {
        Mesh mesh = new Mesh();
mesh.name = "PlacementGridMesh";

        int verticalLines = gridWidth + 1;
      int horizontalLines = gridHeight + 1;
        int totalLines = verticalLines + horizontalLines;

   // 4 vertices per line (quad)
Vector3[] vertices = new Vector3[totalLines * 4];
  Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[totalLines * 6];

     int vertexIndex = 0;
        int triangleIndex = 0;

      float halfWidth = gridWidth * gridSize * 0.5f;
        float halfHeight = gridHeight * gridSize * 0.5f;

        // Vertical lines
        for (int i = 0; i <= gridWidth; i++)
        {
            float x = -halfWidth + i * gridSize;
      CreateLineQuad(ref vertices, ref uvs, ref triangles, ref vertexIndex, ref triangleIndex,
   new Vector3(x, 0, -halfHeight),
                new Vector3(x, 0, halfHeight),
                lineWidth);
}

        // Horizontal lines
   for (int i = 0; i <= gridHeight; i++)
        {
            float z = -halfHeight + i * gridSize;
   CreateLineQuad(ref vertices, ref uvs, ref triangles, ref vertexIndex, ref triangleIndex,
  new Vector3(-halfWidth, 0, z),
            new Vector3(halfWidth, 0, z),
     lineWidth);
        }

mesh.vertices = vertices;
     mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Create a quad for a line
 /// </summary>
    private void CreateLineQuad(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles,
        ref int vertexIndex, ref int triangleIndex, Vector3 start, Vector3 end, float width)
 {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * width * 0.5f;

        vertices[vertexIndex] = start - perpendicular;
        vertices[vertexIndex + 1] = start + perpendicular;
        vertices[vertexIndex + 2] = end + perpendicular;
        vertices[vertexIndex + 3] = end - perpendicular;

        uvs[vertexIndex] = new Vector2(0, 0);
        uvs[vertexIndex + 1] = new Vector2(1, 0);
  uvs[vertexIndex + 2] = new Vector2(1, 1);
  uvs[vertexIndex + 3] = new Vector2(0, 1);

        triangles[triangleIndex] = vertexIndex;
        triangles[triangleIndex + 1] = vertexIndex + 1;
        triangles[triangleIndex + 2] = vertexIndex + 2;
        triangles[triangleIndex + 3] = vertexIndex;
      triangles[triangleIndex + 4] = vertexIndex + 2;
        triangles[triangleIndex + 5] = vertexIndex + 3;

      vertexIndex += 4;
        triangleIndex += 6;
    }

    /// <summary>
    /// Create grid using LineRenderers (simpler but less performance)
    /// </summary>
    private void CreateLineGrid()
    {
      int verticalLines = gridWidth + 1;
        int horizontalLines = gridHeight + 1;
        lineRenderers = new LineRenderer[verticalLines + horizontalLines];

   float halfWidth = gridWidth * gridSize * 0.5f;
      float halfHeight = gridHeight * gridSize * 0.5f;

        int lineIndex = 0;

        // Vertical lines
        for (int i = 0; i <= gridWidth; i++)
        {
     GameObject lineObj = new GameObject($"GridLine_V{i}");
  lineObj.transform.SetParent(gridObject.transform);

     LineRenderer line = lineObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(line);

       float x = -halfWidth + i * gridSize;
            line.SetPosition(0, new Vector3(x, 0, -halfHeight));
            line.SetPosition(1, new Vector3(x, 0, halfHeight));

lineRenderers[lineIndex++] = line;
        }

      // Horizontal lines
     for (int i = 0; i <= gridHeight; i++)
     {
       GameObject lineObj = new GameObject($"GridLine_H{i}");
            lineObj.transform.SetParent(gridObject.transform);

          LineRenderer line = lineObj.AddComponent<LineRenderer>();
      ConfigureLineRenderer(line);

   float z = -halfHeight + i * gridSize;
  line.SetPosition(0, new Vector3(-halfWidth, 0, z));
 line.SetPosition(1, new Vector3(halfWidth, 0, z));

        lineRenderers[lineIndex++] = line;
        }
    }

    /// <summary>
 /// Configure a LineRenderer
    /// </summary>
    private void ConfigureLineRenderer(LineRenderer line)
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    line.material = gridMaterial != null ? gridMaterial : new Material(Shader.Find("Sprites/Default"));
     line.startColor = gridColor;
        line.endColor = gridColor;
     line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.positionCount = 2;
    }

    /// <summary>
    /// Create default grid material
    /// </summary>
    private Material CreateGridMaterial()
    {
        Material mat;

        if (useShader)
     {
    // Try to use Unlit/Color shader
            Shader shader = Shader.Find("Unlit/Color");
    if (shader == null)
            {
       shader = Shader.Find("Sprites/Default");
            }
 mat = new Material(shader);
        }
        else
        {
     mat = new Material(Shader.Find("Sprites/Default"));
    }

        mat.color = gridColor;
  mat.SetFloat("_Mode", 2); // Transparent mode if available
        mat.renderQueue = 3000; // Transparent queue

        return mat;
    }

    /// <summary>
    /// Update grid position to follow camera or stay at placement position
    /// </summary>
    private void UpdateGridPosition()
    {
        if (mainCamera == null || gridObject == null) return;

        // Grid stays at set position, just update Y based on terrain
     Vector3 currentPos = gridObject.transform.position;
   currentPos.y = GetTerrainHeight(currentPos) + gridYOffset;
  gridObject.transform.position = currentPos;
    }

    /// <summary>
    /// Update grid visibility based on distance
    /// </summary>
    private void UpdateGridVisibility()
    {
      if (!fadeWithDistance || mainCamera == null || gridObject == null) return;

     float distance = Vector3.Distance(mainCamera.transform.position, gridObject.transform.position);
        float alpha = 1f;

        if (distance > fadeStartDistance)
        {
            float fadeRange = fadeEndDistance - fadeStartDistance;
            float fadeAmount = (distance - fadeStartDistance) / fadeRange;
  alpha = Mathf.Clamp01(1f - fadeAmount);
      }

        // Update material alpha
if (gridRenderer != null && gridRenderer.material != null)
        {
            Color color = gridColor;
    color.a *= alpha;
     gridRenderer.material.color = color;
        }

   // Update LineRenderers alpha
 if (lineRenderers != null)
 {
            foreach (var line in lineRenderers)
   {
    if (line != null)
     {
       Color color = gridColor;
        color.a *= alpha;
            line.startColor = color;
         line.endColor = color;
    }
            }
        }
    }

    /// <summary>
    /// Get terrain height at position
    /// </summary>
    private float GetTerrainHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f))
   {
          return hit.point.y;
        }
        return 0f;
    }

    /// <summary>
    /// Set grid size
    /// </summary>
    public void SetGridSize(float size)
    {
  gridSize = size;
        RecreateGrid();
    }

    /// <summary>
    /// Set grid dimensions
    /// </summary>
    public void SetGridDimensions(int width, int height)
    {
        gridWidth = width;
 gridHeight = height;
     RecreateGrid();
    }

    /// <summary>
    /// Recreate grid with current settings
    /// </summary>
    private void RecreateGrid()
    {
      if (gridObject != null)
        {
            Destroy(gridObject);
        }
        CreateGrid();
        if (!isVisible)
        {
     Hide();
      }
    }

    /// <summary>
    /// Set grid color
    /// </summary>
  public void SetGridColor(Color color)
    {
        gridColor = color;

      if (gridRenderer != null && gridRenderer.material != null)
    {
   gridRenderer.material.color = color;
        }

        if (lineRenderers != null)
        {
            foreach (var line in lineRenderers)
          {
           if (line != null)
       {
line.startColor = color;
        line.endColor = color;
     }
            }
        }
    }

    void OnDestroy()
    {
        if (gridObject != null)
        {
     Destroy(gridObject);
     }
    }

    void OnDrawGizmosSelected()
    {
        // Draw grid bounds
        Gizmos.color = Color.yellow;
        Vector3 size = new Vector3(gridWidth * gridSize, 0.1f, gridHeight * gridSize);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
