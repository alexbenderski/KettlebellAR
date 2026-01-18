using UnityEngine;
using UnityEngine.UI;

public class GlowAnimator : MonoBehaviour
{
    [Header("Target Frame Image")]
    [SerializeField] private Image outGlow;     // assign your frame image here

    [Header("Glow Settings")]
    [SerializeField] private Color glowColor = Color.yellow;  // Color picker - default yellow
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.9f;
    [SerializeField, Range(0.1f, 5f)] private float speed = 1.5f; // cycles per second
    [SerializeField] private bool autoPlay = true;

    private bool isPlaying = false;
    private Color baseColor = Color.yellow;

    private void Awake()
    {
        if (outGlow == null)
            outGlow = GetComponent<Image>();
        
        // Set base color from the picker
        baseColor = glowColor;
    }

    private void OnEnable()
    {
        if (autoPlay)
            StartGlow();
    }

    private void OnDisable()
    {
        StopGlow();
    }

    public void StartGlow()
    {
        isPlaying = true;
    }

    public void StopGlow()
    {
        isPlaying = false;
        ApplyAlpha(minAlpha);
    }

    private void Update()
    {
        if (!isPlaying || outGlow == null)
            return;

        // Smooth pulsing alpha using sine wave
        float t = (Mathf.Sin(Time.time * Mathf.PI * 2f * speed) + 1f) * 0.5f; // 0..1
        float a = Mathf.Lerp(minAlpha, maxAlpha, t);
        ApplyAlpha(a);
    }

    private void ApplyAlpha(float a)
    {
        var c = baseColor;
        c.a = Mathf.Clamp01(a);
        outGlow.color = c;
    }
}
