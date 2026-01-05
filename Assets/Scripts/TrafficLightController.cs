using UnityEngine;
using System.Collections.Generic;

public class TrafficLightController : MonoBehaviour
{
    [Header("Traffic Light Groups")]
    [Tooltip("Drag all North-South traffic lights here (lights facing North or South)")]
    public List<GameObject> northSouthLights = new List<GameObject>();

    [Tooltip("Drag all East-West traffic lights here (lights facing East or West)")]
    public List<GameObject> eastWestLights = new List<GameObject>();

    [Header("Light Timing (in seconds)")]
    public float redDuration = 5f;
    public float yellowDuration = 2f;
    public float greenDuration = 5f;

    [Header("Materials (Assign Shader Graph materials here)")]
    [Tooltip("Drag materials created from TrafficLightEmissive shader here")]
    public Material redLightMaterial;
    public Material yellowLightMaterial;
    public Material greenLightMaterial;

    [Tooltip("Material for inactive lights (non-emissive). Will be created automatically if not assigned.")]
    public Material offLightMaterial;


    private enum LightState { Red, Yellow, Green }
    private LightState currentState = LightState.Red;
    private LightState previousState = LightState.Red; 
    private float stateTimer = 0f;
    private bool northSouthIsGreen = true; 

    private Dictionary<GameObject, LightMeshes> lightMeshCache = new Dictionary<GameObject, LightMeshes>();

    private Dictionary<Renderer, Material> materialInstanceCache = new Dictionary<Renderer, Material>();

    private class LightMeshes
    {
        public Renderer redLight;
        public Renderer yellowLight;
        public Renderer greenLight;
    }

    void Start()
    {
        if (northSouthLights == null || northSouthLights.Count == 0)
        {
            Debug.LogWarning("TrafficLightController: No North-South traffic lights assigned! Please drag traffic light GameObjects into the northSouthLights list in the Inspector.");
        }

        if (eastWestLights == null || eastWestLights.Count == 0)
        {
            Debug.LogWarning("TrafficLightController: No East-West traffic lights assigned! Please drag traffic light GameObjects into the eastWestLights list in the Inspector.");
        }

        CacheLightMeshes(northSouthLights);
        CacheLightMeshes(eastWestLights);

        if (redLightMaterial == null || yellowLightMaterial == null || greenLightMaterial == null)
        {
            Debug.LogError("TrafficLightController: Please assign Red, Yellow, and Green materials created from TrafficLightEmissive shader!");
            return;
        }

        if (offLightMaterial == null)
        {
            CreateOffMaterial();
        }

        if (lightMeshCache.Count == 0)
        {
            Debug.LogError("TrafficLightController: No traffic lights were found! Make sure:");
            Debug.LogError("  1. Traffic light GameObjects are assigned to northSouthLights and/or eastWestLights lists");
            Debug.LogError("  2. Each traffic light has a child named 'Traffic_light_EU'");
            Debug.LogError("  3. Each Traffic_light_EU has children named 'red', 'yellow', and 'green' with Renderer components");
            return;
        }

        SetAllLightsRed();
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case LightState.Red:
                if (stateTimer >= redDuration)
                {
                    currentState = LightState.Green;
                    stateTimer = 0f;
                    SetLightsGreen();
                    Debug.Log("DEBUG: Red -> Green");
                }
                break;

            case LightState.Green:
                if (stateTimer >= greenDuration)
                {
                    currentState = LightState.Yellow;
                    stateTimer = 0f;
                    SetLightsYellow();
                    Debug.Log("DEBUG: Green -> Yellow");
                }
                break;

            case LightState.Yellow:
                if (stateTimer >= yellowDuration)
                {
                    currentState = LightState.Red;
                    stateTimer = 0f;

                    if (eastWestLights != null && eastWestLights.Count > 0)
                        northSouthIsGreen = !northSouthIsGreen;

                    SetAllLightsRed();
                    Debug.Log($"DEBUG: Yellow -> Red (toggle next green: northSouthIsGreen={northSouthIsGreen})");
                }
                break;
        }
    }

    void CacheLightMeshes(List<GameObject> trafficLights)
    {
        foreach (GameObject trafficLight in trafficLights)
        {
            if (trafficLight == null)
            {
                Debug.LogWarning("TrafficLightController: Null traffic light in list!");
                continue;
            }

            Transform trafficLightEU = trafficLight.transform.Find("Traffic_light_EU");
            if (trafficLightEU == null)
            {
                trafficLightEU = FindChildRecursive(trafficLight.transform, "Traffic_light_EU");
            }

            if (trafficLightEU != null)
            {
                LightMeshes meshes = new LightMeshes();

                meshes.redLight = FindLightMesh(trafficLightEU, "red");
                meshes.yellowLight = FindLightMesh(trafficLightEU, "yellow");
                meshes.greenLight = FindLightMesh(trafficLightEU, "green");

                if (meshes.redLight != null && meshes.yellowLight != null && meshes.greenLight != null)
                {
                    lightMeshCache[trafficLight] = meshes;
                }
                else
                {
                    Debug.LogWarning($"TrafficLightController: Could not find all light meshes for {trafficLight.name}. Red: {meshes.redLight != null}, Yellow: {meshes.yellowLight != null}, Green: {meshes.greenLight != null}");
                }
            }
            else
            {
                Debug.LogWarning($"TrafficLightController: Could not find Traffic_light_EU in {trafficLight.name}");
            }
        }
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    Renderer FindLightMesh(Transform parent, string color)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(color))
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer == null)
                {
                    Debug.LogWarning($"TrafficLightController: Found {child.name} but it has no Renderer component!");
                }
                return renderer;
            }
        }
        return null;
    }

    void CreateOffMaterial()
    {
        if (redLightMaterial == null || redLightMaterial.shader == null)
        {
            Debug.LogError("TrafficLightController: Cannot create off material - redLightMaterial or shader is null!");
            return;
        }

        offLightMaterial = new Material(redLightMaterial.shader);
        offLightMaterial.name = "TrafficLightOff (Auto)";
        if (offLightMaterial.HasProperty("_BaseColor"))
        {
            offLightMaterial.SetColor("_BaseColor", Color.black);
        }
        if (offLightMaterial.HasProperty("_EmissionColor"))
        {
            offLightMaterial.SetColor("_EmissionColor", Color.black);
        }
        if (offLightMaterial.HasProperty("_EmissionIntensity"))
        {
            offLightMaterial.SetFloat("_EmissionIntensity", 0f);
        }
        offLightMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
    }

    void OnDestroy()
    {
        foreach (var materialInstance in materialInstanceCache.Values)
        {
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
        }
        materialInstanceCache.Clear();
    }

    void SetAllLightsRed()
    {
        SetLightsState(northSouthLights, LightState.Red);
        SetLightsState(eastWestLights, LightState.Red);
    }

    void SetLightsGreen()
    {
        if (eastWestLights == null || eastWestLights.Count == 0)
        {
            SetLightsState(northSouthLights, LightState.Green);
        }
        else if (northSouthIsGreen)
        {
            SetLightsState(northSouthLights, LightState.Green);
            SetLightsState(eastWestLights, LightState.Red);
        }
        else
        {
            SetLightsState(northSouthLights, LightState.Red);
            SetLightsState(eastWestLights, LightState.Green);
        }
    }

    void SetLightsYellow()
    {
        if (eastWestLights == null || eastWestLights.Count == 0)
        {
            SetLightsState(northSouthLights, LightState.Yellow);
        }
        else if (northSouthIsGreen)
        {
            SetLightsState(northSouthLights, LightState.Yellow);
            SetLightsState(eastWestLights, LightState.Red);
        }
        else
        {
            SetLightsState(northSouthLights, LightState.Red);
            SetLightsState(eastWestLights, LightState.Yellow);
        }
    }

    void SetLightsState(List<GameObject> trafficLights, LightState state)
    {
        foreach (GameObject trafficLight in trafficLights)
        {
            if (trafficLight == null || !lightMeshCache.ContainsKey(trafficLight))
            {
                Debug.LogWarning($"TrafficLightController: Traffic light {trafficLight?.name} not found in cache!");
                continue;
            }

            LightMeshes meshes = lightMeshCache[trafficLight];

            SetLightMaterial(meshes.redLight, offLightMaterial, "red");
            SetLightMaterial(meshes.yellowLight, offLightMaterial, "yellow");
            SetLightMaterial(meshes.greenLight, offLightMaterial, "green");

            Renderer activeLight = null;
            Material activeMaterial = null;

            switch (state)
            {
                case LightState.Red:
                    activeLight = meshes.redLight;
                    activeMaterial = redLightMaterial;
                    break;
                case LightState.Yellow:
                    activeLight = meshes.yellowLight;
                    activeMaterial = yellowLightMaterial;
                    break;
                case LightState.Green:
                    activeLight = meshes.greenLight;
                    activeMaterial = greenLightMaterial;
                    break;
            }

            if (activeLight != null && activeMaterial != null)
            {
                SetLightMaterial(activeLight, activeMaterial, state.ToString().ToLower());
            }
            else
            {
                Debug.LogWarning($"TrafficLightController: Active light or material is null for {trafficLight.name} in state {state}");
            }
        }
    }

    void SetLightMaterial(Renderer renderer, Material material, string lightName)
    {
        if (renderer == null)
        {
            Debug.LogWarning($"TrafficLightController: Renderer is null for {lightName} light!");
            return;
        }

        if (material == null)
        {
            Debug.LogWarning($"TrafficLightController: Material is null for {lightName} light!");
            return;
        }

        renderer.enabled = true;

        Material materialInstance;
        if (!materialInstanceCache.ContainsKey(renderer))
        {
            materialInstance = new Material(material);
            materialInstance.name = material.name + " (Instance)";
            materialInstanceCache[renderer] = materialInstance;
        }
        else
        {
            materialInstance = materialInstanceCache[renderer];
        }

        materialInstance.CopyPropertiesFromMaterial(material);

        materialInstance.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        materialInstance.EnableKeyword("_EMISSION");

        renderer.material = materialInstance;

        renderer.UpdateGIMaterials();

        DynamicGI.UpdateEnvironment();

        if (renderer.material == null || renderer.material.shader != material.shader)
        {
            Debug.LogWarning($"TrafficLightController: Material assignment failed for {renderer.gameObject.name}! Expected shader: {material.shader.name}, Got: {renderer.material?.shader.name}");
        }
    }

    public bool IsGreenFor(StopPoint.Direction dir)
    {

        bool anyGreen = (currentState == LightState.Green);

        if (!anyGreen) return false;

        if (dir == StopPoint.Direction.NorthSouth)
            return northSouthIsGreen;
        else
            return !northSouthIsGreen;
    }

    public bool IsRedFor(StopPoint.Direction dir)
    {
        return !IsGreenFor(dir);
    }


}
