using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public Light sun;

    [Header("Cycle")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.35f;

    public bool autoRun = false;
    public float dayLengthSeconds = 120f;

    [Header("Sun Settings")]
    public float dayIntensity = 1.2f;
    public float nightIntensity = 0.05f;
    public float dayShadowStrength = 1.0f;
    public float nightShadowStrength = 0.0f;

    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.35f, 0.45f, 0.65f);

    [Header("Ambient")]
    public Color ambientDay = new Color(0.85f, 0.85f, 0.85f);
    public Color ambientNight = new Color(0.10f, 0.12f, 0.18f);

    [Header("Controls")]
    public float manualScrubSpeed = 0.1f;

    public float DayFactor { get; private set; } = 1f;

    void Start()
    {
        if (!sun)
            Debug.LogError("DayNightCycle: Assign a Directional Light to 'sun' in the Inspector.");

        Apply(timeOfDay);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Comma)) timeOfDay -= Time.deltaTime * manualScrubSpeed;
        if (Input.GetKey(KeyCode.Period)) timeOfDay += Time.deltaTime * manualScrubSpeed;

        if (autoRun)
            timeOfDay += Time.deltaTime / Mathf.Max(1f, dayLengthSeconds);

        timeOfDay = Mathf.Repeat(timeOfDay, 1f);
        Apply(timeOfDay);
    }

    void Apply(float t)
    {
        if (!sun) return;

        float sunAngle = (t * 360f) - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // 0..1..0 (night->day->night)
        float dayFactor = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
        DayFactor = dayFactor; 

        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayFactor);
        sun.color = Color.Lerp(nightColor, dayColor, dayFactor);
        sun.shadowStrength = Mathf.Lerp(nightShadowStrength, dayShadowStrength, dayFactor);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(ambientNight, ambientDay, dayFactor);
    }
}
