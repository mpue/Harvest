using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class EmissionPulseURP : MonoBehaviour
{
 [Header("Target")]
 [SerializeField] private Renderer targetRenderer;
 [SerializeField] private int materialIndex =0;

 [Header("Emission")]
 [SerializeField] private Color emissionColor = Color.white;
 [SerializeField] private float minIntensity =0f;
 [SerializeField] private float maxIntensity =1f;
 [SerializeField] private float speed =1f; // cycles per second

 [Header("Shader Property")]
 [Tooltip("Name of the color property used by the shader for emission. Default for URP/Lit is '_EmissionColor'.")]
 [SerializeField] private string emissionColorProperty = "_EmissionColor";

 [Header("Behavior")]
 [Tooltip("If true the component will modify the shared material. If false it will instantiate a material instance for this renderer.")]
 [SerializeField] private bool useSharedMaterial = false;
 [SerializeField] private bool enableEmissionKeyword = true;

 [Header("Optimization")]
 [Tooltip("If true the component will update emission using a MaterialPropertyBlock instead of modifying the material instance. Useful to avoid extra material instances.")]
 [SerializeField] private bool usePropertyBlock = false;

 private Material targetMaterial;
 private MaterialPropertyBlock mpb;

 void Awake()
 {
 if (targetRenderer == null)
 targetRenderer = GetComponent<Renderer>();

 if (targetRenderer == null)
 {
 Debug.LogError("EmissionPulseURP: No Renderer found on GameObject or assigned targetRenderer.");
 enabled = false;
 return;
 }

 // clamp material index
 materialIndex = Mathf.Clamp(materialIndex,0, Mathf.Max(0, targetRenderer.sharedMaterials.Length -1));

 // Obtain material reference or create instance
 if (useSharedMaterial || usePropertyBlock)
 {
 // For shared material or property block we reference sharedMaterial (don't create instances)
 targetMaterial = targetRenderer.sharedMaterials[materialIndex];
 }
 else
 {
 // Accessing renderer.materials will create instance(s) for this renderer
 var mats = targetRenderer.materials;
 if (materialIndex >=0 && materialIndex < mats.Length)
 targetMaterial = mats[materialIndex];
 else
 targetMaterial = targetRenderer.material;
 }

 if (targetMaterial == null && !usePropertyBlock)
 {
 Debug.LogError("EmissionPulseURP: Could not obtain target material.");
 enabled = false;
 return;
 }

 if (enableEmissionKeyword && !usePropertyBlock && targetMaterial != null)
 {
 // Enable emission keyword on the material so the shader uses emission
 targetMaterial.EnableKeyword("_EMISSION");
 // For GI (may be ignored in URP) mark as realtime emissive
 targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
 }

 if (usePropertyBlock)
 {
 mpb = new MaterialPropertyBlock();
 }
 }

 void Update()
 {
 // Ping-pong using sin for smooth pulse
 float t = (Mathf.Sin(Time.time * speed * Mathf.PI *2f) +1f) *0.5f;
 float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

 // Compute final color in linear space (URP expects linear color for emission)
 Color finalColor = emissionColor.linear * intensity;

 if (usePropertyBlock)
 {
 // Update via MaterialPropertyBlock to preserve the shared material (and emission texture)
 mpb.Clear();
 mpb.SetColor(emissionColorProperty, finalColor);
 targetRenderer.SetPropertyBlock(mpb, materialIndex);

 // Note: Enabling emission keyword per renderer via property block isn't supported universally.
 }
 else
 {
 if (targetMaterial == null) return;

 // If material uses an emission map, most URP shaders multiply the emission color with the map.
 // Setting the emission color will therefore preserve the texture and only scale its brightness.
 targetMaterial.SetColor(emissionColorProperty, finalColor);

 // Try to update dynamic GI/emissive flag for supported pipelines
 try
 {
 DynamicGI.SetEmissive(targetRenderer, finalColor);
 }
 catch { }
 }
 }

 void OnValidate()
 {
 // keep sane values in editor
 minIntensity = Mathf.Max(0f, minIntensity);
 maxIntensity = Mathf.Max(minIntensity, maxIntensity);
 speed = Mathf.Max(0f, speed);
 if (materialIndex <0) materialIndex =0;
 if (string.IsNullOrEmpty(emissionColorProperty)) emissionColorProperty = "_EmissionColor";
 }
}
