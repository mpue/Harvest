using UnityEngine;

/// <summary>
/// Visual marker for rally points in the production system
/// Shows a blue flag where produced units will move to
/// Automatically displays in Scene View with grid overlay when selected
/// </summary>
[ExecuteInEditMode]
public class RallyPoint : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.blue;
    [SerializeField] private float gizmoSize = 1f;
    [SerializeField] private bool showLabel = true;
    [SerializeField] private bool showFlag = true;

    private void OnDrawGizmos()
    {
        // Set color with transparency
        Color color = gizmoColor;
        color.a = 0.6f;
        Gizmos.color = color;

        // Draw wire sphere at rally point
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.5f);

        // Draw flag pole and flag
        if (showFlag)
        {
            DrawFlag();
        }

        // Draw ground circle
        DrawCircle(transform.position, gizmoSize, 32);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw more prominent visuals when selected
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // Draw larger flag when selected
        DrawFlag(1.5f);

        // Draw grid on ground
        DrawGroundGrid(transform.position, gizmoSize * 2f, 4);

#if UNITY_EDITOR
        // Draw label
        if (showLabel)
        {
            UnityEditor.Handles.Label(
             transform.position + Vector3.up * gizmoSize * 2.5f,
            $"Rally Point\n{gameObject.name}",
      new GUIStyle()
      {
          alignment = TextAnchor.MiddleCenter,
          normal = new GUIStyleState() { textColor = gizmoColor }
      }
         );
        }
#endif
    }

    private void DrawFlag(float scale = 1f)
    {
        Color color = gizmoColor;
        color.a = 0.8f;
        Gizmos.color = color;

        Vector3 flagBase = transform.position;
        Vector3 flagTop = flagBase + Vector3.up * gizmoSize * 2f * scale;

        // Draw flag pole
        Gizmos.DrawLine(flagBase, flagTop);

        // Draw flag
        Vector3 flagRight = flagTop + transform.right * gizmoSize * 0.8f * scale;
        Vector3 flagBottom = flagTop + Vector3.down * gizmoSize * 0.6f * scale;

        // Flag triangle
        Gizmos.DrawLine(flagTop, flagRight);
        Gizmos.DrawLine(flagRight, flagBottom);
        Gizmos.DrawLine(flagBottom, flagTop);

        // Flag fill (multiple lines to simulate fill)
        for (int i = 1; i < 5; i++)
        {
            float t = i / 5f;
            Vector3 left = Vector3.Lerp(flagTop, flagBottom, t);
            Vector3 right = Vector3.Lerp(flagTop, flagRight, t);
            Gizmos.DrawLine(left, right);
        }
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
