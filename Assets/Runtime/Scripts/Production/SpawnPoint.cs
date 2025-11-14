using UnityEngine;

/// <summary>
/// Visual marker for spawn points in the production system
/// Shows a green sphere with arrow indicating spawn direction
/// Automatically displays in Scene View with grid overlay when selected
/// </summary>
[ExecuteInEditMode]
public class SpawnPoint : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoSize = 1f;
    [SerializeField] private bool showLabel = true;
    [SerializeField] private bool showArrow = true;

    private void OnDrawGizmos()
    {
    // Set color with transparency
  Color color = gizmoColor;
        color.a = 0.6f;
        Gizmos.color = color;

        // Draw wire sphere at spawn point
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.5f);
        
        // Draw solid smaller sphere in center
        color.a = 0.8f;
     Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, gizmoSize * 0.2f);

    // Draw direction arrow
        if (showArrow)
        {
        Vector3 forward = transform.forward * gizmoSize * 1.5f;
    Gizmos.DrawLine(transform.position, transform.position + forward);
      
            // Arrow head
      Vector3 arrowTip = transform.position + forward;
            Vector3 right = transform.right * gizmoSize * 0.3f;
     Vector3 up = transform.up * gizmoSize * 0.3f;
        
          Gizmos.DrawLine(arrowTip, arrowTip - forward * 0.3f + right * 0.3f);
 Gizmos.DrawLine(arrowTip, arrowTip - forward * 0.3f - right * 0.3f);
      }

        // Draw ground circle
     DrawCircle(transform.position, gizmoSize, 32);
    }

    private void OnDrawGizmosSelected()
    {
      // Draw more prominent visuals when selected
     Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
     // Draw grid on ground
        DrawGroundGrid(transform.position, gizmoSize * 2f, 4);

#if UNITY_EDITOR
        // Draw label
     if (showLabel)
  {
            UnityEditor.Handles.Label(
transform.position + Vector3.up * gizmoSize,
        $"Spawn Point\n{gameObject.name}",
            new GUIStyle()
                {
   alignment = TextAnchor.MiddleCenter,
         normal = new GUIStyleState() { textColor = gizmoColor }
    }
         );
        }
#endif
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
    angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
         Gizmos.DrawLine(lastPoint, newPoint);
 lastPoint = newPoint;
        }
    }

    private void DrawGroundGrid(Vector3 center, float size, int divisions)
    {
        Color color = gizmoColor;
        color.a = 0.3f;
 Gizmos.color = color;

        float halfSize = size * 0.5f;
        float step = size / divisions;

        // Draw grid lines
        for (int i = 0; i <= divisions; i++)
    {
            float offset = -halfSize + (i * step);
      
            // Lines along Z axis
    Gizmos.DrawLine(
center + new Vector3(offset, 0, -halfSize),
             center + new Vector3(offset, 0, halfSize)
            );
 
       // Lines along X axis
      Gizmos.DrawLine(
 center + new Vector3(-halfSize, 0, offset),
center + new Vector3(halfSize, 0, offset)
         );
        }
    }
}
