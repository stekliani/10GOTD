using TMPro;
using UnityEngine;

public class UIGlowPulse : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private Material _instancedMaterial;

    [Header("UI Colors")]
    [SerializeField]
    private Color[] glowColors = new Color[] {
        new Color(0f, 1f, 1f),     // Cyan (Button Border)
        new Color(0.2f, 0.8f, 1f), // Light Blue (Button Background)
        new Color(0.5f, 0f, 1f),   // Purple (Window Border)
        new Color(1f, 0.5f, 0f),   // Orange (Window Background)
        new Color(1f, 0f, 0.5f)    // Pink (Upgrades Window BG)
    };

    [Header("Animation Settings")]
    [SerializeField] private float colorChangeSpeed = 2f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;

    private int currentColorIndex = 0;
    private float colorTimer = 0f;

    void Start()
    {
        if (textMeshPro == null) textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null) return;

        // Use a per-object material instance so we don't affect other TMP texts
        _instancedMaterial = textMeshPro.fontMaterial;
        _instancedMaterial.EnableKeyword("GLOW_ON");
    }

    void Update()
    {
        if (_instancedMaterial == null) return;
        // Pulse alpha
        float alpha = Mathf.PingPong(Time.time * pulseSpeed, maxAlpha - minAlpha) + minAlpha;

        // Update color based on time
        colorTimer += Time.unscaledDeltaTime * colorChangeSpeed;

        if (colorTimer >= 1f)
        {
            colorTimer = 0f;
            currentColorIndex = (currentColorIndex + 1) % glowColors.Length;
        }

        // Smooth transition between colors
        int nextIndex = (currentColorIndex + 1) % glowColors.Length;
        Color currentColor = Color.Lerp(glowColors[currentColorIndex],
                                       glowColors[nextIndex],
                                       colorTimer);

        // Apply alpha
        currentColor.a = alpha;

        // Apply to material
        _instancedMaterial.SetColor("_GlowColor", currentColor);

        // Optional: Pulse power too
        float glowPower = Mathf.PingPong(Time.time * 2, 0.5f) + 0.5f;
        _instancedMaterial.SetFloat("_GlowPower", glowPower);
    }
}