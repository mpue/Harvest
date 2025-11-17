using UnityEngine;

/// <summary>
/// Debug helper for HealthBar issues
/// Add this to a HealthBar to see debug information
/// </summary>
public class HealthBarDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool drawDebugRays = true;
    [SerializeField] private bool logVisibilityChanges = true; // NEW
    [SerializeField] private Color rayColor = Color.cyan;

    private HealthBar healthBar;
    private Camera mainCamera;
    private Transform parentTransform;
    private float lastAlpha = -1f; // NEW: Track alpha changes

    void Start()
    {
        healthBar = GetComponent<HealthBar>();
        mainCamera = Camera.main;

        if (transform.parent != null)
        {
            parentTransform = transform.parent;
        }
    }

    void Update()
    {
        if (drawDebugRays && mainCamera != null)
        {
            // Draw ray from healthbar to camera
            Debug.DrawRay(transform.position, mainCamera.transform.position - transform.position, rayColor);

            // Draw forward direction of healthbar
            Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);

            // Draw up direction
            Debug.DrawRay(transform.position, transform.up * 1f, Color.green);
        }

        // NEW: Log visibility changes
        if (logVisibilityChanges)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null && Mathf.Abs(cg.alpha - lastAlpha) > 0.01f)
            {
                Debug.Log($"[HealthBar] Alpha changed: {lastAlpha:F2} ? {cg.alpha:F2}");
                lastAlpha = cg.alpha;
            }
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.5f));
        style.padding = new RectOffset(5, 5, 5, 5);

        string debugText = "=== HEALTHBAR DEBUG ===\n";

        // NEW: Visibility info
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            debugText += $"\nCanvas Group Alpha: {cg.alpha:F2}\n";
            debugText += $"Visible: {(cg.alpha > 0.1f ? "YES" : "NO")}\n";
        }

        // NEW: Selection state
        if (parentTransform != null)
        {
            BaseUnit unit = parentTransform.GetComponent<BaseUnit>();
            if (unit != null)
            {
                debugText += $"\nUnit Selected: {unit.IsSelected}\n";
            }
        }

        // NEW: Health info
        if (parentTransform != null)
        {
            Health health = parentTransform.GetComponent<Health>();
            if (health != null)
            {
                debugText += $"Health: {health.CurrentHealth:F0}/{health.MaxHealth:F0}\n";
                debugText += $"Health %: {health.HealthPercentage:F2}\n";
            }
        }

        // Position info
        debugText += $"\nPosition:\n";
        debugText += $"  World: {transform.position}\n";
        debugText += $"  Local: {transform.localPosition}\n";

        // Rotation info
        debugText += $"\nRotation:\n";
        debugText += $"  World: {transform.rotation.eulerAngles}\n";
        debugText += $"  Local: {transform.localRotation.eulerAngles}\n";

        // Parent info
        if (parentTransform != null)
        {
            debugText += $"\nParent Rotation:\n";
            debugText += $"  {parentTransform.rotation.eulerAngles}\n";
        }

        // Camera info
        if (mainCamera != null)
        {
            Vector3 dirToCamera = mainCamera.transform.position - transform.position;
            debugText += $"\nCamera Direction:\n";
            debugText += $"  {dirToCamera.normalized}\n";

            Vector3 forward = transform.forward;
            float angle = Vector3.Angle(forward, dirToCamera);
            debugText += $"\nAngle to Camera: {angle:F1}°\n";
            debugText += $"(Should be ~0° if facing camera)\n";
        }

        GUI.Label(new Rect(10, 100, 350, 500), debugText, style);
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    void OnDrawGizmos()
    {
        if (!drawDebugRays || mainCamera == null) return;

        // Draw healthbar bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.2f, 0.1f));

        // Draw arrow to camera
        Gizmos.color = Color.cyan;
        Vector3 toCamera = mainCamera.transform.position - transform.position;
        Gizmos.DrawRay(transform.position, toCamera.normalized * 2f);

        // NEW: Show visibility state with color
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            Gizmos.color = cg.alpha > 0.5f ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
