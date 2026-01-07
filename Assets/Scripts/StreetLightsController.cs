using UnityEngine;
using System.Collections.Generic;

public class StreetLightsController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to your DayNightCycle (recommended). If set, we use its DayFactor.")]
    public DayNightCycle dayNightCycle;

    [Tooltip("Fallback: if DayNightCycle is not set, we can still use sun intensity threshold.")]
    public Light sun;

    [Header("Find + Auto-setup")]
    [Tooltip("Find lamp meshes by name contains this (case-insensitive).")]
    public string lampMeshNameContains = "pole_light";

    [Tooltip("Create a child Light on each found lamp mesh if none exists.")]
    public bool autoCreateLights = true;

    [Header("Night logic (when using DayNightCycle)")]
    [Tooltip("When DayFactor is below this, we consider it 'night enough' to start turning on lamps.")]
    [Range(0f, 1f)]
    public float nightStartsAtDayFactor = 0.56f; // ~ turns on around timeOfDay 0.19

    [Tooltip("How long the fade takes in seconds.")]
    public float fadeSeconds = 1.0f;

    [Header("Fallback night detection (only if DayNightCycle is NOT assigned)")]
    [Tooltip("If sun intensity is <= this, we consider it night.")]
    public float nightSunIntensityThreshold = 0.4f;

    [Header("Light settings")]
    public LightType lightType = LightType.Point;
    public float nightIntensity = 2.0f;
    public float range = 10f;
    public Color lightColor = Color.white;

    private readonly List<Light> streetLights = new();
    private float currentIntensity = 0f;

    void Start()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        }

        if (sun == null)
        {
            if (dayNightCycle != null && dayNightCycle.sun != null)
                sun = dayNightCycle.sun;
            else
                sun = FindFirstObjectByType<Light>(); // not perfect, but better than null
        }

        var allTransforms = FindObjectsOfType<Transform>(true);
        string needle = lampMeshNameContains.ToLower();

        foreach (var t in allTransforms)
        {
            if (t == null || string.IsNullOrEmpty(t.name)) continue;
            if (!t.name.ToLower().Contains(needle)) continue;

            var existing = t.GetComponentInChildren<Light>(true);
            if (existing != null)
            {
                SetupLight(existing);
                streetLights.Add(existing);
                continue;
            }

            if (!autoCreateLights) continue;

            GameObject lightGO = new GameObject("AutoStreetLight");
            lightGO.transform.SetParent(t, false);

            lightGO.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            var l = lightGO.AddComponent<Light>();
            SetupLight(l);
            streetLights.Add(l);
        }

        SetAll(0f);
    }

    void Update()
    {
        float target = ComputeTargetIntensity();

        float speed = (fadeSeconds <= 0.001f) ? 9999f : (1f / fadeSeconds);
        currentIntensity = Mathf.MoveTowards(currentIntensity, target, Time.deltaTime * speed * nightIntensity);

        SetAll(currentIntensity);
    }

    private float ComputeTargetIntensity()
    {
        if (dayNightCycle != null)
        {
            float dayFactor = dayNightCycle.DayFactor;
            float nightFactor = Mathf.InverseLerp(nightStartsAtDayFactor, 0f, dayFactor);
            nightFactor = Mathf.Clamp01(nightFactor);

            nightFactor = nightFactor * nightFactor * (3f - 2f * nightFactor);

            return nightFactor * nightIntensity;
        }

        if (sun != null)
        {
            bool isNight = sun.intensity <= nightSunIntensityThreshold;
            return isNight ? nightIntensity : 0f;
        }

        Debug.LogWarning("StreetLightsController: No DayNightCycle and no Sun found. Lights will stay off.");
        return 0f;
    }

    private void SetupLight(Light l)
    {
        l.type = lightType;
        l.color = lightColor;
        l.range = range;
        l.shadows = LightShadows.None;
        l.intensity = 0f;
        l.enabled = true;
    }

    private void SetAll(float intensity)
    {
        bool enable = intensity > 0.01f;

        for (int i = 0; i < streetLights.Count; i++)
        {
            var l = streetLights[i];
            if (l == null) continue;

            l.enabled = enable;
            if (enable) l.intensity = intensity;
        }
    }
}
