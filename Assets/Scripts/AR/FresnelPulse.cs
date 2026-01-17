using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Animates the Fresnel Power on Custom/GlowingEdgesFresnel materials to create a pulsing glow effect.
/// Attach this to a GameObject with a Renderer, or to a parent that has Renderers in its children.
/// </summary>
public class FresnelPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Minimum Fresnel Power (more glow)")]
    public float minPower = 0.5f;
    
    [Tooltip("Maximum Fresnel Power (less glow)")]
    public float maxPower = 10f;
    
    [Tooltip("Pulse speed - higher = faster pulsing")]
    public float pulseSpeed = 1f;
    
    [Header("Target Settings")]
    [Tooltip("If true, animates all materials on this object and its children. If false, only this object's materials.")]
    public bool includeChildren = true;
    
    [Tooltip("Optional: manually assign specific renderers to animate. Leave empty to auto-detect.")]
    public Renderer[] targetRenderers;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Store renderers instead of material instances to avoid Unity's material instancing issues
    private List<Renderer> glowingRenderers = new List<Renderer>();
    private float lastFresnel = -1f;
    private const string FRESNEL_PROPERTY = "_FresnelPower";
    private const string SHADER_NAME = "Custom/GlowingEdgesFresnel";

    void OnEnable()
    {
        FindGlowingRenderers();
        if (showDebugLogs)
            Debug.Log($"[FresnelPulse] OnEnable - Found {glowingRenderers.Count} renderers with glowing materials");
    }

    void FindGlowingRenderers()
    {
        glowingRenderers.Clear();
        
        Renderer[] renderers;
        
        // Get renderers based on settings
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            renderers = targetRenderers;
        }
        else if (includeChildren)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            Renderer singleRenderer = GetComponent<Renderer>();
            renderers = singleRenderer != null ? new Renderer[] { singleRenderer } : new Renderer[0];
        }

        // Find renderers that have materials with the glowing shader
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            
            // Check if ANY material on this renderer uses the glowing shader
            bool hasGlowingMaterial = false;
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null && mat.shader.name == SHADER_NAME)
                {
                    hasGlowingMaterial = true;
                    break;
                }
            }
            
            if (hasGlowingMaterial)
            {
                glowingRenderers.Add(renderer);
                if (showDebugLogs)
                    Debug.Log($"[FresnelPulse] Added renderer: {renderer.gameObject.name}");
            }
        }
        
        if (glowingRenderers.Count == 0 && showDebugLogs)
        {
            Debug.LogWarning($"[FresnelPulse] No renderers with '{SHADER_NAME}' shader found on {gameObject.name}");
        }
    }

    void Update()
    {
        if (glowingRenderers.Count == 0)
        {
            // Retry finding renderers (maybe they weren't loaded yet)
            FindGlowingRenderers();
            return;
        }

        // Calculate pulsing value using PingPong (oscillates smoothly between 0 and 1)
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        
        // Lerp between minPower and maxPower
        float currentPower = Mathf.Lerp(minPower, maxPower, t);

        // Apply to all renderers' materials directly
        foreach (Renderer renderer in glowingRenderers)
        {
            if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;
            
            // Get material instances (Unity creates them automatically on first access)
            Material[] materials = renderer.materials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.shader.name == SHADER_NAME && mat.HasProperty(FRESNEL_PROPERTY))
                {
                    mat.SetFloat(FRESNEL_PROPERTY, currentPower);
                }
            }
        }
        
        // Log only when value changes significantly (to avoid spam)
        if (showDebugLogs && Mathf.Abs(currentPower - lastFresnel) > 1f)
        {
            lastFresnel = currentPower;
            Debug.Log($"[FresnelPulse] Fresnel Power: {currentPower:F2} on {glowingRenderers.Count} renderers");
        }
    }

    void OnDisable()
    {
        ResetMaterials();
    }

    void ResetMaterials()
    {
        foreach (Renderer renderer in glowingRenderers)
        {
            if (renderer == null) continue;
            
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat != null && mat.shader.name == SHADER_NAME && mat.HasProperty(FRESNEL_PROPERTY))
                {
                    mat.SetFloat(FRESNEL_PROPERTY, 3f);
                }
            }
        }
        
        if (showDebugLogs)
            Debug.Log("[FresnelPulse] Reset all materials to default power 3");
    }

    void OnDestroy()
    {
        ResetMaterials();
        glowingRenderers.Clear();
    }

    // Expose public methods to control pulsing from other scripts
    public void StartPulsing()
    {
        enabled = true;
    }

    public void StopPulsing()
    {
        enabled = false;
    }

    public void SetPulseSpeed(float speed)
    {
        pulseSpeed = Mathf.Max(0.1f, speed);
    }
}
